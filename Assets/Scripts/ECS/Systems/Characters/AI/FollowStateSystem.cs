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
        private EntityQuery _followGroup;
        private EntityQuery _targetGroup;

        protected override void OnCreate()
        {
            this._followGroup = this.GetEntityQuery(new EntityQueryDesc
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
                if (!EntityManager.Exists(followStateComponent.Target)
                    || EntityManager.HasComponent<InCutsceneComponent>(followStateComponent.Target))
                {
                    this.PostUpdateCommands.RemoveComponent<FollowStateComponent>(entity);
                    return;
                }

                var currentPosition = positionComponent.CurrentPosition;
                var targetPosition = this.EntityManager.GetComponentData<PositionComponent>(followStateComponent.Target).CurrentPosition;

                var distance = math.distance(targetPosition, currentPosition);
                if (distance <= weaponComponent.AttackRange)
                    movementComponent.Direction = float2.zero;
                else
                {
                    var curr = new Vector3(currentPosition.x, currentPosition.y, 0);
                    var dest = new Vector3(targetPosition.x, targetPosition.y, 0);
                    navMeshAgent.nextPosition = curr;
                    navMeshAgent.SetDestination(dest);

                    if (navMeshAgent.path.status != NavMeshPathStatus.PathComplete)
                        movementComponent.Direction = float2.zero;
                    else
                        movementComponent.Direction = new float2(navMeshAgent.desiredVelocity.x, navMeshAgent.desiredVelocity.y);
                }

                var currentTime = Time.time;
                if (distance <= weaponComponent.AttackRange
                    && currentTime - followStateComponent.LastTimeAttacked > weaponComponent.CoolDown)
                {
                    movementComponent.Direction = targetPosition - currentPosition; ;
                    followStateComponent.LastTimeAttacked = currentTime;
                    this.PostUpdateCommands.AddComponent(entity,
                        new AttackStateComponent
                        {
                            StartedAt = currentTime,
                            Target = followStateComponent.Target
                        });
                }
            });
        }
    }
}
