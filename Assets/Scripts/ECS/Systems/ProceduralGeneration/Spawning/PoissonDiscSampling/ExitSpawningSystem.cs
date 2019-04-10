using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.SceneBootstraps;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Spawning.PoissonDiscSampling
{
    public class ExitSpawningSystem : JobComponentSystem
    {
        private const int SystemRequestID = 3;

        private struct ExitSpawnStartedComponent : IComponentData { }
        private struct InitializeValidationGridJob : IJobProcessComponentDataWithEntity<FinalBoardComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<FinalTileComponent> Tiles;
            public float2 PlayerPosition;

            public void Execute(Entity boardEntity, int index, ref FinalBoardComponent finalBoardComponent)
            {
                var poissonDiscEntity = CommandBuffer.CreateEntity(index);
                var boardSize = finalBoardComponent.Size;
                var radius = 30;
                CommandBuffer.AddComponent(index, poissonDiscEntity, new PoissonDiscSamplingComponent
                {
                    GridSize = boardSize,
                    SamplesLimit = 50,
                    RequestID = SystemRequestID,
                    Radius = radius
                });

                var cellList = GetCells(boardSize, radius);
                while (cellList.Length == 0)
                {
                    radius -= 5;
                    cellList = GetCells(boardSize, radius);
                }

                for (int i = 0; i < cellList.Length; i++)
                {
                    var entity = CommandBuffer.CreateEntity(index);
                    CommandBuffer.AddComponent(index, entity, cellList[i]);
                }
                CommandBuffer.AddComponent(index, boardEntity, new ExitSpawnStartedComponent());
            }

            private NativeList<PoissonCellComponent> GetCells(int2 boardSize, int radius)
            {
                var cellList = new NativeList<PoissonCellComponent>(Allocator.Temp);
                for (int y = 0; y < boardSize.y; y++)
                    for (int x = 0; x < boardSize.x; x++)
                    {
                        int validationIndex = GetValidationIndex(y * boardSize.x + x, boardSize, radius);
                        cellList.Add(new PoissonCellComponent
                        {
                            SampleIndex = validationIndex,
                            Position = Tiles[y * boardSize.x + x].Position,
                            RequestID = SystemRequestID
                        });
                    }

                return cellList;
            }

            private int GetValidationIndex(int tileIndex, int2 boardSize, int radius)
            {
                var tile = Tiles[tileIndex];
                if (tile.TileType == TileType.Wall
                    && math.distance(PlayerPosition, tile.Position) >= radius
                    && tile.Position.x > 2 && tile.Position.x < boardSize.x - 2
                    && tile.Position.y > 2 && tile.Position.y < boardSize.y - 2
                    && Tiles[(tile.Position.y - 1) * boardSize.x + tile.Position.x].TileType == TileType.Floor
                    && Tiles[(tile.Position.y - 1) * boardSize.x + tile.Position.x + 1].TileType == TileType.Floor
                    && Tiles[(tile.Position.y - 1) * boardSize.x + tile.Position.x - 1].TileType == TileType.Floor
                    && Tiles[(tile.Position.y + 1) * boardSize.x + tile.Position.x].TileType == TileType.Wall
                    && Tiles[(tile.Position.y) * boardSize.x + tile.Position.x + 1].TileType == TileType.Wall
                    && Tiles[(tile.Position.y) * boardSize.x + tile.Position.x - 1].TileType == TileType.Wall)
                    return -1;

                return -2;
            }
        }

        private struct TagBoardDoneJob : IJobProcessComponentDataWithEntity<FinalBoardComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref FinalBoardComponent finalBoardComponent)
            {
                CommandBuffer.AddComponent(index, entity, new ExitSpawnedComponent());
            }
        }

        private struct CleanSamplesJob : IJobProcessComponentDataWithEntity<SampleComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref SampleComponent sampleComponent)
            {
                if (sampleComponent.RequestID == SystemRequestID)
                    CommandBuffer.DestroyEntity(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private ComponentGroup _tilesGroup;
        private ComponentGroup _boardSpawnInitGroup;
        private ComponentGroup _boardSpawnReadyGroup;
        private ComponentGroup _samplesGroup;

        protected override void OnCreateManager()
        {
            _endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            _tilesGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalTileComponent)
                }
            });
            _boardSpawnInitGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent),
                    typeof(PlayerSpawnedComponent)
                },
                None = new ComponentType[]
                {
                    typeof(ExitSpawnStartedComponent)
                }
            });
            _boardSpawnReadyGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent), typeof(ExitSpawnStartedComponent)
                },
                None = new ComponentType[]
                {
                    typeof(ExitSpawnedComponent)
                }
            });
            _samplesGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(SampleComponent)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_boardSpawnInitGroup.CalculateLength() > 0)
                return SetupValidationGrid(inputDeps);

            if (_boardSpawnReadyGroup.CalculateLength() > 0
                && PlayerSpawningSystem.PlayerInstantiated)
            {
                if (_samplesGroup.CalculateLength() > 0)
                {
                    var samplesArray = _samplesGroup.ToComponentDataArray<SampleComponent>(Allocator.TempJob);
                    var samplesList = new NativeList<SampleComponent>(Allocator.TempJob);
                    for (int i = 0; i < samplesArray.Length; i++)
                        if (samplesArray[i].RequestID == SystemRequestID)
                            samplesList.Add(samplesArray[i]);

                    var tagBoardDoneJobHandle = new TagBoardDoneJob
                    {
                        CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                    }.Schedule(this, inputDeps);
                    var cleanSamplesJobHandle = new CleanSamplesJob
                    {
                        CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                    }.Schedule(this, inputDeps);

                    inputDeps = JobHandle.CombineDependencies(tagBoardDoneJobHandle, cleanSamplesJobHandle);
                    _endFrameBarrier.AddJobHandleForProducer(inputDeps);

                    inputDeps.Complete();
                    for (int i = 0; i < samplesList.Length; i++)
                        InstantiateExit(samplesList[i].Position);

                    samplesArray.Dispose();
                    samplesList.Dispose();
                }
            }
            return inputDeps;
        }

        private JobHandle SetupValidationGrid(JobHandle inputDeps)
        {
            var playerPos = PlayerSpawningSystem.PlayerPosition;
            var playerPosition = new float2(playerPos.x, playerPos.y);
            var initializeValidationGridJobHandle = new InitializeValidationGridJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                Tiles = _tilesGroup.ToComponentDataArray<FinalTileComponent>(Allocator.TempJob),
                PlayerPosition = playerPosition
            }.Schedule(this, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(initializeValidationGridJobHandle);
            return initializeValidationGridJobHandle;
        }

        private void InstantiateExit(int2 position)
        {
            var exit = Object.Instantiate(PrefabManager.Instance.DungeonLevelExit,
                new Vector3(position.x + 0.5f, position.y + 0.75f, 0), Quaternion.identity);
        }
    }
}
