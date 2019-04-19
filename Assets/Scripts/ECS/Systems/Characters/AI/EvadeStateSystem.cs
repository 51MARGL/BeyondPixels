using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.AI;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    [UpdateBefore(typeof(FollowStateSystem))]
    public class EvadeStateSystem : ComponentSystem
    {
        private ComponentGroup _evadeGroup;

        protected override void OnCreateManager()
        {
            this._evadeGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(MovementComponent), typeof(EvadeStateComponent),
                    typeof(PositionComponent), typeof(NavMeshAgent)
                },
                None = new ComponentType[]
                {
                    typeof(AttackStateComponent), typeof(FollowStateComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._evadeGroup).ForEach((Entity entity,
                                                NavMeshAgent navMeshAgent,
                                                ref MovementComponent movementComponent,
                                                ref PositionComponent positionComponent) =>
            {
                if (math.distance(positionComponent.CurrentPosition, positionComponent.InitialPosition) > 0.5f)
                {
                    var curr = new Vector3(positionComponent.CurrentPosition.x, positionComponent.CurrentPosition.y, 0);
                    var dest = new Vector3(positionComponent.InitialPosition.x, positionComponent.InitialPosition.y, 0);
                    navMeshAgent.nextPosition = curr;
                    navMeshAgent.SetDestination(dest);

                    if (navMeshAgent.path.status == NavMeshPathStatus.PathComplete)
                        movementComponent.Direction = new float2(navMeshAgent.desiredVelocity.x, navMeshAgent.desiredVelocity.y);
                }
                else
                {
                    movementComponent.Direction = float2.zero;

                    this.PostUpdateCommands.RemoveComponent(entity, typeof(EvadeStateComponent));
                    this.PostUpdateCommands.AddComponent(entity,
                        new IdleStateComponent
                        {
                            StartedAt = Time.time
                        });
                }
            });
        }
    }
}
