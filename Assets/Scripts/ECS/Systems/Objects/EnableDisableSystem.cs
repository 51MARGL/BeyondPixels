using BeyondPixels.ECS.Components.Objects;

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
                this.CommandBuffer.RemoveComponent<Disabled>(index, entityEnableComponent.Target);
                this.CommandBuffer.DestroyEntity(index, entity);
            }
        }
        private struct DisableJob : IJobProcessComponentDataWithEntity<EntityDisableComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref EntityDisableComponent entityDisableComponent)
            {
                this.CommandBuffer.AddComponent(index, entityDisableComponent.Target, new Disabled());
                this.CommandBuffer.DestroyEntity(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreateManager()
        {
            this._endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inpuDeps)
        {
            var enableJobHanlde = new EnableJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, inpuDeps);
            var disableJobHanlde = new DisableJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, enableJobHanlde);
            this._endFrameBarrier.AddJobHandleForProducer(disableJobHanlde);
            return disableJobHanlde;
        }
    }
}
