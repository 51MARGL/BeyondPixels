using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class IdleStateSystem : JobComponentSystem
    {
        [ExcludeComponent(typeof(AttackStateComponent), typeof(FollowStateComponent))]
        private struct IdleStateJob : IJobProcessComponentDataWithEntity<IdleStateComponent, PositionComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public float CurrentTime;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref IdleStateComponent idleStateComponent,
                                [ReadOnly] ref PositionComponent positionComponent)
            {
                var random = new System.Random((int)this.CurrentTime + index);
                if (this.CurrentTime - idleStateComponent.StartedAt < random.Next(10, 50) / 10f)
                    return;

                this.CommandBuffer.RemoveComponent(index, entity, typeof(IdleStateComponent));
                if (math.distance(positionComponent.CurrentPosition, positionComponent.InitialPosition) < 1)
                    this.CommandBuffer.AddComponent(index, entity,
                        new InspectStateComponent
                        {
                            StartedAt = CurrentTime
                        });
                else
                    this.CommandBuffer.AddComponent(index, entity,
                        new EvadeStateComponent
                        {
                            StartedAt = CurrentTime
                        });
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreateManager()
        {
            this._endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new IdleStateJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                CurrentTime = Time.time
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
