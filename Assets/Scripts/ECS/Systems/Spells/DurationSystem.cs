using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Components.Spells;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Spells
{
    public class DurationSystem : JobComponentSystem
    {
        [ExcludeComponent(typeof(DestroyComponent))]
        private struct DurationJob : IJobForEachWithEntity<DurationComponent, SpellComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public float DeltaTime;

            public void Execute(Entity entity,
                                int index,
                                ref DurationComponent durationComponent,
                                [ReadOnly] ref SpellComponent spellComponent)
            {
                if (durationComponent.Duration < 0)
                    this.CommandBuffer.AddComponent(index, entity, new DestroyComponent());
                else
                    durationComponent.Duration -= this.DeltaTime;
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreateManager()
        {
            this._endFrameBarrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new DurationJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                DeltaTime = Time.deltaTime
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
