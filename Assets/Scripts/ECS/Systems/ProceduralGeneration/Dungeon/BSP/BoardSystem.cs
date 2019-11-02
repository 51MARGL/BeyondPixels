using System.Diagnostics;
using System.IO;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.BSP;

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon.BSP
{
    public class BoardSystem : JobComponentSystem
    {
        [BurstCompile]
        private struct CreateRoomsJob : IJobParallelFor
        {
            public NativeArray<NodeComponent> TreeArray;

            [ReadOnly]
            public int RandomSeed;
            [ReadOnly]
            public BoardComponent Board;

            public void Execute(int index)
            {
                if (this.TreeArray[index].IsNull == 1 || this.TreeArray[index].IsLeaf == 0)
                    return;

                var random = new Random((uint)(this.RandomSeed + index));
                var node = this.TreeArray[index];
                var randomMarginX = random.NextInt(2, 11);
                var randomMarginY = random.NextInt(2, 11);
                var roomX = randomMarginX;
                var roomY = randomMarginY;
                var roomWidth = node.RectBounds.w - roomX - randomMarginX;
                var roomHeight = node.RectBounds.z - roomY - randomMarginY;

                if (roomWidth < 3 || roomHeight < 3)
                {
                    roomX = 2;
                    roomY = 2;
                    roomWidth = node.RectBounds.w - 4;
                    roomHeight = node.RectBounds.z - 4;
                }

                node.Room = new RoomComponent
                {
                    X = roomX + node.RectBounds.x,
                    Y = roomY + node.RectBounds.y,
                    Size = new int2(roomWidth, roomHeight)
                };
                this.TreeArray[index] = node;
            }
        }

        [BurstCompile]
        private struct FindCorridorsForLevelJob : IJobParallelFor
        {
            [WriteOnly]
            public NativeQueue<CorridorComponent>.ParallelWriter Corridors;

            [ReadOnly]
            public NativeArray<NodeComponent> TreeArray;

            [ReadOnly]
            public int RandomSeed;
            [ReadOnly]
            public int StartIndex;

            public void Execute(int index)
            {
                var nodeIndex = this.StartIndex + index;
                if (this.TreeArray[nodeIndex].IsNull == 1
                    || this.TreeArray[nodeIndex].IsLeaf == 1
                    || this.TreeArray[2 * nodeIndex + 1].IsNull == 1
                    || this.TreeArray[2 * nodeIndex + 2].IsNull == 1)
                    return;

                var random = new Random((uint)(this.RandomSeed + index));
                var leftRoomIndex = this.GetRoomIndex(2 * nodeIndex + 1, this.TreeArray.Length);
                var rightRoomIndex = this.GetRoomIndex(2 * nodeIndex + 2, this.TreeArray.Length);

                var leftRoom = this.TreeArray[leftRoomIndex].Room;
                var rightRoom = this.TreeArray[rightRoomIndex].Room;

                this.Corridors.Enqueue(new CorridorComponent
                {
                    Start = new int2(random.NextInt(leftRoom.X + 2, leftRoom.X + leftRoom.Size.x - 1),
                                     random.NextInt(leftRoom.Y + 2, leftRoom.Y + leftRoom.Size.y - 1)),
                    End = new int2(random.NextInt(rightRoom.X + 2, rightRoom.X + rightRoom.Size.x - 1),
                                   random.NextInt(rightRoom.Y + 2, rightRoom.Y + rightRoom.Size.y - 1)),
                });
            }

            public int GetRoomIndex(int index, int currentBest)
            {
                if (index >= currentBest)
                    return currentBest;

                var node = this.TreeArray[index];
                if (node.IsLeaf == 1)
                {
                    if (index < currentBest)
                        return index;
                    else
                        return currentBest;
                }
                if (this.TreeArray[2 * index + 1].IsNull == 0)
                    currentBest = this.GetRoomIndex(2 * index + 1, currentBest);

                if (this.TreeArray[2 * index + 2].IsNull == 0)
                    currentBest = this.GetRoomIndex(2 * index + 2, currentBest);

                return currentBest;
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

        [BurstCompile]
        private struct CreateCorridorsJob : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<TileType> Tiles;

            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<CorridorComponent> Corridors;

            [ReadOnly]
            public int TileStride;

            public void Execute(int index)
            {
                var tileA = this.Corridors[index].Start;
                var tileB = this.Corridors[index].End;

                var dx = tileB.x - tileA.x;
                var dy = tileB.y - tileA.y;

                if (index % 2 == 0)
                {
                    if (dx < 0)
                        this.ConnectHorizontal(tileB.x, tileA.x, tileB.y);
                    else
                        this.ConnectHorizontal(tileA.x, tileB.x, tileA.y);

                    if (dy < 0)
                        this.ConnectVertical(tileB.y, tileA.y, tileB.x);
                    else
                        this.ConnectVertical(tileA.y, tileB.y, tileA.x);
                }
                else
                {
                    if (dy < 0)
                        this.ConnectVertical(tileB.y, tileA.y, tileB.x);
                    else
                        this.ConnectVertical(tileA.y, tileB.y, tileA.x);

                    if (dx < 0)
                        this.ConnectHorizontal(tileB.x, tileA.x, tileB.y);
                    else
                        this.ConnectHorizontal(tileA.x, tileB.x, tileA.y);
                }
            }

            private void ConnectHorizontal(int x1, int x2, int y)
            {
                var dx = x2 - x1;
                for (var x = 0; x < dx; x++)
                {
                    this.Tiles[y * this.TileStride + x1 + x] = TileType.Floor;
                    this.Tiles[(y - 1) * this.TileStride + x1 + x] = TileType.Floor;
                }
            }

            private void ConnectVertical(int y1, int y2, int x)
            {
                var dy = y2 - y1;
                for (var y = 0; y < dy; y++)
                {
                    this.Tiles[(y1 + y) * this.TileStride + x] = TileType.Floor;
                    this.Tiles[(y1 + y) * this.TileStride + x - 1] = TileType.Floor;
                }
            }
        }

        [BurstCompile]
        private struct SetRoomTilesJob : IJobParallelFor
        {
            public NativeArray<NodeComponent> TreeArray;

            [NativeDisableContainerSafetyRestriction]
            [WriteOnly]
            public NativeArray<TileType> Tiles;

            [ReadOnly]
            public int TileStride;

            public void Execute(int index)
            {
                if (this.TreeArray[index].IsNull == 1 || this.TreeArray[index].IsLeaf == 0)
                    return;

                var currentRoom = this.TreeArray[index].Room;
                for (var j = 0; j < currentRoom.Size.x; j++)
                {
                    var xCoord = currentRoom.X + j;

                    if (currentRoom.X == 0 || currentRoom.Y == 0)
                        return;
                    for (var k = 0; k < currentRoom.Size.y; k++)
                    {
                        var yCoord = currentRoom.Y + k;
                        this.Tiles[(yCoord * this.TileStride) + xCoord] = TileType.Floor;
                    }
                }
            }
        }

        [BurstCompile]
        private struct RemoveThinWallsJob : IJob
        {
            [ReadOnly]
            public BoardComponent Board;
            public NativeArray<TileType> Tiles;

            public void Execute()
            {
                this.RemoveThinWalls(this.Board, this.Tiles, this.Board.Size.x);
            }

            private void RemoveThinWalls(BoardComponent board, NativeArray<TileType> tiles, int tilesStride)
            {
                var inconsistentTileDetected = true;
                while (inconsistentTileDetected)
                {
                    inconsistentTileDetected = false;
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
                                var currentTile = tiles[(y * tilesStride) + x];
                                currentTile = TileType.Floor;
                                tiles[(y * tilesStride) + x] = currentTile;
                                inconsistentTileDetected = true;
                            }
                }
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
            public NativeArray<TileType> Tiles;
            [ReadOnly]
            public int RandomSeed;

            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<NodeComponent> TreeArray;

            public void Execute()
            {
                if (this.IsBoardValid())
                {
                    for (var y = 0; y < this.BoardComponent.Size.y; y++)
                        for (var x = 0; x < this.BoardComponent.Size.x; x++)
                        {
                            var entity = this.CommandBuffer.CreateEntity();
                            this.CommandBuffer.AddComponent(entity, new FinalTileComponent
                            {
                                TileType = this.Tiles[(y * this.BoardComponent.Size.x) + x],
                                Position = new int2(x, y)
                            });
                        }

                    this.CommandBuffer.AddComponent(this.BoardEntity, new BoardReadyComponent());
                    var finalBoardEntity = this.CommandBuffer.CreateEntity();
                    this.CommandBuffer.AddComponent(finalBoardEntity, new FinalBoardComponent
                    {
                        Size = this.BoardComponent.Size,
                        RandomSeed = this.BoardComponent.RandomSeed
                    });
                }
                else
                {
                    var random = new Random((uint)this.RandomSeed);

                    var randomSize = new int2(random.NextInt(75, 150), random.NextInt(50, 150));
                    var board = this.CommandBuffer.CreateEntity();
                    this.CommandBuffer.AddComponent(board, new BoardComponent
                    {
                        Size = randomSize,
                        MinRoomSize = this.BoardComponent.MinRoomSize,
                        RandomSeed = random.NextUInt(1, uint.MaxValue)
                    });
                    this.CommandBuffer.DestroyEntity(this.BoardEntity);
                }
            }

            private bool IsBoardValid()
            {
                var freeTilesCount = 0;
                for (var i = 0; i < this.Tiles.Length; i++)
                    if (this.Tiles[i] == TileType.Floor)
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

        private NativeQueue<CorridorComponent> CorridorsQueue;
        private NativeArray<TileType> Tiles;
        private NativeArray<NodeComponent> TreeArray;
        private int CurrentPhase;

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private EntityQuery _boardGroup;
        protected override void OnCreate()
        {
            this._endFrameBarrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
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
                    var random = new Random(board.RandomSeed);
                    if (this.CurrentPhase == 0)
                    {
                        this.Tiles = new NativeArray<TileType>(board.Size.x * board.Size.y, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        for (var j = 0; j < this.Tiles.Length; j++)
                            this.Tiles[j] = TileType.Wall;

                        var bspTree = new BSPTree(board.Size.x, board.Size.y, board.MinRoomSize, ref random);
                        this.TreeArray = bspTree.ToNativeArray();
                        this.CorridorsQueue = new NativeQueue<CorridorComponent>(Allocator.TempJob);
                        var concurrentQueue = this.CorridorsQueue.AsParallelWriter();

                        inputDeps = new CreateRoomsJob
                        {
                            TreeArray = TreeArray,
                            RandomSeed = random.NextInt(),
                            Board = board
                        }.Schedule(this.TreeArray.Length, 1, inputDeps);

                        for (var level = bspTree.Height - 1; level >= 0; level--)
                        {
                            var nodesAmount = (int)math.pow(2, level - 1);
                            inputDeps = new FindCorridorsForLevelJob
                            {
                                TreeArray = TreeArray,
                                RandomSeed = random.NextInt(),
                                StartIndex = nodesAmount - 1,
                                Corridors = concurrentQueue,
                            }.Schedule(nodesAmount, 1, inputDeps);
                        }
                    }
                    else if (this.CurrentPhase == 1)
                    {
                        var corridorsArray = new NativeArray<CorridorComponent>(this.CorridorsQueue.Count, Allocator.TempJob);

                        var corridorsQueueToArrayJobHandle = new CorridorsQueueToArrayJob
                        {
                            CorridorsArray = corridorsArray,
                            CorridorsQueue = this.CorridorsQueue
                        }.Schedule(inputDeps);

                        var createCorridorsJobHandle = new CreateCorridorsJob
                        {
                            Tiles = Tiles,
                            Corridors = corridorsArray,
                            TileStride = board.Size.x
                        }.Schedule(corridorsArray.Length, 1, corridorsQueueToArrayJobHandle);

                        var setRoomTilesJobHandle = new SetRoomTilesJob
                        {
                            TreeArray = TreeArray,
                            Tiles = Tiles,
                            TileStride = board.Size.x
                        }.Schedule(this.TreeArray.Length, 1, inputDeps);

                        inputDeps = new RemoveThinWallsJob
                        {
                            Board = board,
                            Tiles = Tiles
                        }.Schedule(JobHandle.CombineDependencies(setRoomTilesJobHandle, createCorridorsJobHandle));
                    }
                    else if (this.CurrentPhase == 2)
                    {
                        this.CorridorsQueue.Dispose();

                        inputDeps = new TagBoardDoneJob
                        {
                            CommandBuffer = this._endFrameBarrier.CreateCommandBuffer(),
                            BoardEntity = boardEntity,
                            BoardComponent = board,
                            TreeArray = TreeArray,
                            Tiles = Tiles,
                            RandomSeed = random.NextInt()
                        }.Schedule(inputDeps);
                        this._endFrameBarrier.AddJobHandleForProducer(inputDeps);
                    }
                }
            }

            this.CurrentPhase = (this.CurrentPhase + 1) % 3;
            var cleanUpJobHandle = new CleanUpJob
            {
                Chunks = boardChunks
            }.Schedule(inputDeps);
            return cleanUpJobHandle;
        }
    }
}
