using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class AllyInitializeSystem : ComponentSystem
    {
        private EntityQuery _group;

        protected override void OnCreateManager()
        {
            this._group = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(AllyInitializeComponent), typeof(Transform), typeof(NavMeshAgent),
                },
                None = new ComponentType[]
                {
                    typeof(PositionComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());
            var playerEntity = GameObject.FindGameObjectWithTag("Player").GetComponent<GameObjectEntity>().Entity;

            if (!this.EntityManager.HasComponent<LevelComponent>(playerEntity))
                return;

            var playerLvlComponent = this.EntityManager.GetComponentData<LevelComponent>(playerEntity);

            this.Entities.With(this._group).ForEach((Entity entity,
                AllyInitializeComponent allyInitializeComponent,
                NavMeshAgent navMeshAgent,
                Transform transform) =>
            {
                navMeshAgent.enabled = true;
                navMeshAgent.updatePosition = false;
                navMeshAgent.updateRotation = false;
                navMeshAgent.updateUpAxis = false;

                #region statsInit
                var lvlComponent = new LevelComponent
                {
                    CurrentLevel = math.max(0, random.NextInt(playerLvlComponent.CurrentLevel,
                                               playerLvlComponent.CurrentLevel + 3)),
                    NextLevelXP = 100,
                    SkillPoints = 0
                };

                this.PostUpdateCommands.AddComponent(entity, lvlComponent);
                this.PostUpdateCommands.AddComponent(entity, new AttackStatComponent
                {
                    BaseValue = 5,
                    CurrentValue = 6 * (lvlComponent.CurrentLevel - 1) / 2
                });
                #endregion

                this.PostUpdateCommands.AddComponent(entity, new CharacterComponent
                {
                    CharacterType = CharacterType.Ally
                });
                this.PostUpdateCommands.AddComponent(entity, new MovementComponent
                {
                    Direction = float2.zero,
                    Speed = allyInitializeComponent.MovementSpeed
                });

                this.PostUpdateCommands.AddComponent(entity, new WeaponComponent
                {
                    DamageValue = allyInitializeComponent.WeaponDamage,
                    AttackRange = allyInitializeComponent.AttackRange,
                    CoolDown = allyInitializeComponent.AttackCoolDown
                });
                this.PostUpdateCommands.AddComponent(entity, new IdleStateComponent
                {
                    StartedAt = Time.time
                });
                this.PostUpdateCommands.AddComponent(entity, new PositionComponent
                {
                    InitialPosition = new float2(transform.position.x, transform.position.y)
                });

                GameObject.Destroy(allyInitializeComponent);
            });
        }
    }
}
