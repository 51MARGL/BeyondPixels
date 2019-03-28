using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Objects
{
    public class EnableDisableSystem : JobComponentSystem
    {
        private struct EnableJob : IJobProcessComponentDataWithEntity<EntityEnableComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref EntityEnableComponent entityEnableComponent)
            {
                CommandBuffer.RemoveComponent<Disabled>(index, entityEnableComponent.Target);
                CommandBuffer.DestroyEntity(index, entity);
            }
        }
        private struct DisableJob : IJobProcessComponentDataWithEntity<EntityDisableComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref EntityDisableComponent entityDisableComponent)
            {
                CommandBuffer.AddComponent(index, entityDisableComponent.Target, new Disabled());
                CommandBuffer.DestroyEntity(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreateManager()
        {
            _endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inpuDeps)
        {
            var enableJobHanlde = new EnableJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, inpuDeps);
            var disableJobHanlde = new DisableJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, enableJobHanlde);
            _endFrameBarrier.AddJobHandleForProducer(disableJobHanlde);
            return disableJobHanlde;
        }
    }
}
