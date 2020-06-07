using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.ECS.Systems.Items;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.AI;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class EnemyInitializeSystem : ComponentSystem
    {
        private EntityQuery _group;

        protected override void OnCreate()
        {
            this._group = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(EnemyInitializeComponent), typeof(Transform),
                    typeof(StatsInitializeComponent), typeof(NavMeshAgent),
                },
                None = new ComponentType[]
{
                    typeof(PositionComponent)
}
            });
        }

        protected override void OnUpdate()
        {
            var random = new Unity.Mathematics.Random((uint)System.Guid.NewGuid().GetHashCode());
            var playerEntity = GameObject.FindGameObjectWithTag("Player").GetComponent<GameObjectEntity>().Entity;

            if (!this.EntityManager.HasComponent<LevelComponent>(playerEntity))
            {
                return;
            }

            var playerLvlComponent = this.EntityManager.GetComponentData<LevelComponent>(playerEntity);

            this.Entities.With(this._group).ForEach((Entity entity,
                EnemyInitializeComponent enemyInitializeComponent,
                StatsInitializeComponent statsInitializeComponent,
                NavMeshAgent navMeshAgent,
                Transform transform) =>
            {
                navMeshAgent.enabled = true;
                navMeshAgent.updatePosition = false;
                navMeshAgent.updateRotation = false;
                navMeshAgent.updateUpAxis = false;

                this.PostUpdateCommands.AddComponent(entity, new CharacterComponent
                {
                    CharacterType = CharacterType.Enemy
                });
                this.PostUpdateCommands.AddComponent(entity, new HealthComponent
                {
                    MaxValue = enemyInitializeComponent.BaseHealth,
                    CurrentValue = enemyInitializeComponent.BaseHealth,
                    BaseValue = enemyInitializeComponent.BaseHealth
                });
                this.PostUpdateCommands.AddComponent(entity, new MovementComponent
                {
                    Direction = float2.zero,
                    Speed = enemyInitializeComponent.MovementSpeed
                });

                this.PostUpdateCommands.AddComponent(entity, new WeaponComponent
                {
                    DamageValue = enemyInitializeComponent.WeaponDamage,
                    MeleeAttackRange = enemyInitializeComponent.MeleeAttackRange,
                    CoolDown = enemyInitializeComponent.AttackCoolDown,
                    SpellAttackRange = enemyInitializeComponent.SpellAttackRange,
                    SpellCheckFrequency = enemyInitializeComponent.SpellCheckFrequency,
                    SpellCastChance = enemyInitializeComponent.SpellCastChance
                });
                this.PostUpdateCommands.AddComponent(entity, new IdleStateComponent
                {
                    StartedAt = Time.time
                });
                this.PostUpdateCommands.AddComponent(entity, new PositionComponent
                {
                    InitialPosition = new float2(transform.position.x, transform.position.y)
                });
                Object.Destroy(enemyInitializeComponent);

                var lvlComponent = this.InitializeStats(entity, statsInitializeComponent, ref random, playerLvlComponent);

                this.InitializeRandomItems(entity, ref random, lvlComponent);

                #region spellInit
                var spellEntity = this.PostUpdateCommands.CreateEntity();
                this.PostUpdateCommands.AddComponent(spellEntity, new ActiveSpellComponent
                {
                    Owner = entity,
                    ActionIndex = 1,
                    SpellIndex = 0
                });
                #endregion
            });
        }

        private LevelComponent InitializeStats(Entity entity, StatsInitializeComponent statsInitializeComponent, ref Unity.Mathematics.Random random, LevelComponent playerLvlComponent)
        {
            var lvlComponent = statsInitializeComponent.LevelComponent;
            lvlComponent.CurrentLevel = playerLvlComponent.CurrentLevel == 1 ? 1 :
                                            random.NextInt(playerLvlComponent.CurrentLevel,
                                                           playerLvlComponent.CurrentLevel + 3);

            var healthStatComponent = statsInitializeComponent.HealthStatComponent;
            var attackStatComponent = statsInitializeComponent.AttackStatComponent;
            var defenceStatComponent = statsInitializeComponent.DefenceStatComponent;
            var magicStatComponent = statsInitializeComponent.MagicStatComponent;

            this.InitializeRandomStats(lvlComponent.CurrentLevel, ref random, ref healthStatComponent,
                                ref attackStatComponent, ref defenceStatComponent, ref magicStatComponent);

            this.PostUpdateCommands.AddComponent(entity, lvlComponent);
            this.PostUpdateCommands.AddComponent(entity, healthStatComponent);
            this.PostUpdateCommands.AddComponent(entity, attackStatComponent);
            this.PostUpdateCommands.AddComponent(entity, defenceStatComponent);
            this.PostUpdateCommands.AddComponent(entity, magicStatComponent);
            this.PostUpdateCommands.AddComponent(entity, statsInitializeComponent.XPRewardComponent);
            this.PostUpdateCommands.AddComponent(entity, new AdjustStatsComponent());
            Object.Destroy(statsInitializeComponent);
            return lvlComponent;
        }

        private void InitializeRandomItems(Entity entity, ref Unity.Mathematics.Random random, LevelComponent lvlComponent)
        {
            if (random.NextInt(0, 100) > 25)
            {
                var weaponEntity = ItemFactory.GetRandomWeapon(lvlComponent.CurrentLevel, ref random, this.PostUpdateCommands);
                this.PostUpdateCommands.AddComponent(weaponEntity, new PickedUpComponent
                {
                    Owner = entity
                });
                this.PostUpdateCommands.AddComponent(weaponEntity, new EquipedComponent());
            }
            if (random.NextInt(0, 100) > 25)
            {
                var spellBookEntity = ItemFactory.GetRandomMagicWeapon(lvlComponent.CurrentLevel, ref random, this.PostUpdateCommands);
                this.PostUpdateCommands.AddComponent(spellBookEntity, new PickedUpComponent
                {
                    Owner = entity
                });
                this.PostUpdateCommands.AddComponent(spellBookEntity, new EquipedComponent());
            }
            if (random.NextInt(0, 100) > 25)
            {
                var helmetEntity = ItemFactory.GetRandomHelmet(lvlComponent.CurrentLevel, ref random, this.PostUpdateCommands);
                this.PostUpdateCommands.AddComponent(helmetEntity, new PickedUpComponent
                {
                    Owner = entity
                });
                this.PostUpdateCommands.AddComponent(helmetEntity, new EquipedComponent());
            }
            if (random.NextInt(0, 100) > 25)
            {
                var chestEntity = ItemFactory.GetRandomChest(lvlComponent.CurrentLevel, ref random, this.PostUpdateCommands);
                this.PostUpdateCommands.AddComponent(chestEntity, new PickedUpComponent
                {
                    Owner = entity
                });
                this.PostUpdateCommands.AddComponent(chestEntity, new EquipedComponent());
            }
            if (random.NextInt(0, 100) > 25)
            {
                var bootsEntity = ItemFactory.GetRandomBoots(lvlComponent.CurrentLevel, ref random, this.PostUpdateCommands);
                this.PostUpdateCommands.AddComponent(bootsEntity, new PickedUpComponent
                {
                    Owner = entity
                });
                this.PostUpdateCommands.AddComponent(bootsEntity, new EquipedComponent());
            }
            if (random.NextInt(0, 100) > 75)
            {
                var randomCount = random.NextInt(1, 3);
                for (var i = 0; i < randomCount; i++)
                {
                    var foodEntity = ItemFactory.GetRandomFood(ref random, this.PostUpdateCommands);
                    this.PostUpdateCommands.AddComponent(foodEntity, new PickedUpComponent
                    {
                        Owner = entity
                    });
                }
            }
            if (random.NextInt(0, 100) > 75)
            {
                var randomCount = random.NextInt(1, 3);
                for (var i = 0; i < randomCount; i++)
                {
                    var potionEntity = ItemFactory.GetHealthPotion(ref random, this.PostUpdateCommands);
                    this.PostUpdateCommands.AddComponent(potionEntity, new PickedUpComponent
                    {
                        Owner = entity
                    });
                }
            }
            this.PostUpdateCommands.AddComponent(entity, new ApplyInitialHealthModifierComponent());
        }

        private void InitializeRandomStats(int currentLevel, ref Unity.Mathematics.Random random,
                                           ref HealthStatComponent healthStatComponent,
                                           ref AttackStatComponent attackStatComponent,
                                           ref DefenceStatComponent defenceStatComponent,
                                           ref MagicStatComponent magicStatComponent)
        {
            var points = random.NextInt(currentLevel, currentLevel * 2);
            for (var i = 1; i < points; i++)
            {
                var randomStat = random.NextInt(0, 100);
                if (randomStat < 25)
                {
                    healthStatComponent.PointsSpent++;
                }
                else if (randomStat < 50)
                {
                    attackStatComponent.PointsSpent++;
                }
                else if (randomStat < 75)
                {
                    defenceStatComponent.PointsSpent++;
                }
                else if (randomStat < 100)
                {
                    magicStatComponent.PointsSpent++;
                }
            }
        }
    }
}
