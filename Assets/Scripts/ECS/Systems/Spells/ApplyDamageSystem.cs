﻿using BeyondPixels.ColliderEvents;
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
        [DisableAutoCreation]
        public class ApplyDamageSystemBarrier : BarrierSystem { }

        private struct ApplyDamageJob : IJobProcessComponentDataWithEntity<CollisionInfo, SpellCollisionComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            public ComponentDataFromEntity<DamageComponent> DamageComponents;
            [ReadOnly]
            public ComponentDataFromEntity<CharacterComponent> CharacterComponents;
            [ReadOnly]
            public ComponentDataFromEntity<TargetRequiredComponent> TargetRequiredComponents;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref CollisionInfo collisionInfo,
                                [ReadOnly] ref SpellCollisionComponent spellCollisionComponent)
            {
                if (CharacterComponents[collisionInfo.Other].CharacterType 
                    == CharacterComponents[collisionInfo.Sender].CharacterType
                    && TargetRequiredComponents[spellCollisionComponent.SpellEntity].Target != collisionInfo.Other)
                {
                    CommandBuffer.DestroyEntity(index, entity);
                    return;
                }

                var damageComponent = DamageComponents[spellCollisionComponent.SpellEntity];
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
        [Inject]
        private ComponentDataFromEntity<DamageComponent> _damageComponents;
        [Inject]
        private ComponentDataFromEntity<CharacterComponent> _characterComponents;
        [Inject]
        public ComponentDataFromEntity<TargetRequiredComponent> _targetRequiredComponents;

        private ApplyDamageSystemBarrier _applyDamageBarrier;

        protected override void OnCreateManager()
        {
            _applyDamageBarrier = World.Active.GetOrCreateManager<ApplyDamageSystemBarrier>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new ApplyDamageJob
            {
                CommandBuffer = _applyDamageBarrier.CreateCommandBuffer().ToConcurrent(),
                DamageComponents = _damageComponents,
                CharacterComponents = _characterComponents,
                TargetRequiredComponents = _targetRequiredComponents
            }.Schedule(this, inputDeps);
            _applyDamageBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
