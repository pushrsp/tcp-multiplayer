using System;
using System.IO;
using Google.Protobuf.Protocol;
// ReSharper disable All

public struct Vector3Int
{
    public int y;
    public int z;
    public int x;

    public Vector3Int(int y, int z, int x)
    {
        this.y = y;
        this.z = z;
        this.x = x;
    }
    
    public float magnitude
    {
        get => MathF.Sqrt(y * y + z * z + x * x);
    }
        
    public static Vector3Int up
    {
        get => new Vector3Int(1,0,0);
    }

    public static Vector3Int down
    {
        get => new Vector3Int(-1,0,0);
    }
        
    public static Vector3Int forward
    {
        get => new Vector3Int(0,1,0);
    }

    public static Vector3Int back
    {
        get => new Vector3Int(0,-1,0);
    }

    public static Vector3Int left
    {
        get => new Vector3Int(0,0,-1);
    }

    public static Vector3Int right
    {
        get => new Vector3Int(0,0,1);
    }

    public static Vector3Int operator +(Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(a.y+b.y, a.z+b.z,a.x+b.x);
    }
    
    public static Vector3Int operator -(Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(a.y-b.y, a.z-b.z,a.x-b.x);
    }
}

public class Map
{
    public int MinY { get; set; }
    public int MaxY { get; set; }

    public int MinZ { get; set; }
    public int MaxZ { get; set; }

    public int MinX { get; set; }
    public int MaxX { get; set; }

    private int YCount { get; set; }
    private int ZCount { get; set; }
    private int XCount { get; set; }
    
    private char[,,] _collision;
    private Player[,,] _players;

    public void LoadMap(int mapId,string pathPrefix)
    {
        string stageName = "Stage_" + mapId.ToString("000");
        string text = File.ReadAllText($"{pathPrefix}/{stageName}/{stageName}_Info.txt");
        
        StringReader reader = new StringReader(text);

        MinY = int.Parse(reader.ReadLine());
        MaxY = int.Parse(reader.ReadLine());

        MinZ = int.Parse(reader.ReadLine());
        MaxZ = int.Parse(reader.ReadLine());

        MinX = int.Parse(reader.ReadLine());
        MaxX = int.Parse(reader.ReadLine());
        
        YCount = MaxY - MinY + 1;
        ZCount = MaxZ - MinZ - 1;
        XCount = MaxX - MinX;
        
        _collision = new char[YCount, ZCount, XCount];
        _players = new Player[YCount, ZCount, XCount];

        for (int y = 0; y < YCount; y++)
        {
            string collision = File.ReadAllText($"{pathPrefix}/{stageName}/{stageName}_Collision_{y}.txt");
            StringReader colReader = new StringReader(collision);

            for (int z = 0; z < ZCount; z++)
            {
                string line = colReader.ReadLine();
                for (int x = 0; x < XCount; x++)
                {
                    _collision[y, z, x] = line[x];
                }
            }
        }
    }

    private bool CheckCollision(int y, int z, int x)
    {
        switch (_collision[y, z, x])
        {
            case '0':
                return true;
            case '1':
                return false;
            case '2':
                return true;
            case '3':
                return false;
            default:
                return false;
        }
    }

    private bool CheckPlayer(int y, int z, int x)
    {
        return _players[y, z, x] == null;
    }
    
    public bool CanGo(Vector3Int pos)
    {
        if (pos.x < MinX || pos.x > MaxX)
            return false;
        if (pos.z < MinZ)
            return false;
        if (pos.y < MinY)
            return false;

        int y = pos.y - MinY;
        int z = MaxZ - pos.z;
        int x = MaxX - pos.x;

        return CheckCollision(y, z, x) && CheckPlayer(y, z, x);
    }

    public bool ApplyMove(Player player, Vector3Int destPos)
    {
        PositionInfo posInfo = player.Info.PosInfo;
        if (posInfo.PosX < MinX || posInfo.PosX > MaxX)
            return false;
        if (posInfo.PosZ < MinZ || posInfo.PosZ > MaxZ)
            return false;
        if (posInfo.PosY < MinY || posInfo.PosY > MaxY)
            return false;

        if (CanGo(destPos) == false)
            return false;

        {
            int y = posInfo.PosY - MinY;
            int z = MaxZ - posInfo.PosZ;
            int x = MaxX - posInfo.PosX;

            if (_players[y, z, x] == player)
                _players[y, z, x] = null;
        }

        {
            int y = destPos.y - MinY;
            int z = MaxZ - destPos.z;
            int x = MaxX - destPos.x;

            _players[y, z, x] = player;

            if (_collision[y, z, x] == '2')
            {
                player.Info.Speed = 5.0f;
            }
            else
            {
                player.Info.Speed = 13.0f;
            }
        }

        posInfo.PosY = destPos.y;
        posInfo.PosZ = destPos.z;
        posInfo.PosX = destPos.x;
        
        return true;
    }
}