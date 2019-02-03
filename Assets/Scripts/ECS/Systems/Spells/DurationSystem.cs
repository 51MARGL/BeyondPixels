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
        [RequireSubtractiveComponent(typeof(DestroyComponent))]
        private struct DurationJob : IJobProcessComponentDataWithEntity<DurationComponent, SpellComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public float DeltaTime;

            public void Execute(Entity entity,
                                int index,
                                ref DurationComponent durationComponent,
                                [ReadOnly] ref SpellComponent spellComponent)
            {
                if (durationComponent.Duration < 0)
                    CommandBuffer.AddComponent(index, entity, new DestroyComponent());
                else
                    durationComponent.Duration -= DeltaTime;
            }
        }
        [Inject]
        private DurationBarrier _barrier;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new DurationJob
            {
                CommandBuffer = _barrier.CreateCommandBuffer().ToConcurrent(),
                DeltaTime = Time.deltaTime
            }.Schedule(this, inputDeps);
        }

        public class DurationBarrier : BarrierSystem { }
    }
}
