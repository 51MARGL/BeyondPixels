using BeyondPixels.ECS.Components.Objects;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Objects
{
    public class DestroySystem : JobComponentSystem
    {
        private struct DestroyEntityJob : IJobForEachWithEntity<DestroyComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public void Execute(Entity entity, int index, [ReadOnly] ref DestroyComponent destroyComponent)
            {
                var syncEntity = this.CommandBuffer.CreateEntity(index);
                this.CommandBuffer.AddComponent(index, syncEntity, new SyncDestroyedComponent
                {
                    EntityID = entity.Index
                });
                this.CommandBuffer.DestroyEntity(index, entity);
            }
        }
        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreate()
        {
            this._endFrameBarrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var destroyJobHandle = new DestroyEntityJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(destroyJobHandle);
            return destroyJobHandle;
        }
    }
}
