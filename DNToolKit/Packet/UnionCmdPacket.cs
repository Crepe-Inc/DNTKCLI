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
    public override Dictionary<string, object> GetObj()
    {
        Dictionary<string, object> jsonobj = base.GetObj();
        jsonobj.Add("data", DummyPacketData);
        return jsonobj;



        return null;
    }
}