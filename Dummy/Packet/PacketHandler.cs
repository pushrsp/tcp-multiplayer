using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;

public class PacketHandler
{
    public static void S_ChatHandler(PacketSession session, IMessage packet)
    {
        S_Chat chatPacket = packet as S_Chat;

        Console.WriteLine($"({chatPacket.PlayerId}) ({chatPacket.Chat})");
    }
}