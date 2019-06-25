using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton;

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon.CellularAutomaton
{
    public class BoardSystem : JobComponentSystem
    {
        [BurstCompile]
        private struct RandomFillBoardJob : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly]
            public NativeArray<TileComponent> Tiles;

            [ReadOnly]
            public int TileStride;

            [ReadOnly]
            public int RandomFillPercent;

            [ReadOnly]
            public int RandomSeed;

            public void Execute(int index)
            {
                var random = new Random((uint)(this.RandomSeed * (index + 1)));
                for (var x = 0; x < this.TileStride; x++)
                    if (random.NextInt(0, 100) > this.RandomFillPercent)
                        this.Tiles[index * this.TileStride + x] = new TileComponent
                        {
                            Position = new int2(x, index),
                            CurrentGenState = TileType.Floor
                        };
                    else
                        this.Tiles[index * this.TileStride + x] = new TileComponent
                        {
                            Position = new int2(x, index),
                            CurrentGenState = TileType.Wall
                        };
            }
        }

        [BurstCompile]
        private struct CalculateNextGenerationJob : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<TileComponent> Tiles;

            [ReadOnly]
            public int TileStride;

            public void Execute(int index)
            {
                var currentTile = this.Tiles[index];

                currentTile.NextGenState =
                    this.GetDeadNeighborsCount(currentTile.Position.x, currentTile.Position.y) > 4
                        ? TileType.Wall
                        : TileType.Floor;

                this.Tiles[index] = currentTile;
            }

            public int GetDeadNeighborsCount(int gridX, int gridY)
            {
                var wallCount = 0;
                for (var neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
                    for (var neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
                    {
                        var currIndex = neighbourY * this.TileStride + neighbourX;
                        if (currIndex >= 0 && currIndex < this.Tiles.Length)
                        {
                            if (neighbourX != gridX || neighbourY != gridY)
                                wallCount +=
                                    this.Tiles[currIndex].CurrentGenState == TileType.Floor ? 0 : 1;
                        }
                        else
                            wallCount++;
                    }

                return wallCount;
            }

        }

        [BurstCompile]
        private struct ProcessNextGenerationJob : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly]
            public NativeArray<TileComponent> Tiles;

            public void Execute(int index)
            {
                var currentTile = this.Tiles[index];
                currentTile.CurrentGenState = currentTile.NextGenState;
                this.Tiles[index] = currentTile;
            }
        }

        //[BurstCompile] 19.1 support only
        private struct GetRoomsJob : IJob
        {
            [ReadOnly]
            public BoardComponent Board;

            public NativeArray<TileComponent> RoomTiles;

            public NativeArray<TileComponent> Tiles;

            [WriteOnly]
            public NativeList<RoomComponent> Rooms;

            public void Execute()
            {
                var flags = new NativeArray<int>(this.Tiles.Length, Allocator.Temp);
                var roomTilesIndex = 0;
                for (var y = 0; y < this.Board.Size.y; y++)
                    for (var x = 0; x < this.Board.Size.x; x++)
                    {
                        var currentTile = this.Tiles[y * this.Board.Size.x + x];
                        if (flags[y * this.Board.Size.x + x] == 0
                            && currentTile.CurrentGenState == TileType.Floor)
                        {
                            var tileCount = this.AddRoomTiles(currentTile, flags, roomTilesIndex, this.Board.Size.x);

                            this.Rooms.Add(new RoomComponent
                            {
                                StartTileIndex = roomTilesIndex,
                                TileCount = tileCount
                            });

                            roomTilesIndex += tileCount;
                        }
                    }
            }

            private int AddRoomTiles(TileComponent startTile, NativeArray<int> flags, int roomTilesIndex, int TileStride)
            {
                var currentIndex = 0;

                var queue = new NativeQueue<TileComponent>(Allocator.Temp);
                queue.Enqueue(startTile);

                while (queue.Count > 0)
                {
                    var tile = queue.Dequeue();
                    this.RoomTiles[roomTilesIndex + currentIndex] = tile;
                    currentIndex++;

                    for (var y = tile.Position.y - 1; y <= tile.Position.y + 1; y++)
                        for (var x = tile.Position.x - 1; x <= tile.Position.x + 1; x++)
                        {
                            var currIndex = y * TileStride + x;

                            if (currIndex >= 0 && currIndex < this.Tiles.Length
                                && (y == tile.Position.y || x == tile.Position.x)
                                && flags[currIndex] == 0 && this.Tiles[currIndex].CurrentGenState == TileType.Floor)
                            {
                                flags[currIndex] = 1;
                                queue.Enqueue(this.Tiles[currIndex]);
                            }
                        }
                }
                return currentIndex;
            }
        }

        private struct RearangeRoomsJob : IJob
        {
            [ReadOnly]
            public BoardComponent Board;

            [ReadOnly]
            public NativeArray<TileComponent> RoomTiles;

            public NativeList<RoomComponent> Rooms;

            public void Execute()
            {
                var widthStep = this.Board.Size.x / math.clamp(this.Board.Size.x / 50, 1, this.Board.Size.x);
                var heightStep = this.Board.Size.y / math.clamp(this.Board.Size.y / 50, 1, this.Board.Size.y);
                var currentIndex = 0;
                for (var y = 0; y < this.Board.Size.y; y += heightStep)
                {
                    for (var x = 0; x < this.Board.Size.x; x += widthStep)
                        for (var i = currentIndex; i < this.Rooms.Length; i++)
                            if (this.RoomTiles[this.Rooms[i].StartTileIndex].Position.x >= x
                                && this.RoomTiles[this.Rooms[i].StartTileIndex].Position.x <= x + widthStep
                                && this.RoomTiles[this.Rooms[i].StartTileIndex].Position.y >= y
                                && this.RoomTiles[this.Rooms[i].StartTileIndex].Position.y <= y + heightStep)
                            {
                                var tmp = this.Rooms[currentIndex];
                                this.Rooms[currentIndex++] = this.Rooms[i];
                                this.Rooms[i] = tmp;
                            }
                }
            }
        }

        //[BurstCompile] 19.1 support only
        private struct FindClosestRoomsConnectionsJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<TileComponent> RoomTiles;

            [WriteOnly]
            public NativeQueue<CorridorComponent>.Concurrent Corridors;

            [ReadOnly]
            public NativeList<RoomComponent> Rooms;

            [NativeDisableContainerSafetyRestriction]
            public NativeArray<int> ConnectedRoomsTable;

            [ReadOnly]
            public int RoomsChunkSize;

            public void Execute(int index)
            {
                this.MarkClosestRoomsConnections(this.ConnectedRoomsTable, index * this.RoomsChunkSize, this.RoomsChunkSize);
            }

            private void MarkClosestRoomsConnections(NativeArray<int> connectedRoomsTable, int roomsStartIndex, int roomsChunkSize)
            {
                for (var i = roomsStartIndex; i < roomsStartIndex + roomsChunkSize && i < this.Rooms.Length; i++)
                {
                    var bestDistance = (float)int.MaxValue;
                    var bestTileA = new TileComponent();
                    var bestTileB = new TileComponent();
                    var bestRoomAIndex = 0;
                    var bestRoomBIndex = 0;

                    for (var j = roomsStartIndex; j < roomsStartIndex + roomsChunkSize && j < this.Rooms.Length; j++)
                    {
                        if (connectedRoomsTable[i * this.Rooms.Length + j] == 1)
                            continue;

                        for (var tileIndexA = 0; tileIndexA < this.Rooms[i].TileCount; tileIndexA++)
                        {
                            for (var tileIndexB = 0; tileIndexB < this.Rooms[j].TileCount; tileIndexB++)
                            {
                                var tileA = this.RoomTiles[this.Rooms[i].StartTileIndex + tileIndexA];
                                var tileB = this.RoomTiles[this.Rooms[j].StartTileIndex + tileIndexB];
                                var distance = math.distance(tileA.Position, tileB.Position);

                                if (distance < bestDistance)
                                {
                                    bestDistance = distance;
                                    bestTileA = tileA;
                                    bestTileB = tileB;
                                    bestRoomAIndex = i;
                                    bestRoomBIndex = j;
                                }
                            }
                        }
                    }

                    this.MarkRoomsConnection(connectedRoomsTable, bestRoomAIndex, bestRoomBIndex, bestTileA, bestTileB, roomsStartIndex, roomsChunkSize);
                }
            }

            private void MarkRoomsConnection(NativeArray<int> connectedRoomsTable, int bestRoomAIndex, int bestRoomBIndex, TileComponent bestTileA, TileComponent bestTileB, int roomsStartIndex, int roomsChunkSize)
            {
                var connectedTableCopy = new NativeArray<int>(connectedRoomsTable.Length, Allocator.Temp);
                connectedRoomsTable.CopyTo(connectedTableCopy);
                for (var i = 0; i < roomsStartIndex + roomsChunkSize && i < this.Rooms.Length; i++)
                {
                    if (connectedTableCopy[bestRoomAIndex * this.Rooms.Length + i] == 1)
                        for (var j = 0; j < roomsStartIndex + roomsChunkSize && j < this.Rooms.Length; j++)
                            if (connectedTableCopy[bestRoomBIndex * this.Rooms.Length + j] == 1)
                            {
                                connectedRoomsTable[i * this.Rooms.Length + j] = 1;
                                connectedRoomsTable[j * this.Rooms.Length + i] = 1;
                            }
                    if (connectedTableCopy[bestRoomBIndex * this.Rooms.Length + i] == 1)
                        for (var j = 0; j < this.Rooms.Length; j++)
                            if (connectedTableCopy[bestRoomAIndex * this.Rooms.Length + j] == 1)
                            {
                                connectedRoomsTable[i * this.Rooms.Length + j] = 1;
                                connectedRoomsTable[j * this.Rooms.Length + i] = 1;
                            }
                }

                this.Corridors.Enqueue(new CorridorComponent
                {
                    Start = bestTileA,
                    End = bestTileB
                });
            }
        }

        //[BurstCompile] 19.1 support only
        private struct FindAllRoomsConnectionsJob : IJob
        {
            [ReadOnly]
            public NativeArray<TileComponent> RoomTiles;

            [WriteOnly]
            public NativeQueue<CorridorComponent>.Concurrent Corridors;

            [ReadOnly]
            public NativeList<RoomComponent> Rooms;

            [DeallocateOnJobCompletion]
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<int> ConnectedRoomsTable;

            public void Execute()
            {
                this.MarkNotAccessibleRoomsConnections(this.ConnectedRoomsTable);
            }

            private void MarkNotAccessibleRoomsConnections(NativeArray<int> connectedRoomsTable)
            {
                var possibleConFound = true;
                var disconnectedRoomList = new NativeList<RoomComponent>(Allocator.Temp);
                var disconnectedRoomIndexList = new NativeList<int>(Allocator.Temp);
                var connectedRoomList = new NativeList<RoomComponent>(Allocator.Temp);
                var connectedRoomIndexList = new NativeList<int>(Allocator.Temp);

                while (possibleConFound)
                {
                    var bestDistance = (float)int.MaxValue;
                    var bestTileA = new TileComponent();
                    var bestTileB = new TileComponent();
                    var bestRoomAIndex = 0;
                    var bestRoomBIndex = 0;

                    possibleConFound = false;
                    disconnectedRoomList.Clear();
                    disconnectedRoomIndexList.Clear();
                    connectedRoomList.Clear();
                    connectedRoomIndexList.Clear();

                    for (var i = 0; i < this.Rooms.Length; i++)
                    {
                        if (connectedRoomsTable[i] == 1)
                        {
                            connectedRoomList.Add(this.Rooms[i]);
                            connectedRoomIndexList.Add(i);
                        }
                        else
                        {
                            disconnectedRoomList.Add(this.Rooms[i]);
                            disconnectedRoomIndexList.Add(i);
                        }
                    }

                    for (var i = 0; i < disconnectedRoomList.Length; i++)
                    {
                        for (var j = 0; j < connectedRoomList.Length; j++)
                        {
                            if (connectedRoomsTable[disconnectedRoomIndexList[i] * this.Rooms.Length + connectedRoomIndexList[j]] == 1)
                                continue;

                            for (var tileIndexA = 0; tileIndexA < disconnectedRoomList[i].TileCount; tileIndexA++)
                            {
                                for (var tileIndexB = 0; tileIndexB < connectedRoomList[j].TileCount; tileIndexB++)
                                {
                                    var tileA = this.RoomTiles[disconnectedRoomList[i].StartTileIndex + tileIndexA];
                                    var tileB = this.RoomTiles[connectedRoomList[j].StartTileIndex + tileIndexB];
                                    var distance = math.distance(tileA.Position, tileB.Position);

                                    if (distance < bestDistance)
                                    {
                                        bestDistance = distance;
                                        possibleConFound = true;
                                        bestTileA = tileA;
                                        bestTileB = tileB;
                                        bestRoomAIndex = disconnectedRoomIndexList[i];
                                        bestRoomBIndex = connectedRoomIndexList[j];
                                    }
                                }
                            }
                        }
                    }
                    if (possibleConFound)
                        this.MarkRoomsConnection(connectedRoomsTable, bestRoomAIndex, bestRoomBIndex, bestTileA, bestTileB);
                }
            }

            private void MarkRoomsConnection(NativeArray<int> connectedRoomsTable, int bestRoomAIndex, int bestRoomBIndex, TileComponent bestTileA, TileComponent bestTileB)
            {
                var connectedTableCopy = new NativeArray<int>(connectedRoomsTable.Length, Allocator.Temp);
                connectedRoomsTable.CopyTo(connectedTableCopy);
                for (var i = 0; i < this.Rooms.Length; i++)
                {
                    if (connectedTableCopy[bestRoomAIndex * this.Rooms.Length + i] == 1)
                        for (var j = 0; j < this.Rooms.Length; j++)
                            if (connectedTableCopy[bestRoomBIndex * this.Rooms.Length + j] == 1)
                            {
                                connectedRoomsTable[i * this.Rooms.Length + j] = 1;
                                connectedRoomsTable[j * this.Rooms.Length + i] = 1;
                            }
                    if (connectedTableCopy[bestRoomBIndex * this.Rooms.Length + i] == 1)
                        for (var j = 0; j < this.Rooms.Length; j++)
                            if (connectedTableCopy[bestRoomAIndex * this.Rooms.Length + j] == 1)
                            {
                                connectedRoomsTable[i * this.Rooms.Length + j] = 1;
                                connectedRoomsTable[j * this.Rooms.Length + i] = 1;
                            }
                }

                this.Corridors.Enqueue(new CorridorComponent
                {
                    Start = bestTileA,
                    End = bestTileB
                });
            }
        }

        [BurstCompile]
        private struct CorridorsQueueToArrayJob : IJob
        {
            [WriteOnly]
            public NativeArray<CorridorComponent> CorridorsArray;
            public NativeQueue<CorridorComponent> CorridorsQueue;

            public void Execute()
            {
                var index = 0;
                while (this.CorridorsQueue.Count > 0)
                    if (this.CorridorsQueue.TryDequeue(out var corridor))
                        this.CorridorsArray[index++] = corridor;
            }
        }

        //[BurstCompile] 19.1 support only
        private struct CreateCorridorsJob : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<TileComponent> Tiles;

            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<CorridorComponent> Corridors;

            [ReadOnly]
            public int TileStride;

            [ReadOnly]
            public int PassageRadius;

            public void Execute(int index)
            {
                var tileA = this.Corridors[index].Start;
                var tileB = this.Corridors[index].End;
                var line = this.GetLine(tileA, tileB);
                for (var i = 0; i < line.Length; i++)
                    this.ClearPass(line[i], this.PassageRadius);

            }

            private NativeList<TileComponent> GetLine(TileComponent start, TileComponent end)
            {
                var line = new NativeList<TileComponent>(Allocator.Temp);

                var x = start.Position.x;
                var y = start.Position.y;

                var dx = end.Position.x - start.Position.x;
                var dy = end.Position.y - start.Position.y;

                var inverted = false;
                var step = (int)math.sign(dx);
                var gradientStep = (int)math.sign(dy);

                var longest = math.abs(dx);
                var shortest = math.abs(dy);

                if (longest < shortest)
                {
                    inverted = true;
                    longest = math.abs(dy);
                    shortest = math.abs(dx);

                    step = (int)math.sign(dy);
                    gradientStep = (int)math.sign(dx);
                }

                var gradientAccumulation = longest / 2;
                for (var i = 0; i < longest; i++)
                {
                    line.Add(this.Tiles[y * this.TileStride + x]);

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

            private void ClearPass(TileComponent tile, int radius)
            {
                for (var x = -radius; x <= radius; x++)
                    for (var y = -radius; y <= radius; y++)
                    {
                        var drawX = tile.Position.x + x;
                        var drawY = tile.Position.y + y;

                        var currTile = this.Tiles[drawY * this.TileStride + drawX];
                        currTile.CurrentGenState = TileType.Floor;
                        this.Tiles[drawY * this.TileStride + drawX] = currTile;

                    }
            }
        }

        [BurstCompile]
        private struct CloseBordersJob : IJob
        {
            [ReadOnly]
            public BoardComponent Board;
            public NativeArray<TileComponent> Tiles;

            public void Execute()
            {
                TileComponent currentTile;
                var tilesStride = this.Board.Size.x;
                for (var x = 0; x < this.Board.Size.x; x++)
                {
                    currentTile = this.Tiles[x];
                    currentTile.CurrentGenState = TileType.Wall;
                    this.Tiles[x] = currentTile;

                    currentTile = this.Tiles[tilesStride + x];
                    currentTile.CurrentGenState = TileType.Wall;
                    this.Tiles[tilesStride + x] = currentTile;

                    currentTile = this.Tiles[((this.Board.Size.y - 1) * tilesStride) + x];
                    currentTile.CurrentGenState = TileType.Wall;
                    this.Tiles[((this.Board.Size.y - 1) * tilesStride) + x] = currentTile;

                    currentTile = this.Tiles[((this.Board.Size.y - 2) * tilesStride) + x];
                    currentTile.CurrentGenState = TileType.Wall;
                    this.Tiles[((this.Board.Size.y - 2) * tilesStride) + x] = currentTile;
                }
                for (var y = 0; y < this.Board.Size.y; y++)
                {
                    currentTile = this.Tiles[y * tilesStride];
                    currentTile.CurrentGenState = TileType.Wall;
                    this.Tiles[y * tilesStride] = currentTile;

                    currentTile = this.Tiles[y * tilesStride + 1];
                    currentTile.CurrentGenState = TileType.Wall;
                    this.Tiles[y * tilesStride + 1] = currentTile;

                    currentTile = this.Tiles[(y * tilesStride) + this.Board.Size.x - 1];
                    currentTile.CurrentGenState = TileType.Wall;
                    this.Tiles[(y * tilesStride) + this.Board.Size.x - 1] = currentTile;

                    currentTile = this.Tiles[(y * tilesStride) + this.Board.Size.x - 2];
                    currentTile.CurrentGenState = TileType.Wall;
                    this.Tiles[(y * tilesStride) + this.Board.Size.x - 2] = currentTile;
                }
            }
        }

        [BurstCompile]
        private struct RemoveThinWallsJob : IJob
        {
            [ReadOnly]
            public BoardComponent Board;
            public NativeArray<TileComponent> Tiles;

            public void Execute()
            {
                this.RemoveThinWalls(this.Board, this.Tiles, this.Board.Size.x);
            }

            private void RemoveThinWalls(BoardComponent board, NativeArray<TileComponent> tiles, int tilesStride)
            {
                var inconsistentTileDetected = true;
                while (inconsistentTileDetected)
                {
                    inconsistentTileDetected = false;
                    for (var y = 1; y < board.Size.y - 1; y++)
                        for (var x = 1; x < board.Size.x - 1; x++)
                            if (tiles[(y * tilesStride) + x].CurrentGenState == TileType.Wall
                                && ((tiles[(y * tilesStride) + x + 1].CurrentGenState == TileType.Floor && tiles[(y * tilesStride) + x - 1].CurrentGenState == TileType.Floor) // pattern -> -
                                    || (tiles[((y + 1) * tilesStride) + x].CurrentGenState == TileType.Floor && tiles[((y - 1) * tilesStride) + x].CurrentGenState == TileType.Floor) // pattern -> |
                                    || ((tiles[((y + 1) * tilesStride) + x + 1].CurrentGenState == TileType.Floor && tiles[((y - 1) * tilesStride) + x - 1].CurrentGenState == TileType.Floor)// pattern -> /
                                        && (tiles[((y + 1) * tilesStride) + x - 1].CurrentGenState == TileType.Wall && tiles[((y - 1) * tilesStride) + x + 1].CurrentGenState == TileType.Wall))// pattern -> \
                                    || ((tiles[((y + 1) * tilesStride) + x + 1].CurrentGenState == TileType.Wall && tiles[((y - 1) * tilesStride) + x - 1].CurrentGenState == TileType.Wall)// pattern -> /
                                        && (tiles[((y + 1) * tilesStride) + x - 1].CurrentGenState == TileType.Floor && tiles[((y - 1) * tilesStride) + x + 1].CurrentGenState == TileType.Floor)) // pattern -> \
                                    || ((tiles[((y + 1) * tilesStride) + x + 1].CurrentGenState == TileType.Wall) && (tiles[((y - 1) * tilesStride) + x - 1].CurrentGenState == TileType.Wall)
                                        && tiles[((y + 1) * tilesStride) + x].CurrentGenState == TileType.Floor && tiles[(y * tilesStride) + x + 1].CurrentGenState == TileType.Floor)
                                    || ((tiles[((y + 1) * tilesStride) + x - 1].CurrentGenState == TileType.Wall) && (tiles[((y - 1) * tilesStride) + x + 1].CurrentGenState == TileType.Wall)
                                        && tiles[((y + 1) * tilesStride) + x].CurrentGenState == TileType.Floor && tiles[(y * tilesStride) + x - 1].CurrentGenState == TileType.Floor)
                                ))
                            {
                                var currentTile = tiles[(y * tilesStride) + x];
                                currentTile.CurrentGenState = TileType.Floor;
                                tiles[(y * tilesStride) + x] = currentTile;
                                inconsistentTileDetected = true;
                            }
                }
            }
        }

        private struct InstantiateTilesJob : IJobParallelFor
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            public NativeArray<TileComponent> Tiles;

            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<TileComponent> RoomTiles;

            [ReadOnly]
            public int TileStride;

            public void Execute(int index)
            {
                if (this.IsBoardValid())
                    for (var x = 0; x < this.TileStride; x++)
                    {
                        var entity = this.CommandBuffer.CreateEntity(index);
                        this.CommandBuffer.AddComponent(index, entity, new FinalTileComponent
                        {
                            TileType = this.Tiles[(index * this.TileStride) + x].CurrentGenState,
                            Position = new int2(x, index)
                        });
                    }
            }

            private bool IsBoardValid()
            {
                var freeTilesCount = 0;
                for (var i = 0; i < this.Tiles.Length; i++)
                    if (this.Tiles[i].CurrentGenState == TileType.Floor)
                        freeTilesCount++;

                if (freeTilesCount < 50)
                    return false;
                return true;
            }
        }

        private struct TagBoardDoneJob : IJob
        {
            public EntityCommandBuffer CommandBuffer;
            [ReadOnly]
            public Entity BoardEntity;
            [ReadOnly]
            public BoardComponent BoardComponent;
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<TileComponent> Tiles;
            [ReadOnly]
            public int RandomSeed;

            public void Execute()
            {
                if (this.IsBoardValid())
                {
                    this.CommandBuffer.AddComponent(this.BoardEntity, new BoardReadyComponent());
                    var finalBoardComponent = this.CommandBuffer.CreateEntity();
                    this.CommandBuffer.AddComponent(finalBoardComponent, new FinalBoardComponent
                    {
                        Size = this.BoardComponent.Size
                    });
                }
                else
                {
                    var random = new Random((uint)this.RandomSeed);

                    var randomSize = new int2(random.NextInt(100, 200), random.NextInt(50, 150));
                    var randomFillPercent = random.NextInt(60, 75);
                    var board = this.CommandBuffer.CreateEntity();
                    this.CommandBuffer.AddComponent(board, new BoardComponent
                    {
                        Size = randomSize,
                        RandomFillPercent = random.NextInt(60, 75),
                        PassRadius = this.BoardComponent.PassRadius
                    });
                    this.CommandBuffer.DestroyEntity(this.BoardEntity);
                }
            }

            private bool IsBoardValid()
            {
                var freeTilesCount = 0;
                for (var i = 0; i < this.Tiles.Length; i++)
                    if (this.Tiles[i].CurrentGenState == TileType.Floor)
                        freeTilesCount++;

                if (freeTilesCount < 50)
                    return false;
                return true;
            }
        }

        [BurstCompile]
        private struct CleanUpJob : IJob
        {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> Chunks;
            public void Execute()
            {

            }
        }

        private NativeList<RoomComponent> RoomList;
        private NativeQueue<CorridorComponent> CorridorsQueue;
        private NativeArray<TileComponent> Tiles;
        private NativeArray<TileComponent> RoomTiles;
        private int CurrentPhase;

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private EntityQuery _boardGroup;
        protected override void OnCreateManager()
        {
            this._endFrameBarrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            this.RoomList = new NativeList<RoomComponent>(Allocator.Persistent);
            this.CorridorsQueue = new NativeQueue<CorridorComponent>(Allocator.Persistent);
            this.CurrentPhase = 0;
            this._boardGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(BoardComponent)
                },
                None = new ComponentType[]
                {
                    typeof(BoardReadyComponent)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var boardChunks = this._boardGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            for (var chunkIndex = 0; chunkIndex < boardChunks.Length; chunkIndex++)
            {
                var chunk = boardChunks[chunkIndex];
                var boardEntities = chunk.GetNativeArray(this.GetArchetypeChunkEntityType());
                var boards = chunk.GetNativeArray(this.GetArchetypeChunkComponentType<BoardComponent>());
                for (var i = 0; i < chunk.Count; i++)
                {
                    var board = boards[i];
                    var boardEntity = boardEntities[i];
                    var random = new Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());
                    if (this.CurrentPhase == 0)
                    {
                        this.Tiles = new NativeArray<TileComponent>(board.Size.x * board.Size.y, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        this.RoomTiles = new NativeArray<TileComponent>(this.Tiles.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        this.RoomList.Clear();
                        this.CorridorsQueue.Clear();

                        var randomFillBoardJobHandle = new RandomFillBoardJob
                        {
                            Tiles = Tiles,
                            TileStride = board.Size.x,
                            RandomFillPercent = board.RandomFillPercent,
                            RandomSeed = random.NextInt()
                        }.Schedule(board.Size.y, 1, inputDeps);

                        var lastGenerationJobHandle = randomFillBoardJobHandle;
                        for (var geneation = 0; geneation < 5; geneation++)
                        {
                            var calculateNextGenerationJobHandle = new CalculateNextGenerationJob
                            {
                                Tiles = Tiles,
                                TileStride = board.Size.x
                            }.Schedule(this.Tiles.Length, 1, lastGenerationJobHandle);

                            lastGenerationJobHandle = new ProcessNextGenerationJob
                            {
                                Tiles = Tiles
                            }.Schedule(this.Tiles.Length, 32, calculateNextGenerationJobHandle);
                        }

                        var closeBordersJobHandle = new CloseBordersJob
                        {
                            Board = board,
                            Tiles = Tiles
                        }.Schedule(lastGenerationJobHandle);

                        inputDeps = new GetRoomsJob
                        {
                            Board = board,
                            RoomTiles = RoomTiles,
                            Tiles = Tiles,
                            Rooms = RoomList
                        }.Schedule(closeBordersJobHandle);
                    }
                    else if (this.CurrentPhase == 1)
                    {
                        var roomCount = this.RoomList.Length;
                        var roomsChunkSize = 10;
                        var concurrentQueue = this.CorridorsQueue.ToConcurrent();
                        var connectedRoomsTable = new NativeArray<int>(roomCount * roomCount, Allocator.TempJob);

                        for (var roomIndex = 0; roomIndex < roomCount; roomIndex++)
                            connectedRoomsTable[roomIndex * roomCount + roomIndex] = 1;

                        var RearangeRoomsJobhandle = new RearangeRoomsJob
                        {
                            Board = board,
                            RoomTiles = RoomTiles,
                            Rooms = RoomList
                        }.Schedule(inputDeps);

                        var findClosestRoomsConnectionsJobHandle = new FindClosestRoomsConnectionsJob
                        {
                            RoomTiles = RoomTiles,
                            Rooms = RoomList,
                            Corridors = concurrentQueue,
                            ConnectedRoomsTable = connectedRoomsTable,
                            RoomsChunkSize = roomsChunkSize
                        }.Schedule(roomCount / roomsChunkSize + 1, 1, RearangeRoomsJobhandle);

                        inputDeps = new FindAllRoomsConnectionsJob
                        {
                            RoomTiles = RoomTiles,
                            Rooms = RoomList,
                            ConnectedRoomsTable = connectedRoomsTable,
                            Corridors = concurrentQueue
                        }.Schedule(findClosestRoomsConnectionsJobHandle);
                    }
                    else if (this.CurrentPhase == 2)
                    {
                        var corridorsArray = new NativeArray<CorridorComponent>(this.CorridorsQueue.Count, Allocator.TempJob);

                        var corridorsQueueToArrayJobHandle = new CorridorsQueueToArrayJob
                        {
                            CorridorsArray = corridorsArray,
                            CorridorsQueue = CorridorsQueue
                        }.Schedule(inputDeps);

                        var createCorridorsJobHandle = new CreateCorridorsJob
                        {
                            Tiles = Tiles,
                            Corridors = corridorsArray,
                            TileStride = board.Size.x,
                            PassageRadius = board.PassRadius
                        }.Schedule(corridorsArray.Length, 1, corridorsQueueToArrayJobHandle);

                        var calculateNextGenerationJobHandle = new CalculateNextGenerationJob
                        {
                            Tiles = Tiles,
                            TileStride = board.Size.x
                        }.Schedule(this.Tiles.Length, 1, createCorridorsJobHandle);

                        var lastGenerationJobHandle = new ProcessNextGenerationJob
                        {
                            Tiles = Tiles,
                        }.Schedule(this.Tiles.Length, 32, calculateNextGenerationJobHandle);

                        var removeThinWallsJobHandle = new RemoveThinWallsJob
                        {
                            Board = board,
                            Tiles = Tiles
                        }.Schedule(lastGenerationJobHandle);

                        var closeBordersJobHandle = new CloseBordersJob
                        {
                            Board = board,
                            Tiles = Tiles
                        }.Schedule(removeThinWallsJobHandle);

                        var instantiateTilesJobHandle = new InstantiateTilesJob
                        {
                            CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                            Tiles = Tiles,
                            TileStride = board.Size.x,
                            RoomTiles = RoomTiles
                        }.Schedule(board.Size.y, 1, closeBordersJobHandle);

                        inputDeps = new TagBoardDoneJob
                        {
                            CommandBuffer = this._endFrameBarrier.CreateCommandBuffer(),
                            BoardEntity = boardEntity,
                            BoardComponent = board,
                            Tiles = Tiles,
                            RandomSeed = random.NextInt()
                        }.Schedule(instantiateTilesJobHandle);
                        this._endFrameBarrier.AddJobHandleForProducer(inputDeps);
                    }
                    else if (this.CurrentPhase == 3)
                    {
                        this.RoomList.Clear();
                        this.CorridorsQueue.Clear();
                    }
                }
            }
            this.CurrentPhase = (this.CurrentPhase + 1) % 4;

            var cleanUpJobHandle = new CleanUpJob
            {
                Chunks = boardChunks
            }.Schedule(inputDeps);
            return cleanUpJobHandle;
        }

        protected override void OnDestroyManager()
        {
            this.RoomList.Dispose();
            this.CorridorsQueue.Dispose();
        }
    }
}
