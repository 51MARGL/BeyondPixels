using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class InspectStateSystem : JobComponentSystem
    {
        [RequireSubtractiveComponent(typeof(AttackStateComponent), typeof(FollowStateComponent))]
        private struct InspectStateJob : IJobProcessComponentDataWithEntity<MovementComponent, InspectStateComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public float CurrentTime;

            public void Execute(Entity entity,
                                int index,
                                ref MovementComponent movementComponent,
                                ref InspectStateComponent inspectStateComponent)
            {
                var random = new System.Random((int)CurrentTime + index);
                if (CurrentTime - inspectStateComponent.StartedAt < random.Next(10, 30) / 10f)
                {
                    if (inspectStateComponent.InspectDirection == Vector2.zero)
                    {
                        inspectStateComponent.InspectDirection = new Vector2(random.Next(-10, 10) / 10f, random.Next(-10, 10) / 10f);
                    }
                    movementComponent.Direction = inspectStateComponent.InspectDirection;
                }
                else
                {
                    movementComponent.Direction = Vector2.zero;

                    CommandBuffer.RemoveComponent(index, entity, typeof(InspectStateComponent));
                    CommandBuffer.AddComponent(index, entity,
                        new IdleStateComponent
                        {
                            StartedAt = CurrentTime
                        });
                }
            }
        }

        [Inject]
        private InspectStateBarrier _inspectStateBarrier;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new InspectStateJob
            {
                CommandBuffer = _inspectStateBarrier.CreateCommandBuffer().ToConcurrent(),
                CurrentTime = Time.time
            }.Schedule(this, inputDeps);
        }

        private class InspectStateBarrier : BarrierSystem { }
    }
}
