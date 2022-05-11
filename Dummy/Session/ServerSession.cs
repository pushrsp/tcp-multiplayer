using System;
using System.Net;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;

namespace Dummy
{
    public class ServerSession : PacketSession
    {
        public void Send(IMessage packet)
        {
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
            MsgId msgId = (MsgId) Enum.Parse(typeof(MsgId), msgName);

            ushort size = (ushort) packet.CalculateSize();
            byte[] sendBuff = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort) size + 4), 0, sendBuff, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort) msgId), 0, sendBuff, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuff, 4, size);

            Send(new ArraySegment<byte>(sendBuff));
        }

        public override void OnConnected(EndPoint endPoint)
        {
            C_Chat chatPacket = new C_Chat();
            chatPacket.Chat = "HI";
            Send(chatPacket);
        }

        public override void OnSend(int numOfBytes)
        {
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
        }

        public override void OnRecvMsg(ArraySegment<byte> segment)
        {
            ClientPacketManager.Instance.OnRecvMsg(this, segment);
        }
    }
}