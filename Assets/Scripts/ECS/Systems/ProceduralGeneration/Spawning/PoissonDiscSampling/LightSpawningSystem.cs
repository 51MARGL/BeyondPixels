using BeyondPixels.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Spawning.PoissonDiscSampling
{
    public class LightSpawningSystem : JobComponentSystem
    {
        private const int SystemRequestID = 2;
        private struct LightSpawnStartedComponent : IComponentData { }

        private struct InitializeValidationGridJob : IJobProcessComponentDataWithEntity<FinalBoardComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<FinalTileComponent> Tiles;

            public void Execute(Entity boardEntity, int index, ref FinalBoardComponent finalBoardComponent)
            {
                var poissonDiscEntity = CommandBuffer.CreateEntity(index);
                var boardSize = finalBoardComponent.Size;
                CommandBuffer.AddComponent(index, poissonDiscEntity, new PoissonDiscSamplingComponent
                {
                    GridSize = boardSize,
                    SamplesLimit = 30,
                    Radius = 10,
                    RequestID = SystemRequestID
                });
                for (int y = 0; y < boardSize.y; y++)
                    for (int x = 0; x < boardSize.x; x++)
                    {
                        var entity = CommandBuffer.CreateEntity(index);
                        int validationIndex = GetValidationIndex(y * boardSize.x + x, boardSize);
                        CommandBuffer.AddComponent(index, entity, new PoissonCellComponent
                        {
                            SampleIndex = validationIndex,
                            Position = Tiles[y * boardSize.x + x].Position,
                            RequestID = SystemRequestID
                        });
                    }
                CommandBuffer.AddComponent(index, boardEntity, new LightSpawnStartedComponent());
            }

            private int GetValidationIndex(int tileIndex, int2 boardSize)
            {
                var tile = Tiles[tileIndex];
                if (tile.TileType == TileType.Wall
                    && tile.Position.x > 1 && tile.Position.x < boardSize.x - 1
                    && tile.Position.y > 1 && tile.Position.y < boardSize.y - 1
                    && Tiles[(tile.Position.y - 1) * boardSize.x + tile.Position.x].TileType == TileType.Floor
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
                CommandBuffer.AddComponent(index, entity, new LightsSpawnedComponent());
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
        private ComponentGroup _tileMapGroup;

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
                    typeof(FinalBoardComponent)
                },
                None = new ComponentType[]
                {
                    typeof(LightSpawnStartedComponent)
                }
            });
            _boardSpawnReadyGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent), typeof(LightSpawnStartedComponent)
                },
                None = new ComponentType[]
                {
                    typeof(LightsSpawnedComponent)
                }
            });
            _samplesGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(SampleComponent)
                }
            });
            _tileMapGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(DungeonTileMapComponent),
                    typeof(Transform)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_boardSpawnInitGroup.CalculateLength() > 0)
                return SetupValidationGrid(inputDeps);

            if (_boardSpawnReadyGroup.CalculateLength() > 0)
            {
                if (!TileMapSystem.TileMapDrawing && _samplesGroup.CalculateLength() > 0)
                {
                    var tileMapComponent = _tileMapGroup.ToComponentArray<DungeonTileMapComponent>()[0];
                    var tileMapTransform = _tileMapGroup.ToComponentArray<Transform>()[0];

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
                        InstantiateLights(samplesList[i].Position,
                                        tileMapComponent,
                                        tileMapTransform);

                    samplesArray.Dispose();
                    samplesList.Dispose();
                }
            }
            return inputDeps;
        }

        private JobHandle SetupValidationGrid(JobHandle inputDeps)
        {
            var initializeValidationGridJobHandle = new InitializeValidationGridJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                Tiles = _tilesGroup.ToComponentDataArray<FinalTileComponent>(Allocator.TempJob),
            }.Schedule(this, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(initializeValidationGridJobHandle);
            return initializeValidationGridJobHandle;
        }

        private void InstantiateLights(int2 position, DungeonTileMapComponent tilemapComponent, Transform lightParent)
        {
            tilemapComponent.WallTorchAnimatedTile.m_AnimationStartTime = UnityEngine.Random.Range(1, 10);
            tilemapComponent.TilemapWallsAnimated.SetTile(new Vector3Int(position.x, position.y, 0), tilemapComponent.WallTorchAnimatedTile);
            Object.Instantiate(tilemapComponent.TorchLight,
                new Vector3(position.x + 0.5f, position.y - 0.5f, -1),
                Quaternion.identity, lightParent);
        }
    }
}
