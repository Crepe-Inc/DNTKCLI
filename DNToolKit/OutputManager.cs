// using System.Text.Json;


using System.Text;
using System.Text.Unicode;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;

namespace DNToolKit.Frontend;


//i could honestly make this a static class but adding more keywords is annoying

public class OutputManager
{
    private StreamWriter file;
    JsonSerializer b = JsonSerializer.Create(new JsonSerializerSettings());
    
    public void AddGamePacket(Packet.Packet packet)
    {
        // b.Serialize(file, packet.GetObj());
        var dat = JsonConvert.SerializeObject(packet.GetObj());
        if (dat.Length <= 10)
        {
            Console.WriteLine(Enum.GetName(packet.PacketType));
            return;
        }

        var strBytes = Encoding.UTF8.GetBytes(dat);
        file.Write(Convert.ToBase64String(strBytes));
        file.Write("█▄█\n");
        
    }

    public OutputManager(string outname)
    {
        this.file = File.AppendText(outname);
        b.Converters.Add(new StringEnumConverter());
    }

    public void Close()
    {
        file.Flush();
        file.Close();
    }

}