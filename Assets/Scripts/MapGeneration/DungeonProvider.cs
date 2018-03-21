using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonProvider : MonoBehaviour
{
    public event Action BoardIsReady;

    public struct Tcoord
    {
        public int X;
        public int Y;

        public Tcoord(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [SerializeField]
    private int _width;

    public int Width
    {
        get { return _width; }
        set { _width = value; }
    }

    [SerializeField]
    private int _height;

    public int Height
    {
        get { return _height; }
        set { _height = value; }
    }

    [Range(0, 100)]
    public int randomFillPercent;
    public int passRadius;

    private byte[,] _board;

    private bool _isNotGenerated = true;

    public List<Tcoord> FreeTilesList { get; private set; }


    public byte[,] GetBoard()
    {
        return _board;
    }

    void Start()
    {
        var start = DateTime.UtcNow;
        FreeTilesList = new List<Tcoord>();
        while (_isNotGenerated)
        {
            GenerateMap();
        }
        print("MapGenerated: " + Math.Abs(start.Subtract(DateTime.UtcNow).TotalSeconds));
        AddFreeTiles();
        BoardIsReady();
    }

    void Update()
    {
    }

    private void GenerateMap()
    {
        _board = new byte[Width, Height];
        RandomFillMap();
        if (CheckOnFreeSpace())
        {
            SmoothMap(3);
            ProcessMap();
            SmoothMap(3);
            RemoveThinWalls();
            SmoothMap(3);
            CloseBorders();
        }
        else
        {
            randomFillPercent -= 5;
            _isNotGenerated = true;
        }
    }

    private void AddFreeTiles()
    {
        for (var i = 0; i < Width; i++)
        {
            for (var j = 0; j < Height; j++)
            {
                if (_board[i, j] == 0)
                    FreeTilesList.Add(new Tcoord(i, j));
            }
        }
    }

    private bool CheckOnFreeSpace()
    {
        for (var i = 0; i < Width; i++)
        {
            for (var j = 0; j < Height; j++)
            {
                if (_board[i, j] == 0) return true;
            }
        }
        return false;
    }

    private void CloseBorders()
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                if ((x == 0 || x == Width - 1 || x == Width - 2 || x == 1
                    || y == 0 || y == 1 || y == Height - 1 || y == Height - 2)
                    && _board[x, y] == 0)
                {
                    _board[x, y] = 1;
                }
            }
        }
    }

    private void RemoveThinWalls()
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                if (NotOnBorder(x, y) && _board[x, y - 1] == 1
                                      && _board[x + 1, y] == 1
                                      && _board[x + 1, y - 1] == 0
                                      && _board[x, y + 1] == 0)
                {
                    ClearPass(new Tcoord(x, y), passRadius);
                }
                else if (NotOnBorder(x, y) && _board[x, y - 1] == 1
                                    && _board[x - 1, y] == 1
                                    && _board[x - 1, y - 1] == 0
                                    && _board[x + 1, y + 1] == 0)
                {
                    ClearPass(new Tcoord(x, y), passRadius);
                }
                else if (NotOnBorder(x, y) && _board[x, y - 1] == 1
                                           && _board[x + 1, y] == 1 && _board[x + 1, y - 1] == 0
                                           && _board[x - 1, y + 1] == 0)
                {
                    ClearPass(new Tcoord(x, y), passRadius);
                }
                else if (NotOnBorder(x, y) && _board[x, y - 1] == 1
                                           && _board[x - 1, y] == 1 && _board[x - 1, y - 1] == 0
                                           && _board[x, y + 1] == 0)
                {
                    ClearPass(new Tcoord(x, y), passRadius);
                }
                else if (NotOnBorder(x, y) && _board[x, y - 1] == 1
                                           && _board[x, y + 1] == 1
                                           && _board[x - 1, y] == 0 && _board[x + 1, y] == 0)
                {
                    ClearPass(new Tcoord(x, y), passRadius);
                }
                else if (NotOnBorder(x, y) && _board[x, y - 1] == 0
                                           && _board[x, y + 1] == 0
                                           && _board[x - 1, y] == 1 && _board[x + 1, y] == 1)
                {
                    ClearPass(new Tcoord(x, y), passRadius);
                }
            }
        }
    }

    private void ProcessMap()
    {
        const int wallThresholdSize = 3;
        const int roomThresholdSize = 2;
        var wallRegions = GetRegions(1);

        var roomRegions = GetRegions(0);
        var finalRooms = new List<Room>();

        foreach (var wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Tcoord tile in wallRegion)
                {
                    _board[tile.X, tile.Y] = 0;
                }
            }
        }

        foreach (var roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (var tile in roomRegion)
                {
                    _board[tile.X, tile.Y] = 1;
                }
            }
            else
            {
                finalRooms.Add(new Room(roomRegion, _board));
            }
        }

        finalRooms.Sort();
        if (finalRooms.Count < 1 || finalRooms[0].roomSize < 20)
        {
            randomFillPercent -= 5;
            _isNotGenerated = true;
        }
        else
        {
            finalRooms[0].isMainRoom = true;
            finalRooms[0].isAccessibleFromMainRoom = true;

            ConnectClosestRooms(finalRooms);
            ConnectRandomRooms(finalRooms);
            _isNotGenerated = false;
        }
    }

    private void ConnectRandomRooms(List<Room> allRooms)
    {
        for (var i = 0; i < 3; i++)
        {
            foreach (var room in allRooms)
            {
                if (Random.Range(0, 100) < 50)
                {
                    room.isAccessibleFromMainRoom = false;
                }
            }
            ConnectClosestRooms(allRooms);
        }
    }

    private void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibility = false)
    {

        var roomListA = new List<Room>();
        var roomListB = new List<Room>();

        if (forceAccessibility)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        var bestDistance = 0;
        var bestTileA = new Tcoord();
        var bestTileB = new Tcoord();
        var bestRoomA = new Room();
        var bestRoomB = new Room();
        var possibleConFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibility)
            {
                possibleConFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (var roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                foreach (var roomAtile in roomA.edgeTiles)
                {
                    foreach (var roomBtile in roomB.edgeTiles)
                    {
                        var tileA = roomAtile;
                        var tileB = roomBtile;
                        var distanceBetweenRooms = (int)(Mathf.Pow(tileA.X - tileB.X, 2) + Mathf.Pow(tileA.Y - tileB.Y, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if (possibleConFound && !forceAccessibility)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConFound && forceAccessibility)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibility)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    private void CreatePassage(Room roomA, Room roomB, Tcoord tileA, Tcoord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        var line = GetLine(tileA, tileB);
        foreach (var c in line)
        {
            ClearPass(c, passRadius);
        }
    }

    private void ClearPass(Tcoord c, int r)
    {
        for (var x = -r; x <= r; x++)
        {
            for (var y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    var drawX = c.X + x;
                    var drawY = c.Y + y;
                    if (IsOnBoard(drawX, drawY))
                    {
                        _board[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    private List<Tcoord> GetLine(Tcoord from, Tcoord to)
    {
        var line = new List<Tcoord>();

        var x = from.X;
        var y = from.Y;

        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        var inverted = false;
        var step = Math.Sign(dx);
        var gradientStep = Math.Sign(dy);

        var longest = Mathf.Abs(dx);
        var shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        var gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Tcoord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }
        return line;
    }

    private List<List<Tcoord>> GetRegions(int tileType)
    {
        var regions = new List<List<Tcoord>>();
        var boardFlags = new int[Width, Height];

        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                if (boardFlags[x, y] == 0 && _board[x, y] == tileType)
                {
                    var newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (var tile in newRegion)
                    {
                        boardFlags[tile.X, tile.Y] = 1;
                    }
                }
            }
        }
        return regions;
    }

    private List<Tcoord> GetRegionTiles(int startX, int startY)
    {
        var tiles = new List<Tcoord>();
        var boardFlags = new int[Width, Height];
        var tileType = _board[startX, startY];

        var queue = new Queue<Tcoord>();
        queue.Enqueue(new Tcoord(startX, startY));
        boardFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            var tile = queue.Dequeue();
            tiles.Add(tile);

            for (var x = tile.X - 1; x <= tile.X + 1; x++)
            {
                for (var y = tile.Y - 1; y <= tile.Y + 1; y++)
                {
                    if (IsOnBoard(x, y) && (y == tile.Y || x == tile.X))
                    {
                        if (boardFlags[x, y] == 0 && _board[x, y] == tileType)
                        {
                            boardFlags[x, y] = 1;
                            queue.Enqueue(new Tcoord(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    private bool IsOnBoard(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    private bool NotOnBorder(int x, int y)
    {
        return y != 0 && x != 0 && y != Height - 1 && x != Width - 1;
    }

    private void RandomFillMap()
    {
        var pseudoRandom = new System.Random();

        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                {
                    _board[x, y] = 1;
                }
                else
                {
                    _board[x, y] = (byte)((pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0);
                }
            }
        }
    }

    private void SmoothMap(int n)
    {
        for (var i = 0; i < n; i++)
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    int neighbourWallTiles = GetSurroundingWallCount(x, y);

                    if (neighbourWallTiles > 4)
                        _board[x, y] = 1;
                    else if (neighbourWallTiles < 4)
                        _board[x, y] = 0;
                }
            }
        }
    }

    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        var wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsOnBoard(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += _board[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }


    // For debug purpose

    //void OnDrawGizmos () {
    //    if (board != null) {
    //        for (int x = 0; x < width; x++) {
    //            for (int y = 0; y < height; y++) {
    //                if (board[x, y] == 1) {
    //                    Gizmos.color = Color.black;
    //                    Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
    //                    Gizmos.DrawCube(pos, Vector3.one);
    //                }
    //            }
    //        }
    //    }
    //}
}
