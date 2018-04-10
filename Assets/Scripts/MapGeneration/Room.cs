using System;
using System.Collections.Generic;

internal class Room : IComparable<Room>
{
    public List<Room> connectedRooms;
    public List<Tile> edgeTiles;
    public bool isAccessibleFromMainRoom;
    public bool isMainRoom;
    public int roomSize;
    public List<Tile> tiles;

    public Room()
    {
    }

    public Room(List<Tile> roomTiles, bool[,] board)
    {
        tiles = roomTiles;
        roomSize = tiles.Count;
        connectedRooms = new List<Room>();

        edgeTiles = new List<Tile>();
        foreach (var tile in tiles)
            for (var x = tile.X - 1; x <= tile.X + 1; x++)
                for (var y = tile.Y - 1; y <= tile.Y + 1; y++)
                    if (x == tile.X || y == tile.Y)
                        if (!board[x, y])
                            edgeTiles.Add(tile);
    }

    public int CompareTo(Room otherRoom)
    {
        return otherRoom.roomSize.CompareTo(roomSize);
    }

    public void SetAccessibleFromMainRoom()
    {
        if (!isAccessibleFromMainRoom)
        {
            isAccessibleFromMainRoom = true;
            foreach (var connectedRoom in connectedRooms) connectedRoom.SetAccessibleFromMainRoom();
        }
    }

    public static void ConnectRooms(Room roomA, Room roomB)
    {
        if (roomA.isAccessibleFromMainRoom)
            roomB.SetAccessibleFromMainRoom();
        else if (roomB.isAccessibleFromMainRoom) roomA.SetAccessibleFromMainRoom();
        roomA.connectedRooms.Add(roomB);
        roomB.connectedRooms.Add(roomA);
    }

    public bool IsConnected(Room otherRoom)
    {
        return connectedRooms.Contains(otherRoom);
    }
}