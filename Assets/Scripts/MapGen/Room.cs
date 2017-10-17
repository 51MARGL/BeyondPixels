using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class Room : IComparable<Room> {
    public List<GenCave.TCoord> tiles;
    public List<GenCave.TCoord> edgeTiles;
    public List<Room> connectedRooms;
    public int roomSize;
    public bool isAccessibleFromMainRoom;
    public bool isMainRoom;

    public Room () {
    }

    public Room (List<GenCave.TCoord> roomTiles, int[,] board) {
        tiles = roomTiles;
        roomSize = tiles.Count;
        connectedRooms = new List<Room>();

        edgeTiles = new List<GenCave.TCoord>();
        foreach (var tile in tiles) {
            for (int x = tile.x - 1; x <= tile.x + 1; x++) {
                for (int y = tile.y - 1; y <= tile.y + 1; y++) {
                    if (x == tile.x || y == tile.y) {
                        if (board[x, y] == 1) {
                            edgeTiles.Add(tile);
                        }
                    }
                }
            }
        }
    }

    public void SetAccessibleFromMainRoom () {
        if (!isAccessibleFromMainRoom) {
            isAccessibleFromMainRoom = true;
            foreach (Room connectedRoom in connectedRooms) {
                connectedRoom.SetAccessibleFromMainRoom();
            }
        }
    }

    public static void ConnectRooms (Room roomA, Room roomB) {
        if (roomA.isAccessibleFromMainRoom) {
            roomB.SetAccessibleFromMainRoom();
        } else if (roomB.isAccessibleFromMainRoom) {
            roomA.SetAccessibleFromMainRoom();
        }
        roomA.connectedRooms.Add(roomB);
        roomB.connectedRooms.Add(roomA);
    }

    public bool IsConnected (Room otherRoom) {
        return connectedRooms.Contains(otherRoom);
    }

    public int CompareTo (Room otherRoom) {
        return otherRoom.roomSize.CompareTo(roomSize);
    }
}

