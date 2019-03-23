using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Spells;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Spells
{
    public class ApplyDamageSystem : JobComponentSystem
    {
        private struct ApplyDamageJob : IJobProcessComponentDataWithEntity<CollisionInfo, SpellCollisionComponent, DamageComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;
            [ReadOnly]
            public ArchetypeChunkComponentType<CharacterComponent> CharacterComponentType;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref CollisionInfo collisionInfo,
                                [ReadOnly] ref SpellCollisionComponent spellCollisionComponent,
                                [ReadOnly] ref DamageComponent damageComponent)
            {
                CharacterType targetType = 0;
                CharacterType casterType = 0;
                for (int c = 0; c < Chunks.Length; c++)
                {
                    var chunk = Chunks[c];
                    var entities = chunk.GetNativeArray(EntityType);
                    var characterComponents = chunk.GetNativeArray(CharacterComponentType);
                    for (int i = 0; i < chunk.Count; i++)
                    {
                        if (entities[i] == collisionInfo.Other)
                            targetType = characterComponents[i].CharacterType;
                        if (entities[i] == collisionInfo.Sender)
                            casterType = characterComponents[i].CharacterType;
                    }
                }
                if (targetType == casterType
                    && spellCollisionComponent.Target != collisionInfo.Other)
                {
                    CommandBuffer.DestroyEntity(index, entity);
                    return;
                }
                
                Entity newEntity;
                switch (collisionInfo.EventType)
                {
                    case EventType.TriggerEnter:
                        newEntity = CommandBuffer.CreateEntity(index);
                        CommandBuffer.AddComponent(index, newEntity, collisionInfo);
                        CommandBuffer.AddComponent(index, newEntity,
                                new DamageComponent
                                {
                                    DamageType = damageComponent.DamageType,
                                    DamageOnImpact = damageComponent.DamageOnImpact
                                });
                        break;
                    case EventType.TriggerStay:
                        newEntity = CommandBuffer.CreateEntity(index);
                        CommandBuffer.AddComponent(index, newEntity, collisionInfo);
                        CommandBuffer.AddComponent(index, newEntity,
                                new DamageComponent
                                {
                                    DamageType = damageComponent.DamageType,
                                    DamageOnImpact = damageComponent.DamagePerSecond
                                });
                        break;
                }

                CommandBuffer.DestroyEntity(index, entity);
            }
        }
        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private ComponentGroup _group;
        protected override void OnCreateManager()
        {
            _endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            _group = GetComponentGroup(typeof(CharacterComponent), typeof(PositionComponent));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var chunks = _group.CreateArchetypeChunkArray(Allocator.TempJob);
            var handle = new ApplyDamageJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                Chunks = chunks,
                EntityType = GetArchetypeChunkEntityType(),
                CharacterComponentType = GetArchetypeChunkComponentType<CharacterComponent>()
            }.Schedule(this, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
