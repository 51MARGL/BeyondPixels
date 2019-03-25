using BeyondPixels.ECS.Components.Objects;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Objects
{
    public class DestroySystem : JobComponentSystem
    {
        private struct DestroyEntityJob : IJobProcessComponentDataWithEntity<DestroyComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public void Execute(Entity entity, int index, [ReadOnly] ref DestroyComponent destroyComponent)
            {
                var syncEntity = CommandBuffer.CreateEntity(index);
                CommandBuffer.AddComponent(index, syncEntity, new SyncDestroyedComponent {
                    EntityID = entity.Index
                });
                CommandBuffer.DestroyEntity(index, entity);
            }
        }
        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreateManager()
        {
            _endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var destroyJobHandle = new DestroyEntityJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(destroyJobHandle);
            return destroyJobHandle;
        }
    }
}
