using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class InspectStateSystem : JobComponentSystem
    {
        [DisableAutoCreation]
        private class InspectStateBarrier : BarrierSystem { }

        [RequireSubtractiveComponent(typeof(AttackStateComponent), typeof(FollowStateComponent))]
        private struct InspectStateJob : IJobProcessComponentDataWithEntity<MovementComponent, InspectStateComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public Unity.Mathematics.Random Random;
            public float CurrentTime;

            public void Execute(Entity entity,
                                int index,
                                ref MovementComponent movementComponent,
                                ref InspectStateComponent inspectStateComponent)
            {
                if (CurrentTime - inspectStateComponent.StartedAt < Random.NextInt(10, 30) / 10f)
                {
                    if (inspectStateComponent.InspectDirection.Equals(float2.zero))
                    {
                        inspectStateComponent.InspectDirection = 
                            new float2(Random.NextFloat(-10, 10), Random.NextFloat(-10, 10));
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

        private InspectStateBarrier _inspectStateBarrier;

        protected override void OnCreateManager()
        {
            _inspectStateBarrier = World.Active.GetOrCreateManager<InspectStateBarrier>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, uint.MaxValue));
            var handle = new InspectStateJob
            {
                CommandBuffer = _inspectStateBarrier.CreateCommandBuffer().ToConcurrent(),
                Random = random,
                CurrentTime = Time.time
            }.Schedule(this, inputDeps);
            _inspectStateBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
