using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Spells;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.AI;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class FollowStateSystem : ComponentSystem
    {
        private EntityQuery _followGroup;
        private EntityQuery _activeSpellGroup;
        private Unity.Mathematics.Random _random;

        protected override void OnCreate()
        {
            this._random = new Unity.Mathematics.Random((uint)System.Guid.NewGuid().GetHashCode());

            this._followGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(MovementComponent), typeof(FollowStateComponent),
                    typeof(WeaponComponent), typeof(PositionComponent), typeof(NavMeshAgent)
                },
                None = new ComponentType[]
                {
                    typeof(AttackStateComponent), typeof(SpellCastingComponent)
                }
            });
            this._activeSpellGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    ComponentType.ReadOnly<ActiveSpellComponent>()
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<CoolDownComponent>(),
                    ComponentType.ReadOnly<InstantiateSpellComponent>()
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._followGroup).ForEach((Entity entity,
                                                NavMeshAgent navMeshAgent,
                                                ref MovementComponent movementComponent,
                                                ref FollowStateComponent followStateComponent,
                                                ref WeaponComponent weaponComponent,
                                                ref PositionComponent positionComponent) =>
            {
                if (!this.EntityManager.Exists(followStateComponent.Target)
                    || this.EntityManager.HasComponent<InCutsceneComponent>(followStateComponent.Target))
                {
                    this.PostUpdateCommands.RemoveComponent<FollowStateComponent>(entity);
                    return;
                }

                var currentPosition = positionComponent.CurrentPosition;
                var targetPosition = this.EntityManager.GetComponentData<PositionComponent>(followStateComponent.Target).CurrentPosition;
                var distance = math.distance(targetPosition, currentPosition);

                var curr = new Vector3(currentPosition.x, currentPosition.y, 0);
                var dest = new Vector3(targetPosition.x, targetPosition.y, 0);
                navMeshAgent.nextPosition = curr;
                navMeshAgent.SetDestination(dest);

                if (navMeshAgent.path.status != NavMeshPathStatus.PathComplete)
                {
                    movementComponent.Direction = float2.zero;
                }
                else
                {
                    movementComponent.Direction = new float2(navMeshAgent.desiredVelocity.x, navMeshAgent.desiredVelocity.y);
                }

                if (distance <= weaponComponent.MeleeAttackRange)
                {
                    var currentTime = Time.time;
                    movementComponent.Direction = float2.zero;

                    if (currentTime - followStateComponent.LastTimeAttacked > weaponComponent.CoolDown)
                    {
                        movementComponent.Direction = targetPosition - currentPosition;
                        followStateComponent.LastTimeAttacked = currentTime;
                        this.PostUpdateCommands.AddComponent(entity,
                            new AttackStateComponent
                            {
                                StartedAt = currentTime,
                                Target = followStateComponent.Target
                            });
                    }
                }
                else if (distance <= weaponComponent.SpellAttackRange)
                {
                    var currentTime = Time.time;

                    if (currentTime - followStateComponent.LastTimeSpellChecked < weaponComponent.SpellCheckFrequency)
                    {
                        return;
                    }

                    followStateComponent.LastTimeSpellChecked = currentTime;

                    if (this._random.NextInt(0, 100) > weaponComponent.SpellCastChance)
                    {
                        return;
                    }

                    using (var spellEntities = this._activeSpellGroup.ToEntityArray(Allocator.TempJob))
                    using (var spellComponents = this._activeSpellGroup.ToComponentDataArray<ActiveSpellComponent>(Allocator.TempJob))
                    {
                        var spellList = new NativeList<(Entity entity, ActiveSpellComponent component)>(Allocator.TempJob);
                        for (var sI = 0; sI < spellEntities.Length; sI++)
                        {
                            if (spellComponents[sI].Owner == entity)
                            {
                                spellList.Add((spellEntities[sI], spellComponents[sI]));
                            }
                        }
                        if (spellList.Length > 0)
                        {
                            var index = this._random.NextInt(0, spellList.Length);
                            movementComponent.Direction = float2.zero;

                            this.PostUpdateCommands.AddComponent(entity, new SpellCastingComponent
                            {
                                SpellIndex = spellList[index].component.SpellIndex,
                                ActiveSpell = spellList[index].entity,
                                StartedAt = Time.time
                            });
                        }
                        spellList.Dispose();
                    }
                }
            });
        }
    }
}