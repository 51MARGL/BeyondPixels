﻿using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.AI;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class InspectStateSystem : ComponentSystem
    {
        private EntityQuery _inspectGroup;
        private Unity.Mathematics.Random _random;

        protected override void OnCreate()
        {
            this._random = new Unity.Mathematics.Random((uint)System.Guid.NewGuid().GetHashCode());

            this._inspectGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(MovementComponent), typeof(InspectStateComponent),
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
            this.Entities.With(this._inspectGroup).ForEach((Entity entity,
                                                NavMeshAgent navMeshAgent,
                                                ref MovementComponent movementComponent,
                                                ref PositionComponent positionComponent,
                                                ref InspectStateComponent inspectStateComponent) =>
            {
                var currentTime = Time.time;
                if (currentTime - inspectStateComponent.StartedAt < this._random.NextInt(10, 30) / 10f)
                {
                    if (inspectStateComponent.InspectDirection.Equals(float2.zero))
                    {
                        inspectStateComponent.InspectDirection =
                            new float2(this._random.NextFloat(-20, 20), this._random.NextFloat(-20, 20));

                        var curr = new Vector3(positionComponent.CurrentPosition.x, positionComponent.CurrentPosition.y, 0);
                        var dest = curr +
                            new Vector3(inspectStateComponent.InspectDirection.x, inspectStateComponent.InspectDirection.y, 0);

                        movementComponent.Direction = inspectStateComponent.InspectDirection;
                        navMeshAgent.nextPosition = curr;
                        navMeshAgent.SetDestination(dest);
                    }

                    if (navMeshAgent.path.status != NavMeshPathStatus.PathComplete)
                        movementComponent.Direction = float2.zero;
                    else
                        movementComponent.Direction = new float2(navMeshAgent.desiredVelocity.x, navMeshAgent.desiredVelocity.y);
                }
                else
                {
                    movementComponent.Direction = float2.zero;

                    this.PostUpdateCommands.RemoveComponent(entity, typeof(InspectStateComponent));
                    this.PostUpdateCommands.AddComponent(entity,
                        new IdleStateComponent
                        {
                            StartedAt = currentTime
                        });
                }
            });
        }
    }
}
