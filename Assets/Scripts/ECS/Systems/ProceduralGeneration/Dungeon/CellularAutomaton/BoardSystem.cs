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
                        {
                            wallCount++;
                        }
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

                            if (tileCount > 5)
                                Rooms.Add(new RoomComponent
                                {
                                    StartTileIndex = roomTilesIndex,
                                    TileCount = tileCount
                                });
                            else
                                for (int i = 0; i < tileCount; i++)
                                {
                                    var tile = RoomTiles[roomTilesIndex + i];
                                    var tile2 = Tiles[tile.Position.y * Board.Size.x + tile.Position.x];
                                    tile2.CurrentGenState = TileType.Wall;
                                    Tiles[tile.Position.y * Board.Size.x + tile.Position.x] = tile2;
                                }

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

        //[BurstCompile] 19.1 support only
        private struct ConnectClosestRoomsJob : IJob
        {
            [ReadOnly]
            public NativeArray<TileComponent> RoomTiles;

            [ReadOnly]
            public NativeArray<TileComponent> Tiles;

            [WriteOnly]
            public NativeList<TileComponent> TilesForPassageList;

            public NativeList<RoomComponent> Rooms;

            public void Execute()
            {
                var connectedRoomsTable = new NativeArray<int>(Rooms.Length * Rooms.Length, Allocator.Temp);
                for (int i = 0; i < Rooms.Length; i++)
                    connectedRoomsTable[i * Rooms.Length + i] = 1;

                var initialRoom = Rooms[0];
                initialRoom.IsAccessibleFromMainRoom = 1;
                Rooms[0] = initialRoom;

                ConnectClosestRooms(connectedRoomsTable);
            }

            private void ConnectClosestRooms(NativeArray<int> connectedRoomsTable)
            {
                for (int i = 0; i < Rooms.Length; i++)
                {
                    var bestDistance = (float)int.MaxValue;
                    var bestTileA = this.Tiles[0];
                    var bestTileB = this.Tiles[0];
                    var bestRoomAIndex = 0;
                    var bestRoomBIndex = 0;

                    for (int j = 0; j < Rooms.Length; j++)
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

                    CreatePassage(connectedRoomsTable, bestRoomAIndex, bestRoomBIndex, bestTileA, bestTileB);
                }
            }

            private void CreatePassage(NativeArray<int> connectedRoomsTable, int bestRoomAIndex, int bestRoomBIndex, TileComponent bestTileA, TileComponent bestTileB)
            {
                connectedRoomsTable[bestRoomAIndex * Rooms.Length + bestRoomBIndex] = 1;
                connectedRoomsTable[bestRoomBIndex * Rooms.Length + bestRoomAIndex] = 1;
                this.TilesForPassageList.Add(bestTileA);
                this.TilesForPassageList.Add(bestTileB);
            }
        }

        //[BurstCompile] 19.1 support only
        private struct CreatePassagesJob : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly]
            public NativeArray<TileComponent> Tiles;

            [ReadOnly]
            public NativeList<TileComponent> TilesForPassageList;

            [ReadOnly]
            public int TileStride;

            [ReadOnly]
            public int PassageRadius;

            public void Execute(int index)
            {
                if (index * 2 + 1 > TilesForPassageList.Length)
                    return;

                var tileA = TilesForPassageList[(index * 2)];
                var tileB = TilesForPassageList[(index * 2) + 1];
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
                        if (x * x + y * y <= radius * radius)
                        {
                            var drawX = tile.Position.x + x;
                            var drawY = tile.Position.y + y;

                            if (drawY * this.TileStride + drawX >= 0 && drawY * this.TileStride + drawX < Tiles.Length)
                            {
                                var currTile = this.Tiles[drawY * this.TileStride + drawX];
                                currTile.CurrentGenState = TileType.Floor;
                                this.Tiles[drawY * this.TileStride + drawX] = currTile;
                            }
                        }
            }
        }

        [BurstCompile]
        private struct FinalizeBoardJob : IJob
        {
            [ReadOnly]
            public BoardComponent Board;
            public NativeArray<TileComponent> Tiles;

            public void Execute()
            {
                //RemoveThinWalls(Board, Tiles, Board.Size.x);
                CloseBorders(Board, Tiles, Board.Size.x);
            }

            private void CloseBorders(BoardComponent board, NativeArray<TileComponent> tiles, int tilesStride)
            {
                TileComponent currentTile;
                for (int x = 0; x < board.Size.x; x++)
                {
                    currentTile = tiles[x];
                    currentTile.CurrentGenState = TileType.Wall;
                    tiles[x] = currentTile;

                    currentTile = tiles[tilesStride + x];
                    currentTile.CurrentGenState = TileType.Wall;
                    tiles[tilesStride + x] = currentTile;

                    currentTile = tiles[((board.Size.y - 1) * tilesStride) + x];
                    currentTile.CurrentGenState = TileType.Wall;
                    tiles[((board.Size.y - 1) * tilesStride) + x] = currentTile;

                    currentTile = tiles[((board.Size.y - 2) * tilesStride) + x];
                    currentTile.CurrentGenState = TileType.Wall;
                    tiles[((board.Size.y - 2) * tilesStride) + x] = currentTile;
                }
                for (int y = 0; y < board.Size.y; y++)
                {
                    currentTile = tiles[y * tilesStride];
                    currentTile.CurrentGenState = TileType.Wall;
                    tiles[y * tilesStride] = currentTile;

                    currentTile = tiles[y * tilesStride + 1];
                    currentTile.CurrentGenState = TileType.Wall;
                    tiles[y * tilesStride + 1] = currentTile;

                    currentTile = tiles[(y * tilesStride) + board.Size.x - 1];
                    currentTile.CurrentGenState = TileType.Wall;
                    tiles[(y * tilesStride) + board.Size.x - 1] = currentTile;

                    currentTile = tiles[(y * tilesStride) + board.Size.x - 2];
                    currentTile.CurrentGenState = TileType.Wall;
                    tiles[(y * tilesStride) + board.Size.x - 2] = currentTile;
                }
            }

            private void RemoveThinWalls(BoardComponent board, NativeArray<TileComponent> tiles, int tilesStride)
            {
                var inconsistentTileDetected = false;
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
                if (inconsistentTileDetected)
                    RemoveThinWalls(board, tiles, tilesStride);
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

            //Index represents row
            public void Execute(int index)
            {
                for (int x = 0; x < TileStride; x++)
                {
                    var entity = CommandBuffer.CreateEntity(index);
                    CommandBuffer.AddComponent(index, entity, Tiles[(index * TileStride) + x]);
                }
            }
        }

        private struct TagBoardDoneJob : IJobParallelFor
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [NativeDisableContainerSafetyRestriction]
            [ReadOnly]
            public NativeArray<BoardComponent> Boards;

            [NativeDisableContainerSafetyRestriction]
            [ReadOnly]
            public NativeArray<Entity> BoardEntities;

            public void Execute(int index)
            {
                CommandBuffer.AddComponent(index, BoardEntities[index], new BoardReadyComponent());
            }
        }

        [BurstCompile]
        private struct CleanUpJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<BoardComponent> Boards;

            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<Entity> BoardEntities;

            public void Execute(int index) { }
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
        private NativeList<TileComponent> TilesForPassageList;
        private NativeArray<TileComponent> Tiles;
        private NativeArray<TileComponent> RoomTiles;
        private bool FirstPhaseUpdate;

        protected override void OnCreateManager()
        {
            _boardSystemBarrier = World.Active.GetOrCreateManager<BoardSystemBarrier>();
            RoomList = new NativeList<RoomComponent>(Allocator.Persistent);
            TilesForPassageList = new NativeList<TileComponent>(Allocator.Persistent);
            FirstPhaseUpdate = true;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var boards = new NativeArray<BoardComponent>(_data.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var boardEntities = new NativeArray<Entity>(_data.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < _data.Length; i++)
            {
                boards[i] = _data.BoardComponents[i];
                boardEntities[i] = _data.EntityArray[i];
            }

            for (int i = 0; i < _data.Length; i++)
            {
                var board = boards[i];
                var boardEntity = boardEntities[i];
                if (FirstPhaseUpdate)
                {
                    Tiles = new NativeArray<TileComponent>(board.Size.x * board.Size.y, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    RoomTiles = new NativeArray<TileComponent>(Tiles.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, uint.MaxValue));
                    RoomList.Clear();
                    TilesForPassageList.Clear();

                    var randomFillBoardJobHandle = new RandomFillBoardJob
                    {
                        Tiles = Tiles,
                        TileStride = board.Size.x,
                        RandomFillPercent = board.RandomFillPercent,
                        RandomSeed = random.NextInt()
                    }.Schedule(board.Size.y, 1, inputDeps);

                    var lastGenerationJobHandle = randomFillBoardJobHandle;
                    for (int geneation = 0; geneation < 3; geneation++)
                    {
                        var calculateNextGenerationJobHandle = new CalculateNextGenerationJob
                        {
                            Tiles = Tiles,
                            TileStride = board.Size.x,
                        }.Schedule(Tiles.Length, 1, lastGenerationJobHandle);

                        lastGenerationJobHandle = new ProcessNextGenerationJob
                        {
                            Tiles = Tiles,
                        }.Schedule(Tiles.Length, 32, calculateNextGenerationJobHandle);
                    }

                    inputDeps = new GetRoomsJob
                    {
                        Board = board,
                        RoomTiles = RoomTiles,
                        Tiles = Tiles,
                        Rooms = RoomList
                    }.Schedule(lastGenerationJobHandle);
                }
                else
                {
                    var roomCount = RoomList.Length;
                    var connectRoomsJobHandle = new ConnectClosestRoomsJob
                    {
                        RoomTiles = RoomTiles,
                        Tiles = Tiles,
                        Rooms = RoomList,
                        TilesForPassageList = TilesForPassageList
                    }.Schedule(inputDeps);

                    var createPassagesJobHandle = new CreatePassagesJob
                    {
                        Tiles = Tiles,
                        TilesForPassageList = TilesForPassageList,
                        TileStride = board.Size.x,
                        PassageRadius = board.PassRadius
                    }.Schedule(roomCount * 2, 1, connectRoomsJobHandle); //dont know TilesForPassageList.length now

                    var finalizeBoardJobHandle = new FinalizeBoardJob
                    {
                        Board = board,
                        Tiles = Tiles,
                    }.Schedule(createPassagesJobHandle);

                    var instantiateTilesJobHandle = new InstantiateTilesJob
                    {
                        CommandBuffer = _boardSystemBarrier.CreateCommandBuffer().ToConcurrent(),
                        Tiles = Tiles,
                        TileStride = board.Size.x,
                        RoomTiles = RoomTiles
                    }.Schedule(board.Size.y, 1, finalizeBoardJobHandle);

                    inputDeps = new TagBoardDoneJob
                    {
                        CommandBuffer = _boardSystemBarrier.CreateCommandBuffer().ToConcurrent(),
                        Boards = boards,
                        BoardEntities = boardEntities
                    }.Schedule(boards.Length, 1, instantiateTilesJobHandle);
                    _boardSystemBarrier.AddJobHandleForProducer(inputDeps);
                }
            }
            FirstPhaseUpdate = !FirstPhaseUpdate;

            return new CleanUpJob
            {
                Boards = boards,
                BoardEntities = boardEntities
            }.Schedule(boards.Length, 1, inputDeps);
        }

        protected override void OnDestroyManager()
        {
            RoomList.Dispose();
            TilesForPassageList.Dispose();
        }
    }
}
