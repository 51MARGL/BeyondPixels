using BeyondPixels.ECS.Components.Characters.Common;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    public class DeathSystem : JobComponentSystem
    {
        [ExcludeComponent(typeof(KilledComponent))]
        private struct DeathJob : IJobProcessComponentDataWithEntity<HealthComponent, CharacterComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref HealthComponent healthComponent,
                                [ReadOnly] ref CharacterComponent characterComponent)
            {
                if (healthComponent.CurrentValue <= 0)
                    this.CommandBuffer.AddComponent(index, entity, new KilledComponent());
            }
        }
        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreateManager()
        {
            this._endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var destroyJobHandle = new DeathJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(destroyJobHandle);
            return destroyJobHandle;
        }
    }
}
