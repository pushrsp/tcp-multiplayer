using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;

public class GameRoom
{
    public int RoomId { get; set; }

    private Dictionary<int, Player> _players = new Dictionary<int, Player>();
    private object _lock = new object();
    private int xOffset = -73;

    private Map _map = new Map();

    public void Init(int mapId)
    {
        _map.LoadMap(mapId, "../../../../Common/MapData");
    }

    public void EnterGame(Player player)
    {
        if (player == null)
            return;

        lock (_lock)
        {
            player.Room = this;
            player.Info.PosInfo.PosY = 0;
            player.Info.PosInfo.PosZ = -160;
            player.Info.PosInfo.PosX = xOffset;
            player.Info.Speed = 13.0f;
            player.Info.PosInfo.State = CreatureState.Idle;

            xOffset += 10;

            _players.Add(player.Info.PlayerId, player);

            {
                S_EnterGame enterGamePacket = new S_EnterGame();
                enterGamePacket.PlayerInfo = player.Info;
                player.Session.Send(enterGamePacket);

                S_Spawn spawnPacket = new S_Spawn();
                foreach (Player p in _players.Values)
                {
                    if (p != player)
                        spawnPacket.Players.Add(p.Info);
                }

                player.Session.Send(spawnPacket);
            }

            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Players.Add(player.Info);
                foreach (Player p in _players.Values)
                {
                    if (p != player)
                        p.Session.Send(spawnPacket);
                }
            }
        }
    }

    public void LeaveGame(int playerId)
    {
        lock (_lock)
        {
            Player player;
            if (_players.TryGetValue(playerId, out player) == false)
                return;

            _players.Remove(playerId);

            {
                player.Room = null;
                player.Session = null;
                player.Info = null;
            }

            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.PlayerId = playerId;

                foreach (Player p in _players.Values)
                    p.Session.Send(despawnPacket);
            }
        }
    }

    public void HandleMove(Player player, C_Move packet)
    {
        if (player == null)
            return;

        lock (_lock)
        {
            PositionInfo movePosInfo = packet.PosInfo;
            PlayerInfo playerInfo = player.Info;

            if (movePosInfo.PosY != playerInfo.PosInfo.PosY ||
                movePosInfo.PosZ != playerInfo.PosInfo.PosZ ||
                movePosInfo.PosX != playerInfo.PosInfo.PosX)
            {
                if (_map.CanGo(new Vector3Int(movePosInfo.PosY, movePosInfo.PosZ, movePosInfo.PosX)) == false)
                    return;
            }

            playerInfo.PosInfo.State = movePosInfo.State;
            _map.ApplyMove(player, new Vector3Int(movePosInfo.PosY, movePosInfo.PosZ, movePosInfo.PosX));

            S_Move movePacket = new S_Move
            {
                PlayerInfo = new PlayerInfo
                {
                    PosInfo = new PositionInfo()
                }
            };

            movePacket.PlayerInfo.Speed = player.Info.Speed;
            movePacket.PlayerInfo.PlayerId = player.Info.PlayerId;
            movePacket.PlayerInfo.PosInfo = playerInfo.PosInfo;

            BroadCast(movePacket);
        }
    }

    private void BroadCast(IMessage packet)
    {
        lock (_lock)
        {
            foreach (Player p in _players.Values)
                p.Session.Send(packet);
        }
    }
}