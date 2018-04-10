using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonProvider : MapProvider
{
    /// <summary>
    ///     Action called when map is ready
    /// </summary>
    public event Action MapIsReady;

    public int Width { get; set; }

    public int Height { get; set; }

    public bool[,] Map { get; set; }

    public int RandomFillPercent { get; set; }

    public int PassRadius { get; set; }

    private List<Tile> _freeTileList;

    /// <summary>
    ///     Provider Constructor
    /// </summary>
    /// <param name="height">Map height</param>
    /// <param name="width">Map width</param>
    /// <param name="randomFillPercent">For random map filling. Bigger value means bigger chance to place dead cells</param>
    /// <param name="passRadius">Radius for creating passages between rooms</param>
    public DungeonProvider(int height, int width, int randomFillPercent, int passRadius)
    {
        Height = height;
        Width = width;
        RandomFillPercent = randomFillPercent;
        PassRadius = passRadius;
    }

    /// <summary>
    ///     Starts to generate map and checks map consistency
    /// </summary>
    public void GenerateMap()
    {
        while (true)
        {
            Map = new bool[Width, Height];
            RandomFillMap();
            SmoothMap(3);
            ProcessMap();
            SmoothMap(3);
            RemoveThinWalls();
            CloseBorders();
            if (GetFreeTiles().Count < 25) // Checking if proper map was generated
            {
                Width += 5;
                Height += 5;
                RandomFillPercent = RandomFillPercent > 15 ? RandomFillPercent - 10 : RandomFillPercent + 10;
                _freeTileList = null;
                continue;
            }

            MapIsReady();
            break;
        }
    }


    /// <summary>
    ///     To prevent wholes on map borders
    /// </summary>
    private void CloseBorders()
    {
        for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
                if ((x == 0 || x == Width - 1 || x == Width - 2 || x == 1
                     || y == 0 || y == 1 || y == Height - 1 || y == Height - 2)
                    && Map[x, y])
                    Map[x, y] = false;
    }

    /// <summary>
    ///     Counts amount of live cells
    /// </summary>
    /// <returns>List of live cells</returns>
    public List<Tile> GetFreeTiles()
    {
        if (_freeTileList == null)
        {
            _freeTileList = new List<Tile>();
            for (var i = 1; i < Width - 1; i++)
                for (var j = 1; j < Height - 1; j++)
                    if (Map[i, j])
                        _freeTileList.Add(new Tile(i, j));
        }

        return _freeTileList;
    }

    /// <summary>
    ///     Recursevly removes inconsistent dead tiles till no more left
    /// </summary>
    private void RemoveThinWalls()
    {
        var inconsistentTileDetected = false;
        for (var x = 1; x < Width - 1; x++)
            for (var y = 1; y < Height - 1; y++)
                if (!Map[x, y]
                    && ((Map[x + 1, y + 1] && Map[x - 1, y - 1]) // pattern -> /
                        || (Map[x - 1, y + 1] && Map[x + 1, y - 1]) // pattern -> \
                        || (Map[x + 1, y] && Map[x - 1, y]) // pattern -> -
                        || (Map[x, y + 1] && Map[x, y - 1]) // pattern -> |
                    ))
                {
                    Map[x, y] = true;
                    inconsistentTileDetected = true;
                }

        if (inconsistentTileDetected)
        {
            RemoveThinWalls();
        }
    }

    /// <summary>
    ///     Processes the map after creating regions
    /// </summary>
    private void ProcessMap()
    {
        const int wallThresholdSize = 3;
        const int roomThresholdSize = 2;
        var wallRegions = GetRegions(false);

        var roomRegions = GetRegions(true);
        var finalRooms = new List<Room>();

        foreach (var wallRegion in wallRegions)
            if (wallRegion.Count < wallThresholdSize)
                foreach (var tile in wallRegion)
                    Map[tile.X, tile.Y] = true;

        foreach (var roomRegion in roomRegions)
            if (roomRegion.Count < roomThresholdSize)
                foreach (var tile in roomRegion)
                    Map[tile.X, tile.Y] = false;
            else
                finalRooms.Add(new Room(roomRegion, Map));

        finalRooms.Sort();

        if (finalRooms.Count < 1)
            return;

        finalRooms[0].isMainRoom = true;
        finalRooms[0].isAccessibleFromMainRoom = true;

        ConnectClosestRooms(finalRooms);
        ConnectRandomRooms(finalRooms);
    }

    /// <summary>
    ///     Connects randomly selected rooms
    /// </summary>
    /// <param name="allRooms">List of rooms to process</param>
    private void ConnectRandomRooms(List<Room> allRooms)
    {
        for (var i = 0; i < 3; i++)
        {
            foreach (var room in allRooms)
                if (Random.Range(0, 100) < 50)
                    room.isAccessibleFromMainRoom = false;
            ConnectClosestRooms(allRooms);
        }
    }

    /// <summary>
    ///     Connects Closest rooms
    /// </summary>
    /// <param name="allRooms">List of rooms to process</param>
    /// <param name="forceAccessibility"></param>
    private void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibility = false)
    {
        var roomListA = new List<Room>();
        var roomListB = new List<Room>();

        if (forceAccessibility)
        {
            foreach (var room in allRooms)
                if (room.isAccessibleFromMainRoom)
                    roomListB.Add(room);
                else
                    roomListA.Add(room);
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        var bestDistance = 0;
        var bestTileA = new Tile();
        var bestTileB = new Tile();
        var bestRoomA = new Room();
        var bestRoomB = new Room();
        var possibleConFound = false;

        foreach (var roomA in roomListA)
        {
            if (!forceAccessibility)
            {
                possibleConFound = false;
                if (roomA.connectedRooms.Count > 0) continue;
            }

            foreach (var roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB)) continue;

                foreach (var roomAtile in roomA.edgeTiles)
                    foreach (var roomBtile in roomB.edgeTiles)
                    {
                        var tileA = roomAtile;
                        var tileB = roomBtile;
                        var distanceBetweenRooms =
                            (int)(Mathf.Pow(tileA.X - tileB.X, 2) + Mathf.Pow(tileA.Y - tileB.Y, 2));

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

            if (possibleConFound && !forceAccessibility) CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
        }

        if (possibleConFound && forceAccessibility)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibility)
            ConnectClosestRooms(allRooms, true);
    }

    /// <summary>
    ///     Creates Passage between 2 through tiles A and B
    /// </summary>
    /// <param name="roomA"></param>
    /// <param name="roomB"></param>
    /// <param name="tileA"></param>
    /// <param name="tileB"></param>
    private void CreatePassage(Room roomA, Room roomB, Tile tileA, Tile tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        var line = GetLine(tileA, tileB);
        foreach (var c in line)
            ClearPass(c, PassRadius);
    }

    /// <summary>
    ///     Clears dead cells around tile with radius
    /// </summary>
    /// <param name="c"></param>
    /// <param name="r">radius</param>
    private void ClearPass(Tile c, int r)
    {
        for (var x = -r; x <= r; x++)
            for (var y = -r; y <= r; y++)
                if (x * x + y * y <= r * r)
                {
                    var drawX = c.X + x;
                    var drawY = c.Y + y;
                    if (IsOnBoard(drawX, drawY))
                        Map[drawX, drawY] = true;
                }
    }

    /// <summary>
    ///     Creates line of tiles between 2 tiles
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    private List<Tile> GetLine(Tile from, Tile to)
    {
        var line = new List<Tile>();

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
        for (var i = 0; i < longest; i++)
        {
            line.Add(new Tile(x, y));

            if (inverted)
                y += step;
            else
                x += step;

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                    x += gradientStep;
                else
                    y += gradientStep;
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    /// <summary>
    ///     Creates regions of certain tipe
    /// </summary>
    /// <param name="tileType"></param>
    /// <returns></returns>
    private List<List<Tile>> GetRegions(bool tileType)
    {
        var regions = new List<List<Tile>>();
        var boardFlags = new int[Width, Height];

        for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
                if (boardFlags[x, y] == 0 && Map[x, y] == tileType)
                {
                    var newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (var tile in newRegion)
                        boardFlags[tile.X, tile.Y] = 1;
                }

        return regions;
    }

    /// <summary>
    ///     Collects all tiles of region
    /// </summary>
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <returns></returns>
    private List<Tile> GetRegionTiles(int startX, int startY)
    {
        var tiles = new List<Tile>();
        var boardFlags = new int[Width, Height];
        var tileType = Map[startX, startY];

        var queue = new Queue<Tile>();
        queue.Enqueue(new Tile(startX, startY));
        boardFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            var tile = queue.Dequeue();
            tiles.Add(tile);

            for (var x = tile.X - 1; x <= tile.X + 1; x++)
                for (var y = tile.Y - 1; y <= tile.Y + 1; y++)
                    if (IsOnBoard(x, y) && (y == tile.Y || x == tile.X))
                        if (boardFlags[x, y] == 0 && Map[x, y] == tileType)
                        {
                            boardFlags[x, y] = 1;
                            queue.Enqueue(new Tile(x, y));
                        }
        }

        return tiles;
    }

    /// <summary>
    ///     Checks borders of map/array
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool IsOnBoard(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    /// <summary>
    ///     Randomly fills map with live and dead cells
    /// </summary>
    private void RandomFillMap()
    {
        var pseudoRandom = new System.Random();

        for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                    Map[x, y] = false;
                else
                    Map[x, y] = pseudoRandom.Next(0, 100) >= RandomFillPercent;
    }

    /// <summary>
    ///     Smoothes the map with Moore neighborhood 
    /// </summary>
    /// <param name="n"></param>
    private void SmoothMap(int n)
    {
        for (var i = 0; i < n; i++)
            for (var x = 0; x < Width; x++)
                for (var y = 0; y < Height; y++)
                {
                    var neighbourWallTiles = GetSurroundingWallCount(x, y);

                    if (neighbourWallTiles > 4)
                        Map[x, y] = false;
                    else if (neighbourWallTiles < 4)
                        Map[x, y] = true;
                }
    }

    /// <summary>
    ///     Moore neighborhood algorithm
    /// </summary>
    /// <param name="gridX"></param>
    /// <param name="gridY"></param>
    /// <returns></returns>
    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        var wallCount = 0;
        for (var neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
            for (var neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
                if (IsOnBoard(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                        wallCount += Map[neighbourX, neighbourY] ? 0 : 1;
                }
                else
                {
                    wallCount++;
                }

        return wallCount;
    }


    // For debug purpose

    //void OnDrawGizmos () {
    //    if (board != null) {
    //        for (int x = 0; x < width; x++) {
    //            for (int y = 0; y < height; y++) {
    //                if (Map[x, y] == 1) {
    //                    Gizmos.color = Color.black;
    //                    Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
    //                    Gizmos.DrawCube(pos, Vector3.one);
    //                }
    //            }
    //        }
    //    }
    //}
}