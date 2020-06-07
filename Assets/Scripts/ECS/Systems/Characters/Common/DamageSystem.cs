using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Objects;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    public class DamageSystem : JobComponentSystem
    {
        [BurstCompile]
        private struct DamageJob : IJobForEachWithEntity<CollisionInfo, FinalDamageComponent>
        {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;
            public ArchetypeChunkComponentType<HealthComponent> HealthComponentType;
            [ReadOnly]
            public ArchetypeChunkComponentType<InCutsceneComponent> InCutsceneComponentType;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref CollisionInfo collisionInfo,
                                [ReadOnly] ref FinalDamageComponent damageComponent)
            {
                for (var c = 0; c < this.Chunks.Length; c++)
                {
                    var chunk = this.Chunks[c];
                    var entities = chunk.GetNativeArray(this.EntityType);
                    var healthComponents = chunk.GetNativeArray(this.HealthComponentType);
                    if (!chunk.Has(this.InCutsceneComponentType))
                    {
                        for (var i = 0; i < chunk.Count; i++)
                        {
                            if (entities[i] == collisionInfo.Target)
                            {
                                var damageAmount = damageComponent.DamageAmount;

                                var healthComponent = healthComponents[i];

                                healthComponent.CurrentValue -= damageAmount;
                                if (healthComponent.CurrentValue < 0)
                                {
                                    healthComponent.CurrentValue = 0;
                                }
                                else if (healthComponent.CurrentValue > healthComponent.MaxValue)
                                {
                                    healthComponent.CurrentValue = healthComponent.MaxValue;
                                }

                                healthComponents[i] = healthComponent;
                                return;
                            }
                        }
                    }
                }
            }
        }

        private struct CleanUpJob : IJobForEachWithEntity<CollisionInfo, FinalDamageComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref CollisionInfo collisionInfo,
                                [ReadOnly] ref FinalDamageComponent damageComponent)
            {
                this.CommandBuffer.DestroyEntity(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private EntityQuery _healthGroup;

        protected override void OnCreate()
        {
            this._endFrameBarrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            this._healthGroup = this.GetEntityQuery(new EntityQueryDesc
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
            var damageHandle = new DamageJob
            {
                Chunks = this._healthGroup.CreateArchetypeChunkArray(Allocator.TempJob),
                EntityType = this.GetArchetypeChunkEntityType(),
                HealthComponentType = this.GetArchetypeChunkComponentType<HealthComponent>(),
                InCutsceneComponentType = this.GetArchetypeChunkComponentType<InCutsceneComponent>()
            }.Schedule(this, inputDeps);

            var cleanUphandle = new CleanUpJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, damageHandle);
            this._endFrameBarrier.AddJobHandleForProducer(cleanUphandle);
            return cleanUphandle;
        }
    }
}
