﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using PacketDotNet;
using Serilog;
using SharpPcap;
using SharpPcap.LibPcap;

namespace DNToolKit.Sniffer;

public class Sniffer
{

    private LibPcapLiveDevice _pcapDevice;
    private UdpHandler _udpHandler;

    private object lockObject = new();

    public void OnPacketArrival(object sender, PacketCapture e)
    {
        _udpHandler.HandleRawCapture(e.GetPacket());
    }

    public void Start()
    {
        _udpHandler = new UdpHandler();
    }
    
    public void AddPcap(string fileName)
    {

        lock (lockObject)
        {
            //i dont like this but the library calls for it...


            var pcap = new CaptureFileReaderDevice(fileName);

            Log.Information(DateTime.Now.ToString("hh:mm:ss t z"));
            List<RawCapture> bs = new();
        
            pcap.OnPacketArrival += delegate(object sender, PacketCapture capture)
            {
                bs.Add(capture.GetPacket());
            };
            
            pcap.Open();
            pcap.Filter = "udp portrange 22101-22102";

            //this returns when EOF
            pcap.Capture();
            pcap.Close();
            //i guessed this lmfao
            bs.Sort((x, y)=>x.Timeval.Date.Ticks < y.Timeval.Date.Ticks ? -1 : 1);


            var count = 0;
            Log.Information("Found {amt} raw packets", bs.Count);
            var stop = new Stopwatch();
            stop.Start();
            foreach (var rawCapture in bs)
            {
                
                //slay

                var udpPacket = PacketDotNet.Packet.ParsePacket(LinkLayers.Ethernet,
                        rawCapture.Data)
                    .Extract<IPv4Packet>()
                    .Extract<UdpPacket>();
                var packetBytes = udpPacket.PayloadData;
                
                

                count++;
                _udpHandler.HandleRawCapture(rawCapture);
                //first 50 ish packets hopefully has tokenrsp in it
                if (count == 500)
                {
                    // count = 0;
                    // Task.Yield();// what the fuck?
                    // i really fucking hate this
                    // Task.Delay(1).Wait();
                    // Log.Information("Waiting...");
                }
            }
            stop.Stop();

            if (bs.Count < 1)
            {
                return;
            }
        }
    }//>DNTKCLI.exe "C:\Users\admin\Documents\Github\DNToolKit\DNToolKit\bin\Debug\net6.0\Captures\2.8_9-08-2022_02-07-21.pcap" "aafaf.dntkap"
    
    public void Close()
    {

        _udpHandler.Close();
        
    }


    // taken from devove's proj
    private static LibPcapLiveDevice GetPcapDevice()
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (PcapInterface allPcapInterface in PcapInterface.GetAllPcapInterfaces())
        {
            var friendlyName = allPcapInterface.FriendlyName ?? string.Empty;
            if (friendlyName.ToLower().Contains("loopback") || friendlyName is "any" or "virbr0-nic" ||friendlyName.ToLower().Contains("wsl") ) continue;
            
            var networkInterface = networkInterfaces.FirstOrDefault(ni => ni.Name == friendlyName);
            if ((networkInterface != null ? (networkInterface.OperationalStatus != OperationalStatus.Up ? 1 : 0) : 1) !=
                0) continue;
            
            using (var device = new LibPcapLiveDevice(allPcapInterface))
            {
                LinkLayers linkType;
                try
                {
                    device.Open();
                    linkType = device.LinkType;
                }
                catch (PcapException ex)
                {
                    Console.WriteLine(ex);
                    continue;
                }
                if (linkType == LinkLayers.Ethernet)
                    return device;
            }
        }
        throw new InvalidOperationException("No ethernet pcap supported devices found, are you running as a user with access to adapters (root on Linux)?");
    }
}