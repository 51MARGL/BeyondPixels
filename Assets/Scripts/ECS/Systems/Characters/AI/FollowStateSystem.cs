using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Objects;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.AI;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class FollowStateSystem : ComponentSystem
    {
        private ComponentGroup _followGroup;
        private ComponentGroup _targetGroup;

        protected override void OnCreateManager()
        {
            this._followGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(MovementComponent), typeof(FollowStateComponent),
                    typeof(WeaponComponent), typeof(PositionComponent), typeof(NavMeshAgent)
                },
                None = new ComponentType[]
                {
                    typeof(AttackStateComponent)
                }
            });
            this._targetGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(PositionComponent), typeof(PlayerComponent)
                },
                None = new ComponentType[]
                {
                    typeof(DestroyComponent), typeof(InCutsceneComponent)
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
                if (this._targetGroup.CalculateLength() == 0)
                {
                    this.PostUpdateCommands.RemoveComponent<FollowStateComponent>(entity);
                    return;
                }

                var flwStateComponent = followStateComponent;
                var wpnComponent = weaponComponent;
                var currentPosition = positionComponent.CurrentPosition;
                var mvmComponent = movementComponent;

                this.Entities.With(this._targetGroup).ForEach((Entity playerEntity, ref PositionComponent playerPositionComponent) =>
                {
                    if (playerEntity == flwStateComponent.Target)
                    {
                        var distance = math.distance(playerPositionComponent.CurrentPosition, currentPosition);
                        if (distance <= wpnComponent.AttackRange)
                            mvmComponent.Direction = float2.zero;
                        else
                        {
                            var curr = new Vector3(currentPosition.x, currentPosition.y, 0);
                            var dest = new Vector3(playerPositionComponent.CurrentPosition.x, playerPositionComponent.CurrentPosition.y, 0);
                            navMeshAgent.nextPosition = curr;
                            navMeshAgent.SetDestination(dest);

                            if (navMeshAgent.path.status == NavMeshPathStatus.PathComplete)
                                mvmComponent.Direction = new float2(navMeshAgent.desiredVelocity.x, navMeshAgent.desiredVelocity.y);
                        }

                        var currentTime = Time.time;
                        if (distance <= wpnComponent.AttackRange
                            && currentTime - flwStateComponent.LastTimeAttacked > wpnComponent.CoolDown)
                        {
                            mvmComponent.Direction = playerPositionComponent.CurrentPosition - currentPosition; ;
                            flwStateComponent.LastTimeAttacked = currentTime;
                            this.PostUpdateCommands.AddComponent(entity,
                                new AttackStateComponent
                                {
                                    StartedAt = currentTime,
                                    Target = flwStateComponent.Target
                                });
                        }
                        return;
                    }
                });
                followStateComponent = flwStateComponent;
                movementComponent = mvmComponent;
            });
        }
    }
}
