using System.Diagnostics;
using System.Text.Json;
using Common;
using DNToolKit.Frontend;
using Newtonsoft.Json;
using Serilog;

namespace DNToolKit;

public class Program
{

    public static OutputManager OutputManager = null!;
    public static Config Config = null!;
    public static Sniffer.Sniffer Sniffer = null!;

    public static ushort GameMajorVersion = 3;
    public static ushort GameMinorVersion = 1;
    

    private static string _configName = "./config.json";
    private static TaskCompletionSource tcs = new TaskCompletionSource();

    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateLogger();
        Log.Information("DNToolKit CLI for v3.0");
        if (args.Length < 2)
        {
            Log.Information("Usage: DNTKCLI.exe [InputFileName] [OutputFileName]");
            return;
        }
        if ((args[0] is null) || (args[1] is null))
        {
            Log.Information("Usage: DNTKCLI.exe [InputFileName] [OutputFileName]");
            return;
        }
        // var key = KeyBruteForcer.BruteForce(senttime: 1658814410247, serverKey: 4502709363913224634, testBuffer: new byte[] { 0x0B, 0xB9});
        //
        // return;
        Config = Config.Default;
        

        Sniffer = new Sniffer.Sniffer();




        //ugh figure out what to rename the sniffer namespace 
        
        OutputManager = new OutputManager(args[1]);
        Sniffer.Start();

        ProtobufFactory.Initialize();

        Log.Information("Ready! Hit Control + C to stop...");
        
        Task.Run(() =>
        {
            Sniffer.AddPcap(args[0]);
        }).Wait();
        

        //Capture.ParseFromBytes(File.ReadAllBytes("./Captures/"));

        // Console.CancelKeyPress  += Close;
        // AppDomain.CurrentDomain.ProcessExit += Close;

    }
    public static uint Now()
    {
        return Convert.ToUInt32((DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks) / 10000000);
    }
    private static void Close(object? sender, ConsoleCancelEventArgs e)
    {        
        e.Cancel = true;
        Stop();
    }

    public static void Stop()
    {
        Sniffer.Close();
        OutputManager.Close();
        Log.Information("Finished cleaning up...");    
        tcs.SetResult();
    }
}