using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Systems.Characters.Stats;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Systems.Items
{
    [UpdateAfter(typeof(AfterStatsAdjustSystem))]
    public class ModifyHealthStatWithItemsSystem : JobComponentSystem
    {
        [BurstCompile]
        private struct ModifyStatJob : IJobForEachWithEntity<CharacterComponent, HealthStatComponent, HealthComponent>
        {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;
            [ReadOnly]
            public ArchetypeChunkComponentType<HealthStatModifierComponent> ModifierStatComponentType;
            [ReadOnly]
            public ArchetypeChunkComponentType<ItemComponent> ItemComponentType;
            [ReadOnly]
            public ArchetypeChunkComponentType<PickedUpComponent> PickedUpComponentType;

            public void Execute(Entity entity, int index,
                            [ReadOnly] ref CharacterComponent characterComponent,
                            ref HealthStatComponent statComponent,
                            ref HealthComponent healthComponent)
            {
                var modifier = 0;

                for (var c = 0; c < this.Chunks.Length; c++)
                {
                    var chunk = this.Chunks[c];
                    var entities = chunk.GetNativeArray(this.EntityType);
                    var modifierComponents = chunk.GetNativeArray(this.ModifierStatComponentType);
                    var itemComponents = chunk.GetNativeArray(this.ItemComponentType);
                    var pickedComponents = chunk.GetNativeArray(this.PickedUpComponentType);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        if (pickedComponents[i].Owner == entity)
                        {
                            modifier += modifierComponents[i].Value * itemComponents[i].Level;
                        }
                    }
                }

                var properValue = (statComponent.BaseValue
                                      + statComponent.PerPointValue
                                      * (statComponent.PointsSpent - 1)) + modifier;
                if (statComponent.CurrentValue != properValue)
                {
                    statComponent.CurrentValue = properValue;

                    healthComponent.MaxValue = healthComponent.BaseValue
                        + (healthComponent.BaseValue / 100f * properValue * math.log2(properValue));

                    if (healthComponent.CurrentValue > healthComponent.MaxValue)
                    {
                        healthComponent.CurrentValue = healthComponent.MaxValue;
                    }
                }
            }
        }

        [RequireComponentTag(typeof(ApplyInitialHealthModifierComponent))]
        private struct ApplyInitialJob : IJobForEachWithEntity<CharacterComponent, HealthComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index,
                            [ReadOnly] ref CharacterComponent characterComponent,
                            ref HealthComponent healthComponent)
            {

                healthComponent.CurrentValue = healthComponent.MaxValue;
                this.CommandBuffer.RemoveComponent<ApplyInitialHealthModifierComponent>(index, entity);
            }
        }

        private EntityQuery _gearGroup;
        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreate()
        {
            this._endFrameBarrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            this._gearGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(ItemComponent), typeof(EquipedComponent),
                    typeof(PickedUpComponent), typeof(HealthStatModifierComponent)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var chunks = this._gearGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            var handle = new ModifyStatJob
            {
                Chunks = chunks,
                EntityType = this.GetArchetypeChunkEntityType(),
                ModifierStatComponentType = this.GetArchetypeChunkComponentType<HealthStatModifierComponent>(),
                ItemComponentType = this.GetArchetypeChunkComponentType<ItemComponent>(),
                PickedUpComponentType = this.GetArchetypeChunkComponentType<PickedUpComponent>()
            }.Schedule(this, inputDeps);

            var handle2 = new ApplyInitialJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, handle);
            this._endFrameBarrier.AddJobHandleForProducer(handle2);
            return handle2;
        }
    }
}
