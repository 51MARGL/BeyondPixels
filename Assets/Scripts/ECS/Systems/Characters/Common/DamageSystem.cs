using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Objects;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    public class DamageSystem : JobComponentSystem
    {
        private struct DamageJob : IJobProcessComponentDataWithEntity<CollisionInfo, FinalDamageComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;
            public ArchetypeChunkComponentType<HealthComponent> HealthComponentType;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref CollisionInfo collisionInfo,
                                [ReadOnly] ref FinalDamageComponent damageComponent)
            {
                for (int c = 0; c < Chunks.Length; c++)
                {
                    var chunk = Chunks[c];
                    var entities = chunk.GetNativeArray(EntityType);
                    var healthComponents = chunk.GetNativeArray(HealthComponentType);
                    for (int i = 0; i < chunk.Count; i++)
                        if (entities[i] == collisionInfo.Target)
                        {
                            var healthComponent = healthComponents[i];

                            healthComponent.CurrentValue -= damageComponent.DamageAmount;
                            if (healthComponent.CurrentValue < 0)
                                healthComponent.CurrentValue = 0;
                            else if (healthComponent.CurrentValue > healthComponent.MaxValue)
                                healthComponent.CurrentValue = healthComponent.MaxValue;

                            CommandBuffer.SetComponent(index, entities[i], healthComponent);

                            CommandBuffer.DestroyEntity(index, entity);
                            return;
                        }
                }

                CommandBuffer.DestroyEntity(index, entity);
            }
        }
        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private ComponentGroup _healthGroup;

        protected override void OnCreateManager()
        {
            _endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            _healthGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(HealthComponent)
                },
                None = new ComponentType[]
                {
                    typeof(DestroyComponent)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new DamageJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                Chunks = _healthGroup.CreateArchetypeChunkArray(Allocator.TempJob),
                EntityType = GetArchetypeChunkEntityType(),
                HealthComponentType = GetArchetypeChunkComponentType<HealthComponent>()
            }.Schedule(this, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
