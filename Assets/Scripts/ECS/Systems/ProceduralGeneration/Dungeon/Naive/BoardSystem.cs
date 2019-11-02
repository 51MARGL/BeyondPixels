using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.Naive;

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon.Naive
{
    public class BoardSystem : JobComponentSystem
    {
        [BurstCompile]
        private struct CreateRoomsAndCorridorsJob : IJobParallelFor
        {
            [ReadOnly]
            public BoardComponent Board;

            [WriteOnly]
            public NativeQueue<RoomComponent>.ParallelWriter Rooms;

            [WriteOnly]
            public NativeQueue<CorridorComponent>.ParallelWriter Corridors;

            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<CorridorComponent> FirstCorridors;

            [ReadOnly]
            public int RoomCount;

            [ReadOnly]
            public int RandomSeed;

            public void Execute(int index)
            {
                var random = new Random((uint)(this.RandomSeed * (index + 1)));

                var firstCorridor = this.FirstCorridors[index];
                var room = this.CreateRoom(this.Board, firstCorridor, ref random);
                this.Rooms.Enqueue(room);

                for (var i = 0; i < this.RoomCount; i++)
                {
                    var corridor = this.CreateCorridor(room, this.Board, ref random);
                    room = this.CreateRoom(this.Board, corridor, ref random);
                    this.Rooms.Enqueue(room);
                    this.Corridors.Enqueue(corridor);
                }
            }

            private RoomComponent CreateRoom(BoardComponent board, CorridorComponent corridor, ref Unity.Mathematics.Random random)
            {
                var roomWidth = random.NextInt(3, board.MaxRoomSize);
                var roomHeight = random.NextInt(3, board.MaxRoomSize);

                var roomX = 0;
                var roomY = 0;

                switch (corridor.Direction)
                {
                    case Direction.Up:
                        roomHeight = math.clamp(roomHeight, 0, board.Size.y - 2 - corridor.EndY);
                        roomY = corridor.EndY;
                        roomX = random.NextInt(corridor.EndX - roomWidth + 1, corridor.EndX + 1);
                        roomX = math.clamp(roomX, 2, board.Size.x - 2 - roomWidth);
                        break;
                    case Direction.Left:
                        roomWidth = math.clamp(roomWidth, 0, board.Size.x - 2 - corridor.EndX);
                        roomX = corridor.EndX;
                        roomY = random.NextInt(corridor.EndY - roomHeight + 1, corridor.EndY + 1);
                        roomY = math.clamp(roomY, 2, board.Size.y - 2 - roomHeight);
                        break;
                    case Direction.Down:
                        roomHeight = math.clamp(roomHeight, 0, corridor.EndY);
                        roomY = corridor.EndY - roomHeight + 1;
                        roomX = random.NextInt(corridor.EndX - roomWidth + 1, corridor.EndX + 1);
                        roomX = math.clamp(roomX, 2, board.Size.x - 2 - roomWidth);
                        break;
                    case Direction.Right:
                        roomWidth = math.clamp(roomWidth, 0, corridor.EndX);
                        roomX = corridor.EndX - roomWidth + 1;
                        roomY = random.NextInt(corridor.EndY - roomHeight + 1, corridor.EndY + 1);
                        roomY = math.clamp(roomY, 2, board.Size.y - 2 - roomHeight);
                        break;
                }

                return new RoomComponent
                {
                    X = math.clamp(roomX, 2, board.Size.x - 2),
                    Y = math.clamp(roomY, 2, board.Size.y - 2),
                    Size = new int2(roomWidth, roomHeight),
                    EnteringCorridor = corridor.Direction
                };
            }

            private CorridorComponent CreateCorridor(RoomComponent room, BoardComponent board, ref Unity.Mathematics.Random random)
            {
                var direction = (Direction)random.NextInt(0, 4);
                var oppositeDirection = (Direction)(((int)room.EnteringCorridor + 2) % 4);

                if (random.NextInt(0, 100) > 75) //25% chance to go further from center
                {
                    var centerX = (int)math.round(board.Size.x / 2f);
                    var centerY = (int)math.round(board.Size.y / 2f);
                    if (room.X > centerX && room.Y > centerY)
                        direction = random.NextBool() ? Direction.Right : random.NextBool() ? Direction.Up : Direction.Down;
                    else if (room.X < centerX && room.Y > centerY)
                        direction = random.NextBool() ? Direction.Left : random.NextBool() ? Direction.Up : Direction.Down;
                    else if (room.X > centerX && room.Y < centerY)
                        direction = random.NextBool() ? Direction.Right : random.NextBool() ? Direction.Up : Direction.Down;
                    else
                        direction = random.NextBool() ? Direction.Left : random.NextBool() ? Direction.Up : Direction.Down;
                }

                if (direction == oppositeDirection)
                {
                    var directionInt = (int)direction;
                    directionInt++;
                    directionInt = directionInt % 4;
                    direction = (Direction)directionInt;

                }

                var corridorLength = random.NextInt(board.MinCorridorLength, board.MaxCorridorLength);
                var corridorX = 0;
                var corridorY = 0;

                var maxLength = board.MaxCorridorLength;

                switch (direction)
                {
                    case Direction.Up:
                        corridorX = random.NextInt(room.X, room.X + room.Size.x);
                        corridorY = room.Y + room.Size.y - 2;
                        maxLength = board.Size.y - 2 - corridorY;
                        break;
                    case Direction.Left:
                        corridorX = room.X + room.Size.x - 2;
                        corridorY = random.NextInt(room.Y, room.Y + room.Size.y);
                        maxLength = board.Size.x - 2 - corridorX;
                        break;
                    case Direction.Down:
                        corridorX = random.NextInt(room.X, room.X + room.Size.x + 1);
                        corridorY = room.Y;
                        maxLength = corridorY - 2;
                        break;
                    case Direction.Right:
                        corridorX = room.X;
                        corridorY = random.NextInt(room.Y, room.Y + room.Size.y + 1);
                        maxLength = corridorX - 2;
                        break;
                }

                corridorLength = math.clamp(corridorLength, 0, maxLength);

                return new CorridorComponent
                {
                    StartX = corridorX,
                    StartY = corridorY,
                    Length = corridorLength,
                    Direction = direction
                };
            }
        }

        [BurstCompile]
        private struct RoomsQueueToArrayJob : IJob
        {
            [WriteOnly]
            public NativeArray<RoomComponent> RoomsArray;
            public NativeQueue<RoomComponent> RoomsQueue;

            public void Execute()
            {
                var index = 0;
                while (this.RoomsQueue.Count > 0)
                    if (this.RoomsQueue.TryDequeue(out var corridor))
                        this.RoomsArray[index++] = corridor;
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
        private struct SetRoomTilesJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<RoomComponent> Roms;

            [NativeDisableContainerSafetyRestriction]
            [WriteOnly]
            public NativeArray<TileType> Tiles;

            [ReadOnly]
            public int TileStride;

            public void Execute(int index)
            {
                var currentRoom = this.Roms[index];
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
        private struct SetCorridorTilesJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<CorridorComponent> Corridors;

            [NativeDisableContainerSafetyRestriction]
            [WriteOnly]
            public NativeArray<TileType> Tiles;

            [ReadOnly]
            public int TileStride;

            public void Execute(int index)
            {
                var currentCorridor = this.Corridors[index];

                for (var j = 0; j < currentCorridor.Length; j++)
                {
                    var xCoord = currentCorridor.StartX;
                    var yCoord = currentCorridor.StartY;

                    switch (currentCorridor.Direction)
                    {
                        case Direction.Up:
                            yCoord += j;
                            break;
                        case Direction.Left:
                            xCoord += j;
                            break;
                        case Direction.Down:
                            yCoord -= j;
                            break;
                        case Direction.Right:
                            xCoord -= j;
                            break;
                    }

                    this.Tiles[(yCoord * this.TileStride) + xCoord] = TileType.Floor;

                    if (currentCorridor.Direction == Direction.Up || currentCorridor.Direction == Direction.Down)
                        this.Tiles[(yCoord * this.TileStride) + xCoord - 1] = TileType.Floor;
                    else
                        this.Tiles[((yCoord - 1) * this.TileStride) + xCoord] = TileType.Floor;
                }
            }
        }

        [BurstCompile]
        private struct CloseBordersJob : IJob
        {
            [ReadOnly]
            public BoardComponent Board;
            public NativeArray<TileType> Tiles;

            public void Execute()
            {
                var tilesStride = this.Board.Size.x;
                for (var x = 0; x < this.Board.Size.x; x++)
                {
                    this.Tiles[x] = TileType.Wall;
                    this.Tiles[tilesStride + x] = TileType.Wall;
                    this.Tiles[((this.Board.Size.y - 1) * tilesStride) + x] = TileType.Wall;
                    this.Tiles[((this.Board.Size.y - 2) * tilesStride) + x] = TileType.Wall;
                }
                for (var y = 0; y < this.Board.Size.y; y++)
                {
                    this.Tiles[y * tilesStride] = TileType.Wall;
                    this.Tiles[y * tilesStride + 1] = TileType.Wall;
                    this.Tiles[(y * tilesStride) + this.Board.Size.x - 1] = TileType.Wall;
                    this.Tiles[(y * tilesStride) + this.Board.Size.x - 2] = TileType.Wall;
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
            [ReadOnly]
            public int TileStride;

            public void Execute()
            {
                if (this.IsBoardValid())
                {
                    var mapBounds = this.GetBounds();
                    for (var y = 0; y < mapBounds.z; y++)
                        for (var x = 0; x < mapBounds.w; x++)
                        {
                            var entity = this.CommandBuffer.CreateEntity();
                            this.CommandBuffer.AddComponent(entity, new FinalTileComponent
                            {
                                TileType = this.Tiles[((y + mapBounds.y) * this.TileStride) + (x + mapBounds.x)],
                                Position = new int2(x, y)
                            });
                        }

                    this.CommandBuffer.AddComponent(this.BoardEntity, new BoardReadyComponent());
                    var finalBoardEntity = this.CommandBuffer.CreateEntity();
                    this.CommandBuffer.AddComponent(finalBoardEntity, new FinalBoardComponent
                    {
                        Size = new int2(mapBounds.w, mapBounds.z),
                        RandomSeed = this.BoardComponent.RandomSeed
                    });
                }
                else
                {
                    var random = new Random((uint)this.RandomSeed);

                    var randomSize = new int2(random.NextInt(75, 150), random.NextInt(50, 150));
                    var roomCount = (int)(randomSize.x * randomSize.y * random.NextFloat(0.0025f, 0.0035f));
                    var board = this.CommandBuffer.CreateEntity();
                    this.CommandBuffer.AddComponent(board, new BoardComponent
                    {
                        Size = randomSize,
                        RoomCount = roomCount,
                        MaxRoomSize = this.BoardComponent.MaxRoomSize,
                        MaxCorridorLength = this.BoardComponent.MaxCorridorLength,
                        MinCorridorLength = this.BoardComponent.MinCorridorLength,
                        RandomSeed = random.NextUInt(1, uint.MaxValue)
                    });
                    this.CommandBuffer.DestroyEntity(this.BoardEntity);
                }
            }

            private int4 GetBounds()
            {
                var bounds = new int4(this.BoardComponent.Size.x, this.BoardComponent.Size.y, 0, 0);

                for (var y = 0; y < this.BoardComponent.Size.y; y++)
                    for (var x = 0; x < this.TileStride; x++)
                    {
                        if (this.Tiles[y * this.TileStride + x] == TileType.Floor)
                        {
                            if (y > bounds.z)
                                bounds.z = y;
                            if (y < bounds.y)
                                bounds.y = y;
                            if (x > bounds.w)
                                bounds.w = x;
                            if (x < bounds.x)
                                bounds.x = x;
                        }
                    }

                bounds.x = math.clamp(bounds.x - 2, 0, this.BoardComponent.Size.x);
                bounds.y = math.clamp(bounds.y - 2, 0, this.BoardComponent.Size.y);
                bounds.z = math.clamp(bounds.z - bounds.y + 3, 0, this.BoardComponent.Size.y - bounds.y);
                bounds.w = math.clamp(bounds.w - bounds.x + 3, 0, this.BoardComponent.Size.x - bounds.x);
                return bounds;
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
        private NativeQueue<RoomComponent> RoomsQueue;
        private NativeArray<TileType> Tiles;
        private int CurrentPhase;
        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private EntityQuery _boardGroup;

        protected override void OnCreate()
        {
            this.CurrentPhase = 0;
            this._endFrameBarrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
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
                        var roomCount = board.RoomCount;

                        this.Tiles = new NativeArray<TileType>(board.Size.x * board.Size.y, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        this.RoomsQueue = new NativeQueue<RoomComponent>(Allocator.TempJob);
                        this.CorridorsQueue = new NativeQueue<CorridorComponent>(Allocator.TempJob);
                        var firstCorridors = new NativeArray<CorridorComponent>(4, Allocator.TempJob);
                        for (var j = 0; j < this.Tiles.Length; j++)
                            this.Tiles[j] = TileType.Wall;

                        // setup fist room and corridors to all 4 directions
                        var firstRoom = CreateRoom(board, ref random);
                        this.RoomsQueue.Enqueue(firstRoom);
                        for (var c = 0; c < 4; c++)
                        {
                            firstCorridors[c] = this.CreateCorridor(firstRoom, board, ref random, c);
                            this.CorridorsQueue.Enqueue(firstCorridors[c]);
                        }

                        inputDeps = new CreateRoomsAndCorridorsJob
                        {
                            Board = board,
                            Rooms = this.RoomsQueue.AsParallelWriter(),
                            Corridors = this.CorridorsQueue.AsParallelWriter(),
                            FirstCorridors = firstCorridors,
                            RoomCount = roomCount / 4,
                            RandomSeed = random.NextInt()
                        }.Schedule(4, 1, inputDeps);
                    }
                    else if (this.CurrentPhase == 1)
                    {
                        var roomsCount = this.RoomsQueue.Count;
                        var corridorsCount = this.CorridorsQueue.Count;
                        var roomsArray = new NativeArray<RoomComponent>(roomsCount, Allocator.TempJob);
                        var corridorsArray = new NativeArray<CorridorComponent>(corridorsCount, Allocator.TempJob);

                        var roomsQueueToArrayJobHandle = new RoomsQueueToArrayJob
                        {
                            RoomsQueue = this.RoomsQueue,
                            RoomsArray = roomsArray
                        }.Schedule(inputDeps);

                        var corridorsQueueToArrayJobHandle = new CorridorsQueueToArrayJob
                        {
                            CorridorsQueue = this.CorridorsQueue,
                            CorridorsArray = corridorsArray
                        }.Schedule(inputDeps);

                        var setRoomTilesJobHandle = new SetRoomTilesJob
                        {
                            Roms = roomsArray,
                            Tiles = Tiles,
                            TileStride = board.Size.x
                        }.Schedule(roomsCount, 1, roomsQueueToArrayJobHandle);

                        var setCorridorTilesJobHandle = new SetCorridorTilesJob
                        {
                            Corridors = corridorsArray,
                            Tiles = Tiles,
                            TileStride = board.Size.x
                        }.Schedule(corridorsCount, 1, corridorsQueueToArrayJobHandle);

                        var removeThinWallsJobHandle = new RemoveThinWallsJob
                        {
                            Board = board,
                            Tiles = Tiles
                        }.Schedule(JobHandle.CombineDependencies(setRoomTilesJobHandle, setCorridorTilesJobHandle));

                        inputDeps = new CloseBordersJob
                        {
                            Board = board,
                            Tiles = Tiles
                        }.Schedule(removeThinWallsJobHandle);
                    }
                    else if (this.CurrentPhase == 2)
                    {
                        this.CorridorsQueue.Dispose();
                        this.RoomsQueue.Dispose();

                        inputDeps = new TagBoardDoneJob
                        {
                            CommandBuffer = this._endFrameBarrier.CreateCommandBuffer(),
                            BoardEntity = boardEntity,
                            Tiles = Tiles,
                            BoardComponent = board,
                            TileStride = board.Size.x,
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

        private static RoomComponent CreateRoom(BoardComponent board, ref Unity.Mathematics.Random random)
        {
            var roomWidth = random.NextInt(3, board.MaxRoomSize);
            var roomHeight = random.NextInt(3, board.MaxRoomSize);

            var centerX = (int)math.round(board.Size.x / 2f - roomWidth / 2f);
            var centerY = (int)math.round(board.Size.y / 2f - roomHeight / 2f);

            return new RoomComponent
            {
                X = centerX,
                Y = centerY,
                Size = new int2(roomWidth, roomHeight),
                EnteringCorridor = Direction.Up
            };
        }

        private CorridorComponent CreateCorridor(RoomComponent room, BoardComponent board, ref Unity.Mathematics.Random random, int forceDirection)
        {
            var direction = (Direction)forceDirection;

            var corridorLength = random.NextInt(board.MinCorridorLength, board.MaxCorridorLength);
            var corridorX = 0;
            var corridorY = 0;

            var maxLength = board.MaxCorridorLength;

            switch (direction)
            {
                case Direction.Up:
                    corridorX = random.NextInt(room.X, room.X + room.Size.x);
                    corridorY = room.Y + room.Size.y - 2;
                    maxLength = board.Size.y - 2 - corridorY;
                    break;
                case Direction.Left:
                    corridorX = room.X + room.Size.x - 2;
                    corridorY = random.NextInt(room.Y, room.Y + room.Size.y);
                    maxLength = board.Size.x - 2 - corridorX;
                    break;
                case Direction.Down:
                    corridorX = random.NextInt(room.X, room.X + room.Size.x + 1);
                    corridorY = room.Y;
                    maxLength = corridorY - 2;
                    break;
                case Direction.Right:
                    corridorX = room.X;
                    corridorY = random.NextInt(room.Y, room.Y + room.Size.y + 1);
                    maxLength = corridorX - 2;
                    break;
            }

            corridorLength = math.clamp(corridorLength, 0, maxLength);

            return new CorridorComponent
            {
                StartX = corridorX,
                StartY = corridorY,
                Length = corridorLength,
                Direction = direction
            };
        }
    }
}
