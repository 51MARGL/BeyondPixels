﻿using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.Naive;
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
        [DisableAutoCreation]
        public class BoardSystemBarrier : BarrierSystem { }

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
                var currentRoom = Roms[index];
                for (int j = 0; j < currentRoom.Size.x; j++)
                {
                    int xCoord = currentRoom.X + j;

                    if (currentRoom.X == 0 || currentRoom.Y == 0)
                        return;
                    for (int k = 0; k < currentRoom.Size.y; k++)
                    {
                        int yCoord = currentRoom.Y + k;                        
                        Tiles[(yCoord * TileStride) + xCoord] = TileType.Floor;
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
                var currentCorridor = Corridors[index];

                for (int j = 0; j < currentCorridor.Length; j++)
                {
                    int xCoord = currentCorridor.StartX;
                    int yCoord = currentCorridor.StartY;

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

                    Tiles[(yCoord * TileStride) + xCoord] = TileType.Floor;

                    if (currentCorridor.Direction == Direction.Up || currentCorridor.Direction == Direction.Down)
                        Tiles[(yCoord * TileStride) + xCoord - 1] = TileType.Floor;
                    else
                        Tiles[((yCoord - 1) * TileStride) + xCoord] = TileType.Floor;
                }
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

        private struct BatchData
        {
            public int FirstRoomIndex;
            public int LastRoomIndex;
            public int FirstCorridorIndex;
        }
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

            public Unity.Mathematics.Random Random;

            public void Execute(int index)
            {
                var batch = Data[index];

                Rooms[batch.FirstRoomIndex] =
                    CreateRoom(Board, Corridors[batch.FirstCorridorIndex], ref Random);
                Corridors[3 + batch.FirstRoomIndex] =
                    CreateCorridor(Rooms[batch.FirstRoomIndex], Board, false, ref Random);

                for (int i = batch.FirstRoomIndex + 1, j = 4 + batch.FirstRoomIndex; i < batch.LastRoomIndex; i++, j++)
                {
                    Rooms[i] = CreateRoom(Board, Corridors[j - 1], ref Random);
                    if (j < Corridors.Length)
                        Corridors[j] = CreateCorridor(Rooms[i], Board, false, ref Random);
                }
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

                var roomCount = board.RoomCount;

                var tiles = new NativeArray<TileType>(board.Size.x * board.Size.y, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                var rooms = new NativeArray<RoomComponent>(roomCount, Allocator.TempJob);
                var corridors = new NativeArray<CorridorComponent>(roomCount + 2, Allocator.TempJob);

                for (int j = 0; j < tiles.Length; j++)
                    tiles[j] = TileType.Wall;

                // setup fist room and corridors to all 4 directions
                var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, uint.MaxValue));
                rooms[0] = CreateRoom(board, ref random);
                corridors[0] = CreateCorridor(rooms[0], board, true, ref random, 0);
                corridors[1] = CreateCorridor(rooms[0], board, true, ref random, 1);
                corridors[2] = CreateCorridor(rooms[0], board, true, ref random, 2);
                corridors[3] = CreateCorridor(rooms[0], board, true, ref random, 3);

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
                    Random = random
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

                var finalizeBoardJobHandle = new FinalizeBoardJob
                {
                    Board = board,
                    Tiles = tiles,
                }.Schedule(JobHandle.CombineDependencies(setRoomTilesJobHandle, setCorridorTilesJobHandle));

                var instantiateTilesJobHandle = new InstantiateTilesJob
                {
                    CommandBuffer = _boardSystemBarrier.CreateCommandBuffer().ToConcurrent(),
                    Tiles = tiles,
                    TileStride = board.Size.x
                }.Schedule(board.Size.y, 1, finalizeBoardJobHandle);

                inputDeps = new CleanUpJob
                {
                    CommandBuffer = _boardSystemBarrier.CreateCommandBuffer().ToConcurrent(),
                    Boards = boards,
                    BoardEntities = boardEntities
                }.Schedule(boards.Length, 1, instantiateTilesJobHandle);
            }
            _boardSystemBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }

        private static RoomComponent CreateRoom(BoardComponent board, ref Unity.Mathematics.Random random)
        {
            var roomWidth = random.NextInt(3, board.RoomSize);
            var roomHeight = random.NextInt(3, board.RoomSize);

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

        private static RoomComponent CreateRoom(BoardComponent board, CorridorComponent corridor, ref Unity.Mathematics.Random random)
        {
            var roomWidth = random.NextInt(3, board.RoomSize);
            var roomHeight = random.NextInt(3, board.RoomSize);

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

        private static CorridorComponent CreateCorridor(RoomComponent room, BoardComponent board, bool first, ref Unity.Mathematics.Random random, int forceDirection = -1)
        {
            var direction = (Direction)random.NextInt(0, 4);
            var oppositeDirection = (Direction)(((int)room.EnteringCorridor + 2) % 4);

            if (first && forceDirection != -1)
                direction = (Direction)forceDirection;
            else if (!first && random.NextInt(0, 100) > 50) //50% chance to go further from center
            {
                var centerX = (int)math.round(board.Size.x / 2f);
                var centerY = (int)math.round(board.Size.y / 2f);
                if (room.X > centerX && room.Y > centerY)
                    direction = random.NextInt(0, 10) > 4 ? Direction.Left : Direction.Up;
                else if (room.X < centerX && room.Y > centerY)
                    direction = random.NextInt(0, 10) > 4 ? Direction.Right : Direction.Up;
                else if (room.X > centerX && room.Y < centerY)
                    direction = random.NextInt(0, 10) > 4 ? Direction.Left : Direction.Down;
                else
                    direction = random.NextInt(0, 10) > 4 ? Direction.Right : Direction.Down;
            }

            if (!first && direction == oppositeDirection)
            {
                int directionInt = (int)direction;
                directionInt++;
                directionInt = directionInt % 4;
                direction = (Direction)directionInt;

            }

            var corridorLength = random.NextInt(board.MinCorridorLength, board.MaxCorridorLength);
            var corridorX = 0;
            var corridorY = 0;

            int maxLength = board.MaxCorridorLength;

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
