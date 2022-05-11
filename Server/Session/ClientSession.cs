using System;
using System.Net;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;

namespace Server
{
    public class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public Player MyPlayer { get; set; }

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
            MyPlayer = PlayerManager.Instance.Add();
            MyPlayer.Session = this;

            GameRoomManager.Instance.Find(1).EnterGame(MyPlayer);
        }

        public override void OnSend(int numOfBytes)
        {
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            GameRoomManager.Instance.Find(1).LeaveGame(MyPlayer.Info.PlayerId);
        }

        public override void OnRecvMsg(ArraySegment<byte> segment)
        {
            PacketManager.Instance.OnRecvMsg(this, segment);
        }
    }
}