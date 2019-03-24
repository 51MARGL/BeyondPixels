using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class InspectStateSystem : JobComponentSystem
    {
        [ExcludeComponent(typeof(AttackStateComponent), typeof(FollowStateComponent))]
        private struct InspectStateJob : IJobProcessComponentDataWithEntity<MovementComponent, InspectStateComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public int RandomSeed;
            public float CurrentTime;

            public void Execute(Entity entity,
                                int index,
                                ref MovementComponent movementComponent,
                                ref InspectStateComponent inspectStateComponent)
            {
                var random = new Unity.Mathematics.Random((uint)(RandomSeed + index));

                if (CurrentTime - inspectStateComponent.StartedAt < random.NextInt(10, 30) / 10f)
                {
                    if (inspectStateComponent.InspectDirection.Equals(float2.zero))
                    {
                        inspectStateComponent.InspectDirection =
                            new float2(random.NextFloat(-10, 10), random.NextFloat(-10, 10));
                    }
                    movementComponent.Direction = inspectStateComponent.InspectDirection;
                }
                else
                {
                    movementComponent.Direction = float2.zero;

                    CommandBuffer.RemoveComponent(index, entity, typeof(InspectStateComponent));
                    CommandBuffer.AddComponent(index, entity,
                        new IdleStateComponent
                        {
                            StartedAt = CurrentTime
                        });
                }
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreateManager()
        {
            _endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());
            var handle = new InspectStateJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                RandomSeed = random.NextInt(),
                CurrentTime = Time.time
            }.Schedule(this, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
