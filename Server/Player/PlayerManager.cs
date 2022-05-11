using System.Collections.Generic;

public class PlayerManager
{
    public static PlayerManager Instance { get; } = new PlayerManager();

    private int _playerId = 1;
    private object _lock = new object();
    private Dictionary<int, Player> _players = new Dictionary<int, Player>();

    public Player Add()
    {
        Player player = new Player();
        lock (_lock)
        {
            player.Info.PlayerId = _playerId++;
            _players.Add(player.Info.PlayerId, player);
        }

        return player;
    }
}