using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;

// ReSharper disable All

public class PacketHandler
{
    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.HandleMove(player, movePacket);
    }
}