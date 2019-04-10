using BeyondPixels.ECS.Components.Characters.Level;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Level
{
    public class LevelSystem : JobComponentSystem
    {
        [ExcludeComponent(typeof(LevelUpComponent))]
        private struct LevelJob : IJobProcessComponentDataWithEntity<LevelComponent, XPComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity,
                                int index,
                                ref LevelComponent levelComponent,
                                [ReadOnly] ref XPComponent xpComponent)
            {
                if (xpComponent.CurrentXP >= levelComponent.NextLevelXP)
                    CommandBuffer.AddComponent(index, entity, new LevelUpComponent());
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreateManager()
        {
            _endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new LevelJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
