using DNToolKit.Frontend;
using DNToolKit.PacketProcessors;

namespace DNToolKit.Packet;



public class UnionCmdPacket : Packet
{
    public class OnionCmd
    {
        public class Cmd
        {
            public uint MessageId;
            //heres where we put 
            public object Body;
        }

        public Cmd[] CmdList;
    }

    public OnionCmd DummyPacketData;
    public override object? GetObj()
    {
        Dictionary<string, object> jsonobj = new();
        jsonobj.Add("PacketHead", Metadata);
        jsonobj.Add("PacketData", DummyPacketData);
        jsonobj.Add("CmdID", PacketType.ToString());
        jsonobj.Add("Sender", (int)Sender);

        return jsonobj;



        return null;
    }
}