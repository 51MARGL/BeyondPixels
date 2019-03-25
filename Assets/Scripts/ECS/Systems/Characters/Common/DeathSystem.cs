using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Objects;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    public class DeathSystem : JobComponentSystem
    {
        [ExcludeComponent(typeof(DestroyComponent))]
        private struct DamageJob : IJobProcessComponentDataWithEntity<HealthComponent, CharacterComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index,
                                [ReadOnly] ref HealthComponent healthComponent,
                                [ReadOnly] ref CharacterComponent characterComponent)
            {
                if (healthComponent.CurrentValue <= 0)
                {
                    CommandBuffer.AddComponent(index, entity, new DestroyComponent());
                    return;
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
            var handle = new DamageJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
