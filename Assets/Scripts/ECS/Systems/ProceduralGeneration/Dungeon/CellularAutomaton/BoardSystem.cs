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
        private struct RnadomFillBoard : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly]
            public NativeArray<TileType> Tiles;

            [ReadOnly]
            public int RandomFillPercent;

            [ReadOnly]
            public int TileStride;

            public void Execute(int index)
            {
                var random = new Random((uint)index + 1);
                for (int x = 0; x < TileStride; x++)
                    if (random.NextInt(0, 100) > RandomFillPercent)
                            Tiles[index * TileStride + x] = TileType.Floor;
            }
        }


        [BurstCompile]
        private struct FinalizeBoardJob : IJob
        {
            [ReadOnly]
            public BoardComponent Board;
            public NativeArray<TileType> Tiles;

            public void Execute()
            {
                RemoveThinWalls(Board, Tiles, Board.Size.x);
                CloseBorders(Board, Tiles, Board.Size.x);
            }

            private void CloseBorders(BoardComponent board, NativeArray<TileType> tiles, int tilesStride)
            {
                for (int x = 0; x < board.Size.x; x++)
                {
                    tiles[x] = TileType.Wall;
                    tiles[tilesStride + x] = TileType.Wall;
                    tiles[((board.Size.y - 1) * tilesStride) + x] = TileType.Wall;
                    tiles[((board.Size.y - 2) * tilesStride) + x] = TileType.Wall;
                }
                for (int y = 0; y < board.Size.y; y++)
                {
                    tiles[y * tilesStride] = TileType.Wall;
                    tiles[y * tilesStride + 1] = TileType.Wall;
                    tiles[(y * tilesStride) + board.Size.x - 1] = TileType.Wall;
                    tiles[(y * tilesStride) + board.Size.x - 2] = TileType.Wall;
                }
            }

            private void RemoveThinWalls(BoardComponent board, NativeArray<TileType> tiles, int tilesStride)
            {
                var inconsistentTileDetected = false;
                for (var y = 1; y < board.Size.y - 1; y++)
                    for (var x = 1; x < board.Size.x - 1; x++)
                        if (tiles[(y * tilesStride) + x] == TileType.Wall
                            && ((tiles[(y * tilesStride) + x + 1] == TileType.Floor && tiles[(y * tilesStride) + x - 1] == TileType.Floor) // pattern -> -
                                || (tiles[((y + 1) * tilesStride) + x] == TileType.Floor && tiles[((y - 1) * tilesStride) + x] == TileType.Floor) // pattern -> |
                                || ((tiles[((y + 1) * tilesStride) + x + 1] == TileType.Floor && tiles[((y - 1) * tilesStride) + x - 1] == TileType.Floor)// pattern -> /
                                    && (tiles[((y + 1) * tilesStride) + x - 1] == TileType.Wall && tiles[((y - 1) * tilesStride) + x + 1] == TileType.Wall))// pattern -> \
                                || ((tiles[((y + 1) * tilesStride) + x + 1] == TileType.Wall && tiles[((y - 1) * tilesStride) + x - 1] == TileType.Wall)// pattern -> /
                                    && (tiles[((y + 1) * tilesStride) + x - 1] == TileType.Floor && tiles[((y - 1) * tilesStride) + x + 1] == TileType.Floor)) // pattern -> \
                                || ((tiles[((y + 1) * tilesStride) + x + 1] == TileType.Wall) && (tiles[((y - 1) * tilesStride) + x - 1] == TileType.Wall)
                                    && tiles[((y + 1) * tilesStride) + x] == TileType.Floor && tiles[(y * tilesStride) + x + 1] == TileType.Floor)
                                || ((tiles[((y + 1) * tilesStride) + x - 1] == TileType.Wall) && (tiles[((y - 1) * tilesStride) + x + 1] == TileType.Wall)
                                    && tiles[((y + 1) * tilesStride) + x] == TileType.Floor && tiles[(y * tilesStride) + x - 1] == TileType.Floor)
                            ))
                        {
                            tiles[(y * tilesStride) + x] = TileType.Floor;
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
            public NativeArray<TileType> Tiles;

            [ReadOnly]
            public int TileStride;

            //Index represents row
            public void Execute(int index)
            {
                for (int x = 0; x < TileStride; x++)
                {
                    var entity = CommandBuffer.CreateEntity(index);
                    CommandBuffer.AddComponent(index, entity, new TileComponent
                    {
                        TileType = Tiles[(index * TileStride) + x],
                        Position = new int2(x, index)
                    });
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

        protected override void OnCreateManager()
        {
            _boardSystemBarrier = World.Active.GetOrCreateManager<BoardSystemBarrier>();
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

                var tiles = new NativeArray<TileType>(board.Size.x * board.Size.y, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                for (int j = 0; j < tiles.Length; j++)
                    tiles[j] = TileType.Wall;


                var randomFillMapJobHandle = new RnadomFillBoard
                {
                    Tiles = tiles,
                    RandomFillPercent = board.RandomFillPercent,
                    TileStride = board.Size.x
                }.Schedule(board.Size.y, 1, inputDeps);

                var finalizeBoardJobHandle = new FinalizeBoardJob
                {
                    Board = board,
                    Tiles = tiles,
                }.Schedule(randomFillMapJobHandle);

                inputDeps = new InstantiateTilesJob
                {
                    CommandBuffer = _boardSystemBarrier.CreateCommandBuffer().ToConcurrent(),
                    Tiles = tiles,
                    TileStride = board.Size.x
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
    }
}
