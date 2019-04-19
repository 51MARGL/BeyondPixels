using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Stats
{
    [UpdateAfter(typeof(AttackStatSystem))]
    [UpdateAfter(typeof(DefenceStatSystem))]
    [UpdateAfter(typeof(HealthStatSystem))]
    [UpdateAfter(typeof(MagicStatSystem))]
    public class AfterStatsAdjustSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(AdjustStatsComponent))]
        private struct AfterStatsAdjustJob : IJobProcessComponentDataWithEntity<LevelComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref LevelComponent levelComponent)
            {
                this.CommandBuffer.RemoveComponent<AdjustStatsComponent>(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreateManager()
        {
            this._endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new AfterStatsAdjustJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
