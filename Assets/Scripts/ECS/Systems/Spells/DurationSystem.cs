using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Components.Spells;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Spells
{
    public class DurationSystem : JobComponentSystem
    {
        [DisableAutoCreation]
        public class DurationBarrier : BarrierSystem { }

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
        private DurationBarrier _durationBarrier;

        protected override void OnCreateManager()
        {
            _durationBarrier = World.Active.GetOrCreateManager<DurationBarrier>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new DurationJob
            {
                CommandBuffer = _durationBarrier.CreateCommandBuffer().ToConcurrent(),
                DeltaTime = Time.deltaTime
            }.Schedule(this, inputDeps);
            _durationBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
