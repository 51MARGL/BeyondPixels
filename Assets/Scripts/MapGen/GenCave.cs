using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenCave : MonoBehaviour {

    public event Action boardIsReady;

    public struct TCoord {
        public int x;
        public int y;

        public TCoord (int x, int y) {
            this.x = x;
            this.y = y;
        }
    }

    public int width;
    public int height;

    [Range(0, 100)]
    public int randomFillPercent;
    public int passRadius;

    int[,] board;

    bool isNotGenerated = true;
    public int[,] GetBoard () {
        return board;
    }

    void Start () {
        double difference;
        DateTime start = DateTime.UtcNow;
        while (isNotGenerated) {
            GenerateMap();
        }
        difference = start.Subtract(DateTime.UtcNow).TotalSeconds;
        print("MapGenerated: " + Math.Abs(difference));
        boardIsReady();
    }

    void Update () {
    }

    void GenerateMap () {
        board = new int[width, height];
        RandomFillMap();
        if (CheckOnFreeSpace()) {
            SmoothMap(3);
            ProcessMap();
            SmoothMap(3);
            RemoveThinWalls();
            SmoothMap(3);
            CloseBorders();
        } else {
            randomFillPercent -= 5;
            isNotGenerated = true;
        }
    }

    bool CheckOnFreeSpace () {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (board[i, j] == 0) return true;
            }
        }
        return false;
    }

    void CloseBorders () {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if ((x == 0 || x == width - 1 || x == width - 2 || x == 1
                    || y == 0 || y == 1 || y == height - 1 || y == height - 2)
                    && board[x, y] == 0) {
                    board[x, y] = 1;
                }
            }
        }
    }

    void RemoveThinWalls () {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (NotOnBorder(x, y) && board[x, y - 1] == 1
                                      && board[x + 1, y] == 1
                                      && board[x + 1, y - 1] == 0
                                      && board[x, y + 1] == 0) {
                    ClearPass(new TCoord(x, y), passRadius);
                } else if (NotOnBorder(x, y) && board[x, y - 1] == 1
                                      && board[x - 1, y] == 1
                                      && board[x - 1, y - 1] == 0
                                      && board[x + 1, y + 1] == 0) {
                    ClearPass(new TCoord(x, y), passRadius);
                } else if (NotOnBorder(x, y) && board[x, y - 1] == 1
                                             && board[x + 1, y] == 1 && board[x + 1, y - 1] == 0
                                             && board[x - 1, y + 1] == 0) {
                    ClearPass(new TCoord(x, y), passRadius);
                } else if (NotOnBorder(x, y) && board[x, y - 1] == 1
                                             && board[x - 1, y] == 1 && board[x - 1, y - 1] == 0
                                             && board[x, y + 1] == 0) {
                    ClearPass(new TCoord(x, y), passRadius);
                } else if (NotOnBorder(x, y) && board[x, y - 1] == 1
                                             && board[x, y + 1] == 1
                                             && board[x - 1, y] == 0 && board[x + 1, y] == 0) {
                    ClearPass(new TCoord(x, y), passRadius);
                } else if (NotOnBorder(x, y) && board[x, y - 1] == 0
                                             && board[x, y + 1] == 0
                                             && board[x - 1, y] == 1 && board[x + 1, y] == 1) {
                    ClearPass(new TCoord(x, y), passRadius);
                }
            }
        }
    }
    void ProcessMap () {
        List<List<TCoord>> wallRegions = GetRegions(1);
        int wallThresholdSize = 3;

        foreach (List<TCoord> wallRegion in wallRegions) {
            if (wallRegion.Count < wallThresholdSize) {
                foreach (TCoord tile in wallRegion) {
                    board[tile.x, tile.y] = 0;
                }
            }
        }

        List<List<TCoord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 2;
        List<Room> finalRooms = new List<Room>();

        foreach (List<TCoord> roomRegion in roomRegions) {
            if (roomRegion.Count < roomThresholdSize) {
                foreach (TCoord tile in roomRegion) {
                    board[tile.x, tile.y] = 1;
                }
            } else {
                finalRooms.Add(new Room(roomRegion, board));
            }
        }
        finalRooms.Sort();
        if (finalRooms.Count < 1 || finalRooms[0].roomSize < 20) {
            randomFillPercent -= 5;
            isNotGenerated = true;
        } else {
            finalRooms[0].isMainRoom = true;
            finalRooms[0].isAccessibleFromMainRoom = true;

            ConnectClosestRooms(finalRooms);
            ConnectRandomRooms(finalRooms);
            isNotGenerated = false;
        }
    }

    void ConnectRandomRooms(List<Room> allRooms)
    {
        for (int i = 0; i < 3; i++)
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
    void ConnectClosestRooms (List<Room> allRooms, bool forceAccessibility = false) {

        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibility) {
            foreach (Room room in allRooms) {
                if (room.isAccessibleFromMainRoom) {
                    roomListB.Add(room);
                } else {
                    roomListA.Add(room);
                }
            }
        } else {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        TCoord bestTileA = new TCoord();
        TCoord bestTileB = new TCoord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConFound = false;

        foreach (Room roomA in roomListA) {
            if (!forceAccessibility) {
                possibleConFound = false;
                if (roomA.connectedRooms.Count > 0) {
                    continue;
                }
            }

            foreach (Room roomB in roomListB) {
                if (roomA == roomB || roomA.IsConnected(roomB)) {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++) {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++) {
                        TCoord tileA = roomA.edgeTiles[tileIndexA];
                        TCoord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.x - tileB.x, 2) + Mathf.Pow(tileA.y - tileB.y, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConFound) {
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
            if (possibleConFound && !forceAccessibility) {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConFound && forceAccessibility) {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibility) {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage (Room roomA, Room roomB, TCoord tileA, TCoord tileB) {
        Room.ConnectRooms(roomA, roomB);
        List<TCoord> line = GetLine(tileA, tileB);
        foreach (TCoord c in line) {
            ClearPass(c, passRadius);
        }
    }

    void ClearPass (TCoord c, int r) {
        for (int x = -r; x <= r; x++) {
            for (int y = -r; y <= r; y++) {
                if (x * x + y * y <= r * r) {
                    int drawX = c.x + x;
                    int drawY = c.y + y;
                    if (IsOnBoard(drawX, drawY)) {
                        board[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    List<TCoord> GetLine (TCoord from, TCoord to) {
        List<TCoord> line = new List<TCoord>();

        int x = from.x;
        int y = from.y;

        int dx = to.x - from.x;
        int dy = to.y - from.y;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest) {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++) {
            line.Add(new TCoord(x, y));

            if (inverted) {
                y += step;
            } else {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest) {
                if (inverted) {
                    x += gradientStep;
                } else {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }
        return line;
    }

    List<List<TCoord>> GetRegions (int tileType) {
        List<List<TCoord>> regions = new List<List<TCoord>>();
        int[,] boardFlags = new int[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (boardFlags[x, y] == 0 && board[x, y] == tileType) {
                    List<TCoord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (TCoord tile in newRegion) {
                        boardFlags[tile.x, tile.y] = 1;
                    }
                }
            }
        }
        return regions;
    }

    List<TCoord> GetRegionTiles (int startX, int startY) {
        List<TCoord> tiles = new List<TCoord>();
        int[,] boardFlags = new int[width, height];
        int tileType = board[startX, startY];

        Queue<TCoord> queue = new Queue<TCoord>();
        queue.Enqueue(new TCoord(startX, startY));
        boardFlags[startX, startY] = 1;

        while (queue.Count > 0) {
            TCoord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.x - 1; x <= tile.x + 1; x++) {
                for (int y = tile.y - 1; y <= tile.y + 1; y++) {
                    if (IsOnBoard(x, y) && (y == tile.y || x == tile.x)) {
                        if (boardFlags[x, y] == 0 && board[x, y] == tileType) {
                            boardFlags[x, y] = 1;
                            queue.Enqueue(new TCoord(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    bool IsOnBoard (int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    bool NotOnBorder (int x, int y) {
        return y != 0 && x != 0 && y != height - 1 && x != width - 1;
    }

    void RandomFillMap () {
        System.Random pseudoRandom = new System.Random();

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
                    board[x, y] = 1;
                } else {
                    board[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap (int n) {
        for (int i = 0; i < n; i++) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    int neighbourWallTiles = GetSurroundingWallCount(x, y);

                    if (neighbourWallTiles > 4)
                        board[x, y] = 1;
                    else if (neighbourWallTiles < 4)
                        board[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount (int gridX, int gridY) {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
                if (IsOnBoard(neighbourX, neighbourY)) {
                    if (neighbourX != gridX || neighbourY != gridY) {
                        wallCount += board[neighbourX, neighbourY];
                    }
                } else {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

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
