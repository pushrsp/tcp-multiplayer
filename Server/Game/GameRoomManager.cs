using System.Collections.Generic;

public class GameRoomManager
{
    public static GameRoomManager Instance { get; } = new GameRoomManager();

    private object _lock = new object();
    private Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
    private int _roomId = 1;

    public GameRoom Add(int mapId)
    {
        GameRoom room = new GameRoom();
        room.Init(mapId);

        lock (_lock)
        {
            room.RoomId = _roomId++;
            _rooms.Add(room.RoomId, room);
        }

        return room;
    }

    public GameRoom Find(int roomId)
    {
        lock (_lock)
        {
            GameRoom room;
            if (_rooms.TryGetValue(roomId, out room))
                return room;

            return null;
        }
    }
}