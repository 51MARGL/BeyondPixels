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

            [NativeDisableParallelForRestriction]
            public NativeArray<RoomComponent> Rooms;

            [NativeDisableParallelForRestriction]
            public NativeArray<CorridorComponent> Corridors;

            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<BatchData> Data;

            [ReadOnly]
            public int RandomSeed;

            public void Execute(int index)
            {
                var batch = this.Data[index];
                var random = new Random((uint)(this.RandomSeed * (index + 1)));

                this.Rooms[batch.FirstRoomIndex] =
                    this.CreateRoom(this.Board, this.Corridors[batch.FirstCorridorIndex], ref random);
                this.Corridors[3 + batch.FirstRoomIndex] =
                    this.CreateCorridor(this.Rooms[batch.FirstRoomIndex], this.Board, ref random);

                for (int i = batch.FirstRoomIndex + 1, j = 4 + batch.FirstRoomIndex; i < batch.LastRoomIndex; i++, j++)
                {
                    this.Rooms[i] = this.CreateRoom(this.Board, this.Corridors[j - 1], ref random);
                    if (j < this.Corridors.Length)
                        this.Corridors[j] = this.CreateCorridor(this.Rooms[i], this.Board, ref random);
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

        private struct InstantiateTilesJob : IJobParallelFor
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            public NativeArray<TileType> Tiles;

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
                            TileType = this.Tiles[(index * this.TileStride) + x],
                            Position = new int2(x, index)
                        });
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

                    var randomSize = new int2(random.NextInt(75, 125), random.NextInt(50, 125));
                    var roomCount = (int)math.log2(randomSize.x * randomSize.y / 100) * random.NextInt(5, 11);
                    var board = this.CommandBuffer.CreateEntity();
                    this.CommandBuffer.AddComponent(board, new BoardComponent
                    {
                        Size = randomSize,
                        RoomCount = roomCount,
                        MaxRoomSize = this.BoardComponent.MaxRoomSize,
                        MaxCorridorLength = this.BoardComponent.MaxCorridorLength,
                        MinCorridorLength = this.BoardComponent.MinCorridorLength
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

        private struct BatchData
        {
            public int FirstRoomIndex;
            public int LastRoomIndex;
            public int FirstCorridorIndex;
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private EntityQuery _boardGroup;
        protected override void OnCreate()
        {
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

                    var roomCount = board.RoomCount;

                    var tiles = new NativeArray<TileType>(board.Size.x * board.Size.y, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    var rooms = new NativeArray<RoomComponent>(roomCount, Allocator.TempJob);
                    var corridors = new NativeArray<CorridorComponent>(roomCount + 2, Allocator.TempJob);

                    for (var j = 0; j < tiles.Length; j++)
                        tiles[j] = TileType.Wall;

                    // setup fist room and corridors to all 4 directions
                    var random = new Random((uint)System.Guid.NewGuid().GetHashCode());
                    rooms[0] = CreateRoom(board, ref random);
                    corridors[0] = this.CreateCorridor(rooms[0], board, ref random, 0);
                    corridors[1] = this.CreateCorridor(rooms[0], board, ref random, 1);
                    corridors[2] = this.CreateCorridor(rooms[0], board, ref random, 2);
                    corridors[3] = this.CreateCorridor(rooms[0], board, ref random, 3);

                    var batch = new NativeArray<BatchData>(4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (int j = 0, k = batch.Length + 1; j < batch.Length; j++, k--)
                        batch[j] = new BatchData
                        {
                            FirstRoomIndex = k > batch.Length ? 1 : rooms.Length / k,
                            LastRoomIndex = rooms.Length / (k - 1),
                            FirstCorridorIndex = j
                        };

                    var CreateRoomsAndCorridorsJobHandle = new CreateRoomsAndCorridorsJob
                    {
                        Board = board,
                        Rooms = rooms,
                        Corridors = corridors,
                        Data = batch,
                        RandomSeed = random.NextInt()
                    }.Schedule(batch.Length, 1, inputDeps);

                    var setRoomTilesJobHandle = new SetRoomTilesJob
                    {
                        Roms = rooms,
                        Tiles = tiles,
                        TileStride = board.Size.x
                    }.Schedule(rooms.Length, 1, CreateRoomsAndCorridorsJobHandle);

                    var setCorridorTilesJobHandle = new SetCorridorTilesJob
                    {
                        Corridors = corridors,
                        Tiles = tiles,
                        TileStride = board.Size.x
                    }.Schedule(corridors.Length, 1, CreateRoomsAndCorridorsJobHandle);

                    var removeThinWallsJobHandle = new RemoveThinWallsJob
                    {
                        Board = board,
                        Tiles = tiles
                    }.Schedule(JobHandle.CombineDependencies(setRoomTilesJobHandle, setCorridorTilesJobHandle));

                    var closeBordersJobHandle = new CloseBordersJob
                    {
                        Board = board,
                        Tiles = tiles
                    }.Schedule(removeThinWallsJobHandle);

                    var instantiateTilesJobHandle = new InstantiateTilesJob
                    {
                        CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                        Tiles = tiles,
                        TileStride = board.Size.x
                    }.Schedule(board.Size.y, 1, closeBordersJobHandle);

                    inputDeps = new TagBoardDoneJob
                    {
                        CommandBuffer = this._endFrameBarrier.CreateCommandBuffer(),
                        BoardEntity = boardEntity,
                        Tiles = tiles,
                        BoardComponent = board,
                        RandomSeed = random.NextInt()
                    }.Schedule(instantiateTilesJobHandle);
                    this._endFrameBarrier.AddJobHandleForProducer(inputDeps);
                }
            }
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
