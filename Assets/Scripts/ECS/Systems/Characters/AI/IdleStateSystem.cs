using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class IdleStateSystem : JobComponentSystem
    {
        [RequireSubtractiveComponent(typeof(AttackStateComponent), typeof(FollowStateComponent))]
        private struct IdleStateJob : IJobProcessComponentDataWithEntity<IdleStateComponent, PositionComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public float CurrentTime;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref IdleStateComponent idleStateComponent,
                                [ReadOnly] ref PositionComponent positionComponent)
            {
                var random = new System.Random((int)CurrentTime + index);
                if (CurrentTime - idleStateComponent.StartedAt < random.Next(10, 50) / 10f)
                    return;

                CommandBuffer.RemoveComponent(index, entity, typeof(IdleStateComponent));
                if (Vector2.Distance(positionComponent.CurrentPosition, positionComponent.InitialPosition) < 1)
                    CommandBuffer.AddComponent(index, entity,
                        new InspectStateComponent
                        {
                            StartedAt = CurrentTime
                        });
                else
                    CommandBuffer.AddComponent(index, entity,
                        new EvadeStateComponent
                        {
                            StartedAt = CurrentTime
                        });
            }
        }

        [Inject]
        private IdleStateBarrier _idleStateBarrier;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new IdleStateJob
            {
                CommandBuffer = _idleStateBarrier.CreateCommandBuffer().ToConcurrent(),
                CurrentTime = Time.time
            }.Schedule(this, inputDeps);
        }

        private class IdleStateBarrier : BarrierSystem { }
    }
}
