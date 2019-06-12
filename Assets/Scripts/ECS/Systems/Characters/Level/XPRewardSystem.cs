﻿using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Level
{
    public class XPRewardSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(CollectXPRewardComponent))]
        private struct XPRewardSystemJob : IJobProcessComponentDataWithEntity<XPRewardComponent, LevelComponent>
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
                                [ReadOnly] ref XPRewardComponent xpRewardComponent,
                                [ReadOnly] ref LevelComponent levelComponent)
            {
                for (var c = 0; c < this.Chunks.Length; c++)
                {
                    var chunk = this.Chunks[c];
                    var entities = chunk.GetNativeArray(this.EntityType);
                    var xpComponentComponents = chunk.GetNativeArray(this.XPComponentType);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        var xpComponentComponent = xpComponentComponents[i];
                        xpComponentComponent.CurrentXP += xpRewardComponent.XPAmount * levelComponent.CurrentLevel;
                        this.CommandBuffer.SetComponent(index, entities[i], xpComponentComponent);
                    }
                }
                this.CommandBuffer.RemoveComponent<CollectXPRewardComponent>(index, entity);
                this.CommandBuffer.RemoveComponent<XPRewardComponent>(index, entity);
            }
        }
        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private ComponentGroup _healthGroup;

        protected override void OnCreateManager()
        {
            this._endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            this._healthGroup = this.GetComponentGroup(new EntityArchetypeQuery
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
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                Chunks = this._healthGroup.CreateArchetypeChunkArray(Allocator.TempJob),
                EntityType = this.GetArchetypeChunkEntityType(),
                XPComponentType = this.GetArchetypeChunkComponentType<XPComponent>()
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(destroyJobHandle);
            return destroyJobHandle;
        }
    }
}
