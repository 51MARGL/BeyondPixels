using System.Linq;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.SceneBootstraps;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Spawning.PoissonDiscSampling
{
    public class EnemySpawningSystem : JobComponentSystem
    {
        private const int SystemRequestID = 1;

        private struct EnemiesSpawnStartedComponent : IComponentData { }
        private struct InitializeValidationGridJob : IJobProcessComponentDataWithEntity<FinalBoardComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<FinalTileComponent> Tiles;
            public float2 PlayerPosition;

            public void Execute(Entity boardEntity, int index, ref FinalBoardComponent finalBoardComponent)
            {
                var poissonDiscEntity = this.CommandBuffer.CreateEntity(index);
                var boardSize = finalBoardComponent.Size;
                this.CommandBuffer.AddComponent(index, poissonDiscEntity, new PoissonDiscSamplingComponent
                {
                    GridSize = boardSize,
                    SamplesLimit = 30,
                    RequestID = SystemRequestID,
                    RadiusFromArray = 1
                });
                for (var i = 0; i < PrefabManager.Instance.EnemyPrefabs.Length; i++)
                {
                    var entity = this.CommandBuffer.CreateEntity(index);
                    this.CommandBuffer.AddComponent(index, entity, new PoissonRadiusComponent
                    {
                        Radius = PrefabManager.Instance.EnemyPrefabs[i].SpawnRadius,
                        RequestID = SystemRequestID
                    });
                }
                for (var y = 0; y < boardSize.y; y++)
                    for (var x = 0; x < boardSize.x; x++)
                    {
                        var entity = this.CommandBuffer.CreateEntity(index);
                        var validationIndex = this.GetValidationIndex(y * boardSize.x + x, boardSize, 10);
                        this.CommandBuffer.AddComponent(index, entity, new PoissonCellComponent
                        {
                            SampleIndex = validationIndex,
                            Position = this.Tiles[y * boardSize.x + x].Position,
                            RequestID = SystemRequestID
                        });
                    }

                this.CommandBuffer.AddComponent(index, boardEntity, new EnemiesSpawnStartedComponent());
            }

            private int GetValidationIndex(int tileIndex, int2 boardSize, int radius)
            {
                var tile = this.Tiles[tileIndex];
                if (tile.TileType == TileType.Floor
                    && math.distance(this.PlayerPosition, tile.Position) >= radius)
                    return -1;

                return -2;
            }
        }

        private struct TagBoardDoneJob : IJobProcessComponentDataWithEntity<FinalBoardComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref FinalBoardComponent finalBoardComponent)
            {
                this.CommandBuffer.AddComponent(index, entity, new EnemiesSpawnedComponent());
            }
        }

        private struct CleanSamplesJob : IJobProcessComponentDataWithEntity<SampleComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref SampleComponent sampleComponent)
            {
                if (sampleComponent.RequestID == SystemRequestID)
                    this.CommandBuffer.DestroyEntity(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private ComponentGroup _tilesGroup;
        private ComponentGroup _boardSpawnInitGroup;
        private ComponentGroup _boardSpawnReadyGroup;
        private ComponentGroup _samplesGroup;
        private ComponentGroup _loadGameGroup;

        protected override void OnCreateManager()
        {
            this._endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            this._tilesGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalTileComponent)
                }
            });
            this._boardSpawnInitGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent),
                    typeof(PlayerSpawnedComponent)
                },
                None = new ComponentType[]
                {
                    typeof(EnemiesSpawnStartedComponent)
                }
            });
            this._boardSpawnReadyGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent), typeof(EnemiesSpawnStartedComponent)
                },
                None = new ComponentType[]
                {
                    typeof(EnemiesSpawnedComponent)
                }
            });
            this._samplesGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(SampleComponent)
                }
            });
            this._loadGameGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(LoadGameComponent)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (this._boardSpawnInitGroup.CalculateLength() > 0)
                return this.SetupValidationGrid(inputDeps);

            if (this._boardSpawnReadyGroup.CalculateLength() > 0
                && this._loadGameGroup.CalculateLength() == 0
                && PlayerSpawningSystem.PlayerInstantiated)
            {
                if (this._samplesGroup.CalculateLength() > 0)
                {
                    var samplesArray = this._samplesGroup.ToComponentDataArray<SampleComponent>(Allocator.TempJob);
                    var samplesList = new NativeList<SampleComponent>(Allocator.TempJob);
                    for (var i = 0; i < samplesArray.Length; i++)
                        if (samplesArray[i].RequestID == SystemRequestID)
                            samplesList.Add(samplesArray[i]);

                    var tagBoardDoneJobHandle = new TagBoardDoneJob
                    {
                        CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                    }.Schedule(this, inputDeps);
                    var cleanSamplesJobHandle = new CleanSamplesJob
                    {
                        CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                    }.Schedule(this, inputDeps);

                    inputDeps = JobHandle.CombineDependencies(tagBoardDoneJobHandle, cleanSamplesJobHandle);
                    this._endFrameBarrier.AddJobHandleForProducer(inputDeps);

                    inputDeps.Complete();

                    for (var i = 0; i < samplesList.Length; i++)
                        this.InstantiateEnemy(samplesList[i].Position, samplesList[i].Radius, this._endFrameBarrier.CreateCommandBuffer());

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
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                Tiles = this._tilesGroup.ToComponentDataArray<FinalTileComponent>(Allocator.TempJob),
                PlayerPosition = playerPosition
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(initializeValidationGridJobHandle);
            return initializeValidationGridJobHandle;
        }

        private void InstantiateEnemy(int2 position, int radius, EntityCommandBuffer commandBuffer)
        {
            var enemy = Object.Instantiate(PrefabManager.Instance.EnemyPrefabs.FirstOrDefault(e => e.SpawnRadius == radius).Prefab,
                new Vector3(position.x + 0.5f, position.y + 0.5f, 0), Quaternion.identity);

            commandBuffer.AddComponent(enemy.GetComponent<GameObjectEntity>().Entity, new Disabled());
        }
    }
}
