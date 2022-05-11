using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;

// ReSharper disable All

public class ClientPacketManager
{
    public static ClientPacketManager Instance { get; } = new ClientPacketManager();

    private Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv =
        new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();

    private Dictionary<ushort, Action<PacketSession, IMessage>> _handlers =
        new Dictionary<ushort, Action<PacketSession, IMessage>>();

    public Action<PacketSession, IMessage, ushort> CustomerHandler { get; set; }

    public ClientPacketManager()
    {
        Register();
    }

    private void Register()
    {
        _onRecv.Add((ushort) MsgId.SChat, MakePacket<S_Chat>);
        _handlers.Add((ushort) MsgId.SChat, PacketHandler.S_ChatHandler);
    }

    public void OnRecvMsg(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>, ushort> action;
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer, id);
    }

    private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
    {
        T packet = new T();
        packet.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

        if (CustomerHandler != null)
        {
            CustomerHandler.Invoke(session, packet, id);
        }
        else
        {
            Action<PacketSession, IMessage> action;
            if (_handlers.TryGetValue(id, out action))
                action.Invoke(session, packet);
        }
    }
}