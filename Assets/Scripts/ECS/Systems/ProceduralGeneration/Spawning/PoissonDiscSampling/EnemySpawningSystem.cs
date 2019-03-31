using System.Linq;
using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon;
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

            public void Execute(Entity boardEntity, int index, ref FinalBoardComponent finalBoardComponent)
            {
                var poissonDiscEntity = CommandBuffer.CreateEntity(index);
                var boardSize = finalBoardComponent.Size;
                CommandBuffer.AddComponent(index, poissonDiscEntity, new PoissonDiscSamplingComponent
                {
                    GridSize = boardSize,
                    SamplesLimit = 30,
                    RequestID = SystemRequestID,
                    RadiusFromArray = 1
                });
                for (int i = 0; i < PrefabManager.Instance.EnemyPrefabs.Length; i++)
                {
                    var entity = CommandBuffer.CreateEntity(index);
                    CommandBuffer.AddComponent(index, entity, new PoissonRadiusComponent
                    {
                        Radius = PrefabManager.Instance.EnemyPrefabs[i].SpawnRadius,
                        RequestID = SystemRequestID
                    });
                }
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

                CommandBuffer.AddComponent(index, boardEntity, new EnemiesSpawnStartedComponent());
            }

            private int GetValidationIndex(int tileIndex, int2 boardSize)
            {
                var tile = Tiles[tileIndex];
                if (tile.TileType == TileType.Floor
                    && Tiles[(tile.Position.y + 1) * boardSize.x + tile.Position.x].TileType == TileType.Floor)
                    return -1;

                return -2;
            }
        }

        private struct TagBoardDoneJob : IJobProcessComponentDataWithEntity<FinalBoardComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref FinalBoardComponent finalBoardComponent)
            {
                CommandBuffer.AddComponent(index, entity, new EnemiesSpawnedComponent());
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
                    typeof(FinalBoardComponent)
                },
                None = new ComponentType[]
                {
                    typeof(EnemiesSpawnStartedComponent)
                }
            });
            _boardSpawnReadyGroup = GetComponentGroup(new EntityArchetypeQuery
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

            if (_boardSpawnReadyGroup.CalculateLength() > 0)
            {
                if (!TileMapSystem.TileMapDrawing && _samplesGroup.CalculateLength() > 0)
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
                        InstantiateEnemy(samplesList[i].Position, samplesList[i].Radius);

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

        private void InstantiateEnemy(int2 position, int radius)
        {
            var commandBuffer = _endFrameBarrier.CreateCommandBuffer();
            var enemy = Object.Instantiate(PrefabManager.Instance.EnemyPrefabs.FirstOrDefault(e => e.SpawnRadius == radius).Prefab,
                new Vector3(position.x + 0.5f, position.y + 0.5f, 0), Quaternion.identity);
            var enemyEntity = enemy.GetComponent<GameObjectEntity>().Entity;
            var enemyInitializeComponent = enemy.GetComponent<EnemyInitializeComponent>();

            commandBuffer.AddComponent(enemyEntity, new Disabled());
            commandBuffer.AddComponent(enemyEntity, new CharacterComponent
            {
                CharacterType = CharacterType.Enemy
            });
            commandBuffer.AddComponent(enemyEntity, new MovementComponent
            {
                Direction = float2.zero,
                Speed = enemyInitializeComponent.MovementSpeed
            });
            commandBuffer.AddComponent(enemyEntity, new HealthComponent
            {
                MaxValue = enemyInitializeComponent.MaxHealth,
                CurrentValue = enemyInitializeComponent.MaxHealth
            });
            commandBuffer.AddComponent(enemyEntity, new WeaponComponent
            {
                DamageValue = enemyInitializeComponent.WeaponDamage,
                AttackRange = enemyInitializeComponent.AttackRange,
                CoolDown = enemyInitializeComponent.AttackCoolDown
            });
            commandBuffer.AddComponent(enemyEntity, new IdleStateComponent
            {
                StartedAt = Time.time
            });
            commandBuffer.AddComponent(enemyEntity, new PositionComponent
            {
                InitialPosition = new float2(enemy.transform.position.x, enemy.transform.position.y)
            });
            Object.Destroy(enemyInitializeComponent);
            commandBuffer.RemoveComponent<EnemyInitializeComponent>(enemyEntity);
        }
    }
}
