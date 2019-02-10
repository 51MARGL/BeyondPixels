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

        private struct GetRoomsJob : IJob
        {
            [ReadOnly]
            public BoardComponent Board;

            [WriteOnly]
            public NativeArray<TileComponent> RoomTiles;

            [ReadOnly]
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
                            var tileCount = GetRoomTiles(currentTile, flags, roomTilesIndex, Board.Size.x);

                            Rooms.Add(new RoomComponent
                            {
                                StartTileIndex = roomTilesIndex,
                                TileCount = tileCount
                            });

                            roomTilesIndex += tileCount;
                        }
                    }
            }

            private int GetRoomTiles(TileComponent startTile, NativeArray<int> flags, int roomTilesIndex, int TileStride)
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

                return 0;
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

        private struct CleanUpJob : IJobParallelFor
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [NativeDisableContainerSafetyRestriction]
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<BoardComponent> Boards;

            [NativeDisableContainerSafetyRestriction]
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<Entity> BoardEntities;

            public void Execute(int index)
            {
                CommandBuffer.AddComponent(index, BoardEntities[index], new BoardReadyComponent());
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

        protected override void OnCreateManager()
        {
            _boardSystemBarrier = World.Active.GetOrCreateManager<BoardSystemBarrier>();
            RoomList = new NativeList<RoomComponent>(Allocator.Persistent);
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
                RoomList.Clear();
                var tiles = new NativeArray<TileComponent>(board.Size.x * board.Size.y, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, uint.MaxValue));

                var randomFillBoardJobHandle = new RandomFillBoardJob
                {
                    Tiles = tiles,
                    TileStride = board.Size.x,
                    RandomFillPercent = board.RandomFillPercent,
                    RandomSeed = random.NextInt()
                }.Schedule(board.Size.y, 1, inputDeps);

                var lastGenerationJobHandle = randomFillBoardJobHandle;
                for (int geneation = 0; geneation < 3; geneation++)
                {
                    var calculateNextGenerationJobHandle = new CalculateNextGenerationJob
                    {
                        Tiles = tiles,
                        TileStride = board.Size.x,
                    }.Schedule(tiles.Length, 1, lastGenerationJobHandle);

                    lastGenerationJobHandle = new ProcessNextGenerationJob
                    {
                        Tiles = tiles,
                    }.Schedule(tiles.Length, 32, calculateNextGenerationJobHandle);
                }
                
                var roomTiles = new NativeArray<TileComponent>(tiles.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                var getRoomsJobHandle = new GetRoomsJob
                {
                    Board = board,
                    RoomTiles = roomTiles,
                    Tiles = tiles,
                    Rooms = RoomList
                }.Schedule(lastGenerationJobHandle);

                var finalizeBoardJobHandle = new FinalizeBoardJob
                {
                    Board = board,
                    Tiles = tiles,
                }.Schedule(getRoomsJobHandle);

                inputDeps = new InstantiateTilesJob
                {
                    CommandBuffer = _boardSystemBarrier.CreateCommandBuffer().ToConcurrent(),
                    Tiles = tiles,
                    TileStride = board.Size.x,
                    RoomTiles = roomTiles
                }.Schedule(board.Size.y, 1, finalizeBoardJobHandle);
            }
            var handle = new CleanUpJob
            {
                CommandBuffer = _boardSystemBarrier.CreateCommandBuffer().ToConcurrent(),
                Boards = boards,
                BoardEntities = boardEntities
            }.Schedule(boards.Length, 1, inputDeps);
            _boardSystemBarrier.AddJobHandleForProducer(handle);
            return handle;
        }

        protected override void OnDestroyManager()
        {
            RoomList.Dispose();
        }
    }
}
