using System.Linq;
using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;
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
using UnityEngine.AI;

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
                        int validationIndex = GetValidationIndex(y * boardSize.x + x, boardSize, 10);
                        CommandBuffer.AddComponent(index, entity, new PoissonCellComponent
                        {
                            SampleIndex = validationIndex,
                            Position = Tiles[y * boardSize.x + x].Position,
                            RequestID = SystemRequestID
                        });
                    }

                CommandBuffer.AddComponent(index, boardEntity, new EnemiesSpawnStartedComponent());
            }

            private int GetValidationIndex(int tileIndex, int2 boardSize, int radius)
            {
                var tile = Tiles[tileIndex];
                if (tile.TileType == TileType.Floor
                    && math.distance(PlayerPosition, tile.Position) >= radius)
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
        private ComponentGroup _loadGameGroup;

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
            _loadGameGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(LoadGameComponent)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_boardSpawnInitGroup.CalculateLength() > 0)
                return SetupValidationGrid(inputDeps);

            if (_boardSpawnReadyGroup.CalculateLength() > 0
                && _loadGameGroup.CalculateLength() == 0
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

                    var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());
                    var playerEntity = GameObject.FindGameObjectWithTag("Player").GetComponent<GameObjectEntity>().Entity;
                    var playerLvlComponent = EntityManager.GetComponentData<LevelComponent>(playerEntity);
                    for (int i = 0; i < samplesList.Length; i++)
                        InstantiateEnemy(samplesList[i].Position, samplesList[i].Radius, playerLvlComponent, ref random);

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

        private void InstantiateEnemy(int2 position, int radius, LevelComponent playerLvlComponent, ref Unity.Mathematics.Random random)
        {
            var commandBuffer = _endFrameBarrier.CreateCommandBuffer();
            var enemy = Object.Instantiate(PrefabManager.Instance.EnemyPrefabs.FirstOrDefault(e => e.SpawnRadius == radius).Prefab,
                new Vector3(position.x + 0.5f, position.y + 0.5f, 0), Quaternion.identity);
            var enemyEntity = enemy.GetComponent<GameObjectEntity>().Entity;
            var enemyInitializeComponent = enemy.GetComponent<EnemyInitializeComponent>();
            var navMeshAgent = enemy.GetComponent<NavMeshAgent>();
            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;

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

            #region statsInit
            var statsInitializeComponent = enemy.GetComponent<StatsInitializeComponent>();
            var lvlComponent = statsInitializeComponent.LevelComponent;
            lvlComponent.CurrentLevel =
                math.max(0, random.NextInt(playerLvlComponent.CurrentLevel,
                                           playerLvlComponent.CurrentLevel + 3));

            var healthStatComponent = statsInitializeComponent.HealthStatComponent;
            var attackStatComponent = statsInitializeComponent.AttackStatComponent;
            var defenceStatComponent = statsInitializeComponent.DefenceStatComponent;
            var magicStatComponent = statsInitializeComponent.MagicStatComponent;

            InitializeRandomStats(lvlComponent.CurrentLevel, ref random, ref healthStatComponent,
                                ref attackStatComponent, ref defenceStatComponent, ref magicStatComponent);

            commandBuffer.AddComponent(enemyEntity, lvlComponent);
            commandBuffer.AddComponent(enemyEntity, healthStatComponent);
            commandBuffer.AddComponent(enemyEntity, new HealthComponent
            {
                MaxValue = healthStatComponent.CurrentValue,
                CurrentValue = healthStatComponent.CurrentValue
            });
            commandBuffer.AddComponent(enemyEntity, attackStatComponent);
            commandBuffer.AddComponent(enemyEntity, defenceStatComponent);
            commandBuffer.AddComponent(enemyEntity, magicStatComponent);
            commandBuffer.AddComponent(enemyEntity, statsInitializeComponent.XPRewardComponent);
            commandBuffer.AddComponent(enemyEntity, new AdjustStatsComponent());
            Object.Destroy(statsInitializeComponent);
            #endregion

            commandBuffer.RemoveComponent<EnemyInitializeComponent>(enemyEntity);
        }
        private void InitializeRandomStats(int currentLevel, ref Unity.Mathematics.Random random,
                                           ref HealthStatComponent healthStatComponent,
                                           ref AttackStatComponent attackStatComponent,
                                           ref DefenceStatComponent defenceStatComponent,
                                           ref MagicStatComponent magicStatComponent)
        {
            for (int i = 1; i < currentLevel; i++)
            {
                var randomStat = random.NextInt(0, 100);
                switch (randomStat)
                {
                    case var _ when randomStat < 25:
                        healthStatComponent.PointsSpent++;
                        break;
                    case var _ when randomStat < 50:
                        attackStatComponent.PointsSpent++;
                        break;
                    case var _ when randomStat < 75:
                        defenceStatComponent.PointsSpent++;
                        break;
                    case var _ when randomStat < 100:
                        magicStatComponent.PointsSpent++;
                        break;
                }
            }
        }
    }
}
