using BeyondPixels.Components.Characters.Spells;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BeyondPixels.Systems.Spells
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
                    CommandBuffer.AddComponent(entity, new DestroyComponent());
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
                CommandBuffer = _barrier.CreateCommandBuffer(),
                DeltaTime = Time.deltaTime
            }.Schedule(this, inputDeps);
        }

        public class DurationBarrier : BarrierSystem { }
    }
}
