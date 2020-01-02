using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Objects;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Common
{
    public class ApplySpellDamageSystem : JobComponentSystem
    {
        private struct ApplyDamageJob : IJobForEachWithEntity<CollisionInfo, SpellCollisionComponent, DamageComponent>
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
                for (var c = 0; c < this.Chunks.Length; c++)
                {
                    var chunk = this.Chunks[c];
                    var entities = chunk.GetNativeArray(this.EntityType);
                    var characterComponents = chunk.GetNativeArray(this.CharacterComponentType);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        if (entities[i] == collisionInfo.Target)
                            targetType = characterComponents[i].CharacterType;
                        if (entities[i] == collisionInfo.Sender)
                            casterType = characterComponents[i].CharacterType;
                    }
                }
                if ((targetType == casterType
                    && spellCollisionComponent.Target != collisionInfo.Sender)
                    || (targetType != casterType
                    && spellCollisionComponent.Target == collisionInfo.Sender))
                {
                    this.CommandBuffer.DestroyEntity(index, entity);
                    return;
                }

                Entity newEntity;
                switch (collisionInfo.EventType)
                {
                    case EventType.TriggerEnter:
                        newEntity = this.CommandBuffer.CreateEntity(index);
                        this.CommandBuffer.AddComponent(index, newEntity, collisionInfo);
                        this.CommandBuffer.AddComponent(index, newEntity,
                                new FinalDamageComponent
                                {
                                    DamageType = damageComponent.DamageType,
                                    DamageAmount = damageComponent.DamageOnImpact
                                });

                        if (spellCollisionComponent.DestroyOnImpact == 1)
                        {
                            this.CommandBuffer.AddComponent(index, spellCollisionComponent.SpellEntity, new DestroyComponent());
                        }
                        break;
                    case EventType.TriggerStay:
                        newEntity = this.CommandBuffer.CreateEntity(index);
                        this.CommandBuffer.AddComponent(index, newEntity, collisionInfo);
                        this.CommandBuffer.AddComponent(index, newEntity,
                                new FinalDamageComponent
                                {
                                    DamageType = damageComponent.DamageType,
                                    DamageAmount = damageComponent.DamagePerSecond
                                });
                        break;
                }

                this.CommandBuffer.DestroyEntity(index, entity);
            }
        }
        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private EntityQuery _group;
        protected override void OnCreate()
        {
            this._endFrameBarrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            this._group = this.GetEntityQuery(typeof(CharacterComponent), typeof(PositionComponent));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var chunks = this._group.CreateArchetypeChunkArray(Allocator.TempJob);
            var handle = new ApplyDamageJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                Chunks = chunks,
                EntityType = this.GetArchetypeChunkEntityType(),
                CharacterComponentType = this.GetArchetypeChunkComponentType<CharacterComponent>()
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
