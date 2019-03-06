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
        [DisableAutoCreation]
        private class BoardSystemBarrier : BarrierSystem { }

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
                var random = new Random((uint)(RandomSeed * (index + 1)));
                for (int x = 0; x < TileStride; x++)
                    if (random.NextInt(0, 100) > RandomFillPercent)
                        Tiles[index * TileStride + x] = new TileComponent
                        {
                            Position = new int2(x, index),
                            CurrentGenState = TileType.Floor
                        };
                    else
                        Tiles[index * TileStride + x] = new TileComponent
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
                var currentTile = Tiles[index];

                currentTile.NextGenState =
                    GetDeadNeighborsCount(currentTile.Position.x, currentTile.Position.y) > 4
                        ? TileType.Wall
                        : TileType.Floor;

                Tiles[index] = currentTile;
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
                var currentTile = Tiles[index];
                currentTile.CurrentGenState = currentTile.NextGenState;
                Tiles[index] = currentTile;
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
                var flags = new NativeArray<int>(Tiles.Length, Allocator.Temp);
                var roomTilesIndex = 0;
                for (int y = 0; y < Board.Size.y; y++)
                    for (int x = 0; x < Board.Size.x; x++)
                    {
                        var currentTile = Tiles[y * Board.Size.x + x];
                        if (flags[y * Board.Size.x + x] == 0
                            && currentTile.CurrentGenState == TileType.Floor)
                        {
                            var tileCount = AddRoomTiles(currentTile, flags, roomTilesIndex, Board.Size.x);

                            Rooms.Add(new RoomComponent
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
                                && flags[currIndex] == 0 && Tiles[currIndex].CurrentGenState == TileType.Floor)
                            {
                                flags[currIndex] = 1;
                                queue.Enqueue(Tiles[currIndex]);
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
                var widthStep = Board.Size.x / (Board.Size.x / 50);
                var heightStep = Board.Size.y / (Board.Size.y / 50);
                var currentIndex = 0;
                for (int y = 0; y < Board.Size.y; y += heightStep)
                {
                    for (int x = 0; x < Board.Size.x; x += widthStep)
                        for (int i = currentIndex; i < Rooms.Length; i++)
                            if (RoomTiles[Rooms[i].StartTileIndex].Position.x >= x
                                && RoomTiles[Rooms[i].StartTileIndex].Position.x <= x + widthStep
                                && RoomTiles[Rooms[i].StartTileIndex].Position.y >= y
                                && RoomTiles[Rooms[i].StartTileIndex].Position.y <= y + heightStep)
                            {
                                var tmp = Rooms[currentIndex];
                                Rooms[currentIndex++] = Rooms[i];
                                Rooms[i] = tmp;
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
                MarkClosestRoomsConnections(ConnectedRoomsTable, index * RoomsChunkSize, RoomsChunkSize);
            }

            private void MarkClosestRoomsConnections(NativeArray<int> connectedRoomsTable, int roomsStartIndex, int roomsChunkSize)
            {
                for (int i = roomsStartIndex; i < roomsStartIndex + roomsChunkSize && i < Rooms.Length; i++)
                {
                    var bestDistance = (float)int.MaxValue;
                    var bestTileA = new TileComponent();
                    var bestTileB = new TileComponent();
                    var bestRoomAIndex = 0;
                    var bestRoomBIndex = 0;

                    for (int j = roomsStartIndex; j < roomsStartIndex + roomsChunkSize && j < Rooms.Length; j++)
                    {
                        if (connectedRoomsTable[i * Rooms.Length + j] == 1)
                            continue;

                        for (int tileIndexA = 0; tileIndexA < Rooms[i].TileCount; tileIndexA++)
                        {
                            for (int tileIndexB = 0; tileIndexB < Rooms[j].TileCount; tileIndexB++)
                            {
                                var tileA = this.RoomTiles[Rooms[i].StartTileIndex + tileIndexA];
                                var tileB = this.RoomTiles[Rooms[j].StartTileIndex + tileIndexB];
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

                    MarkRoomsConnection(connectedRoomsTable, bestRoomAIndex, bestRoomBIndex, bestTileA, bestTileB, roomsStartIndex, roomsChunkSize);
                }
            }

            private void MarkRoomsConnection(NativeArray<int> connectedRoomsTable, int bestRoomAIndex, int bestRoomBIndex, TileComponent bestTileA, TileComponent bestTileB, int roomsStartIndex, int roomsChunkSize)
            {
                var connectedTableCopy = new NativeArray<int>(connectedRoomsTable.Length, Allocator.Temp);
                connectedRoomsTable.CopyTo(connectedTableCopy);
                for (int i = 0; i < roomsStartIndex + roomsChunkSize && i < Rooms.Length; i++)
                {
                    if (connectedTableCopy[bestRoomAIndex * Rooms.Length + i] == 1)
                        for (int j = 0; j < roomsStartIndex + roomsChunkSize && j < Rooms.Length; j++)
                            if (connectedTableCopy[bestRoomBIndex * Rooms.Length + j] == 1)
                            {
                                connectedRoomsTable[i * Rooms.Length + j] = 1;
                                connectedRoomsTable[j * Rooms.Length + i] = 1;
                            }
                    if (connectedTableCopy[bestRoomBIndex * Rooms.Length + i] == 1)
                        for (int j = 0; j < Rooms.Length; j++)
                            if (connectedTableCopy[bestRoomAIndex * Rooms.Length + j] == 1)
                            {
                                connectedRoomsTable[i * Rooms.Length + j] = 1;
                                connectedRoomsTable[j * Rooms.Length + i] = 1;
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
                MarkNotAccessibleRoomsConnections(ConnectedRoomsTable);
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

                    for (int i = 0; i < Rooms.Length; i++)
                    {
                        if (connectedRoomsTable[i] == 1)
                        {
                            connectedRoomList.Add(Rooms[i]);
                            connectedRoomIndexList.Add(i);
                        }
                        else
                        {
                            disconnectedRoomList.Add(Rooms[i]);
                            disconnectedRoomIndexList.Add(i);
                        }
                    }

                    for (int i = 0; i < disconnectedRoomList.Length; i++)
                    {
                        for (int j = 0; j < connectedRoomList.Length; j++)
                        {
                            if (connectedRoomsTable[disconnectedRoomIndexList[i] * Rooms.Length + connectedRoomIndexList[j]] == 1)
                                continue;

                            for (int tileIndexA = 0; tileIndexA < disconnectedRoomList[i].TileCount; tileIndexA++)
                            {
                                for (int tileIndexB = 0; tileIndexB < connectedRoomList[j].TileCount; tileIndexB++)
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
                        MarkRoomsConnection(connectedRoomsTable, bestRoomAIndex, bestRoomBIndex, bestTileA, bestTileB);
                }
            }

            private void MarkRoomsConnection(NativeArray<int> connectedRoomsTable, int bestRoomAIndex, int bestRoomBIndex, TileComponent bestTileA, TileComponent bestTileB)
            {
                var connectedTableCopy = new NativeArray<int>(connectedRoomsTable.Length, Allocator.Temp);
                connectedRoomsTable.CopyTo(connectedTableCopy);
                for (int i = 0; i < Rooms.Length; i++)
                {
                    if (connectedTableCopy[bestRoomAIndex * Rooms.Length + i] == 1)
                        for (int j = 0; j < Rooms.Length; j++)
                            if (connectedTableCopy[bestRoomBIndex * Rooms.Length + j] == 1)
                            {
                                connectedRoomsTable[i * Rooms.Length + j] = 1;
                                connectedRoomsTable[j * Rooms.Length + i] = 1;
                            }
                    if (connectedTableCopy[bestRoomBIndex * Rooms.Length + i] == 1)
                        for (int j = 0; j < Rooms.Length; j++)
                            if (connectedTableCopy[bestRoomAIndex * Rooms.Length + j] == 1)
                            {
                                connectedRoomsTable[i * Rooms.Length + j] = 1;
                                connectedRoomsTable[j * Rooms.Length + i] = 1;
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
                int index = 0;
                while (CorridorsQueue.Count > 0)
                    if (CorridorsQueue.TryDequeue(out var corridor))
                        CorridorsArray[index++] = corridor;
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
                var tileA = Corridors[index].Start;
                var tileB = Corridors[index].End;
                var line = GetLine(tileA, tileB);
                for (int i = 0; i < line.Length; i++)
                    ClearPass(line[i], PassageRadius);

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
                var tilesStride = Board.Size.x;
                for (int x = 0; x < Board.Size.x; x++)
                {
                    currentTile = Tiles[x];
                    currentTile.CurrentGenState = TileType.Wall;
                    Tiles[x] = currentTile;

                    currentTile = Tiles[tilesStride + x];
                    currentTile.CurrentGenState = TileType.Wall;
                    Tiles[tilesStride + x] = currentTile;

                    currentTile = Tiles[((Board.Size.y - 1) * tilesStride) + x];
                    currentTile.CurrentGenState = TileType.Wall;
                    Tiles[((Board.Size.y - 1) * tilesStride) + x] = currentTile;

                    currentTile = Tiles[((Board.Size.y - 2) * tilesStride) + x];
                    currentTile.CurrentGenState = TileType.Wall;
                    Tiles[((Board.Size.y - 2) * tilesStride) + x] = currentTile;
                }
                for (int y = 0; y < Board.Size.y; y++)
                {
                    currentTile = Tiles[y * tilesStride];
                    currentTile.CurrentGenState = TileType.Wall;
                    Tiles[y * tilesStride] = currentTile;

                    currentTile = Tiles[y * tilesStride + 1];
                    currentTile.CurrentGenState = TileType.Wall;
                    Tiles[y * tilesStride + 1] = currentTile;

                    currentTile = Tiles[(y * tilesStride) + Board.Size.x - 1];
                    currentTile.CurrentGenState = TileType.Wall;
                    Tiles[(y * tilesStride) + Board.Size.x - 1] = currentTile;

                    currentTile = Tiles[(y * tilesStride) + Board.Size.x - 2];
                    currentTile.CurrentGenState = TileType.Wall;
                    Tiles[(y * tilesStride) + Board.Size.x - 2] = currentTile;
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
                RemoveThinWalls(Board, Tiles, Board.Size.x);
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
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<TileComponent> Tiles;

            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<TileComponent> RoomTiles;

            [ReadOnly]
            public int TileStride;

            public void Execute(int index)
            {
                for (int x = 0; x < TileStride; x++)
                {
                    var entity = CommandBuffer.CreateEntity(index);
                    CommandBuffer.AddComponent(index, entity, Tiles[(index * TileStride) + x]);
                }
            }
        }

        private struct TagBoardDoneJob : IJob
        {
            public EntityCommandBuffer CommandBuffer;
            [ReadOnly]
            public Entity BoardEntity;

            public void Execute()
            {
                CommandBuffer.AddComponent(BoardEntity, new BoardReadyComponent());
            }
        }

        private struct Data
        {
            public readonly int Length;
            public ComponentDataArray<BoardComponent> BoardComponents;
            public SubtractiveComponent<BoardReadyComponent> BoardReadyComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private Data _data;

        private BoardSystemBarrier _boardSystemBarrier;
        private NativeList<RoomComponent> RoomList;
        private NativeQueue<CorridorComponent> CorridorsQueue;
        private NativeArray<TileComponent> Tiles;
        private NativeArray<TileComponent> RoomTiles;
        private int CurrentPhase;

        protected override void OnCreateManager()
        {
            _boardSystemBarrier = World.Active.GetOrCreateManager<BoardSystemBarrier>();
            RoomList = new NativeList<RoomComponent>(Allocator.Persistent);
            CorridorsQueue = new NativeQueue<CorridorComponent>(Allocator.Persistent);
            CurrentPhase = 0;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            for (int i = 0; i < _data.Length; i++)
            {
                var board = _data.BoardComponents[i];
                if (CurrentPhase == 0)
                {
                    Tiles = new NativeArray<TileComponent>(board.Size.x * board.Size.y, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    RoomTiles = new NativeArray<TileComponent>(Tiles.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    var random = new Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmss").GetHashCode());
                    RoomList.Clear();
                    CorridorsQueue.Clear();

                    var randomFillBoardJobHandle = new RandomFillBoardJob
                    {
                        Tiles = Tiles,
                        TileStride = board.Size.x,
                        RandomFillPercent = board.RandomFillPercent,
                        RandomSeed = random.NextInt()
                    }.Schedule(board.Size.y, 1, inputDeps);

                    var lastGenerationJobHandle = randomFillBoardJobHandle;
                    for (int geneation = 0; geneation < 5; geneation++)
                    {
                        var calculateNextGenerationJobHandle = new CalculateNextGenerationJob
                        {
                            Tiles = Tiles,
                            TileStride = board.Size.x
                        }.Schedule(Tiles.Length, 1, lastGenerationJobHandle);

                        lastGenerationJobHandle = new ProcessNextGenerationJob
                        {
                            Tiles = Tiles
                        }.Schedule(Tiles.Length, 32, calculateNextGenerationJobHandle);
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
                else if (CurrentPhase == 1)
                {
                    var roomCount = RoomList.Length;
                    var roomsChunkSize = 10;
                    var concurrentQueue = CorridorsQueue.ToConcurrent();
                    var connectedRoomsTable = new NativeArray<int>(roomCount * roomCount, Allocator.TempJob);

                    for (int roomIndex = 0; roomIndex < roomCount; roomIndex++)
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
                else if (CurrentPhase == 2)
                {
                    var corridorsArray = new NativeArray<CorridorComponent>(CorridorsQueue.Count, Allocator.TempJob);

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
                    }.Schedule(Tiles.Length, 1, createCorridorsJobHandle);

                    var lastGenerationJobHandle = new ProcessNextGenerationJob
                    {
                        Tiles = Tiles,
                    }.Schedule(Tiles.Length, 32, calculateNextGenerationJobHandle);

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
                        CommandBuffer = _boardSystemBarrier.CreateCommandBuffer().ToConcurrent(),
                        Tiles = Tiles,
                        TileStride = board.Size.x,
                        RoomTiles = RoomTiles
                    }.Schedule(board.Size.y, 1, closeBordersJobHandle);

                    inputDeps = new TagBoardDoneJob
                    {
                        CommandBuffer = _boardSystemBarrier.CreateCommandBuffer(),
                        BoardEntity = _data.EntityArray[i]
                    }.Schedule(instantiateTilesJobHandle);
                    _boardSystemBarrier.AddJobHandleForProducer(inputDeps);
                }
            }
            CurrentPhase = (CurrentPhase + 1) % 3;

            return inputDeps;
        }

        protected override void OnDestroyManager()
        {
            RoomList.Dispose();
            CorridorsQueue.Dispose();
        }
    }
}
