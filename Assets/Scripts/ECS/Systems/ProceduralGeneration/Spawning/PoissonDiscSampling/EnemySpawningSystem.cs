using System.Linq;

using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.ECS.Systems.Items;
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

                    var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());
                    var playerEntity = GameObject.FindGameObjectWithTag("Player").GetComponent<GameObjectEntity>().Entity;
                    var playerLvlComponent = this.EntityManager.GetComponentData<LevelComponent>(playerEntity);
                    for (var i = 0; i < samplesList.Length; i++)
                        this.InstantiateEnemy(samplesList[i].Position, samplesList[i].Radius, playerLvlComponent, ref random);

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

        private void InstantiateEnemy(int2 position, int radius, LevelComponent playerLvlComponent, ref Unity.Mathematics.Random random)
        {
            var commandBuffer = this._endFrameBarrier.CreateCommandBuffer();
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
            commandBuffer.AddComponent(enemyEntity, new HealthComponent
            {
                MaxValue = enemyInitializeComponent.BaseHealth,
                CurrentValue = enemyInitializeComponent.BaseHealth,
                BaseValue = enemyInitializeComponent.BaseHealth
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

            this.InitializeRandomStats(lvlComponent.CurrentLevel, ref random, ref healthStatComponent,
                                ref attackStatComponent, ref defenceStatComponent, ref magicStatComponent);

            commandBuffer.AddComponent(enemyEntity, lvlComponent);
            commandBuffer.AddComponent(enemyEntity, healthStatComponent);
            commandBuffer.AddComponent(enemyEntity, attackStatComponent);
            commandBuffer.AddComponent(enemyEntity, defenceStatComponent);
            commandBuffer.AddComponent(enemyEntity, magicStatComponent);
            commandBuffer.AddComponent(enemyEntity, statsInitializeComponent.XPRewardComponent);
            commandBuffer.AddComponent(enemyEntity, new AdjustStatsComponent());
            Object.Destroy(statsInitializeComponent);
            #endregion

            #region items
            if (random.NextInt(0, 100) > 50)
            {
                var weaponEntity = ItemFactory.GetRandomWeapon(lvlComponent.CurrentLevel, ref random);
                commandBuffer.AddComponent(weaponEntity, new PickedUpComponent
                {
                    Owner = enemyEntity
                });
                commandBuffer.AddComponent(weaponEntity, new EquipedComponent());
            }
            if (random.NextInt(0, 100) > 50)
            {
                var spellBookEntity = ItemFactory.GetRandomMagicWeapon(lvlComponent.CurrentLevel, ref random);
                commandBuffer.AddComponent(spellBookEntity, new PickedUpComponent
                {
                    Owner = enemyEntity
                });
                commandBuffer.AddComponent(spellBookEntity, new EquipedComponent());
            }
            if (random.NextInt(0, 100) > 50)
            {
                var helmetEntity = ItemFactory.GetRandomHelmet(lvlComponent.CurrentLevel, ref random);
                commandBuffer.AddComponent(helmetEntity, new PickedUpComponent
                {
                    Owner = enemyEntity
                });
                commandBuffer.AddComponent(helmetEntity, new EquipedComponent());
            }
            if (random.NextInt(0, 100) > 50)
            {
                var chestEntity = ItemFactory.GetRandomChest(lvlComponent.CurrentLevel, ref random);
                commandBuffer.AddComponent(chestEntity, new PickedUpComponent
                {
                    Owner = enemyEntity
                });
                commandBuffer.AddComponent(chestEntity, new EquipedComponent());
            }
            if (random.NextInt(0, 100) > 50)
            {
                var bootsEntity = ItemFactory.GetRandomBoots(lvlComponent.CurrentLevel, ref random);
                commandBuffer.AddComponent(bootsEntity, new PickedUpComponent
                {
                    Owner = enemyEntity
                });
                commandBuffer.AddComponent(bootsEntity, new EquipedComponent());
            }
            if (random.NextInt(0, 100) > 75)
            {
                var randomCount = random.NextInt(1, 3);
                for (int i = 0; i < randomCount; i++)
                {
                    var foodEntity = ItemFactory.GetRandomFood(ref random);
                    commandBuffer.AddComponent(foodEntity, new PickedUpComponent
                    {
                        Owner = enemyEntity
                    });
                }
            }
            if (random.NextInt(0, 100) > 75)
            {
                var randomCount = random.NextInt(1, 3);
                for (int i = 0; i < randomCount; i++)
                {
                    var potionEntity = ItemFactory.GetHealthPotion(ref random);
                    commandBuffer.AddComponent(potionEntity, new PickedUpComponent
                    {
                        Owner = enemyEntity
                    });
                }
            }
            commandBuffer.AddComponent(enemyEntity, new ApplyInitialHealthModifierComponent());
            #endregion

            commandBuffer.RemoveComponent<EnemyInitializeComponent>(enemyEntity);
        }
        private void InitializeRandomStats(int currentLevel, ref Unity.Mathematics.Random random,
                                           ref HealthStatComponent healthStatComponent,
                                           ref AttackStatComponent attackStatComponent,
                                           ref DefenceStatComponent defenceStatComponent,
                                           ref MagicStatComponent magicStatComponent)
        {
            for (var i = 1; i < currentLevel; i++)
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
