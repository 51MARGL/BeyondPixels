using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Objects;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Level
{
    public class XPRewardSystem : JobComponentSystem
    {
        private struct XPRewardSystemJob : IJobProcessComponentDataWithEntity<DestroyComponent, XPRewardComponent, LevelComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;
            public ArchetypeChunkComponentType<XPComponent> XPComponentType;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref DestroyComponent destroyComponent,
                                [ReadOnly] ref XPRewardComponent xpRewardComponent,
                                [ReadOnly] ref LevelComponent levelComponent)
            {
                for (int c = 0; c < Chunks.Length; c++)
                {
                    var chunk = Chunks[c];
                    var entities = chunk.GetNativeArray(EntityType);
                    var xpComponentComponents = chunk.GetNativeArray(XPComponentType);
                    for (int i = 0; i < chunk.Count; i++)
                    {
                        var xpComponentComponent = xpComponentComponents[i];
                        xpComponentComponent.CurrentXP += xpRewardComponent.XPAmount * levelComponent.CurrentLevel;
                        CommandBuffer.SetComponent(index, entities[i], xpComponentComponent);
                    }
                }
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
                    typeof(PlayerComponent),
                    typeof(XPComponent)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var destroyJobHandle = new XPRewardSystemJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                Chunks = _healthGroup.CreateArchetypeChunkArray(Allocator.TempJob),
                EntityType = GetArchetypeChunkEntityType(),
                XPComponentType = GetArchetypeChunkComponentType<XPComponent>()
            }.Schedule(this, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(destroyJobHandle);
            return destroyJobHandle;
        }
    }
}
