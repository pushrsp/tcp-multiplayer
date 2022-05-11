using Google.Protobuf.Protocol;
using Server;

public class Player
{
    public ClientSession Session { get; set; }
    public GameRoom Room { get; set; }
    public PlayerInfo Info { get; set; } = new PlayerInfo {PosInfo = new PositionInfo()};
}