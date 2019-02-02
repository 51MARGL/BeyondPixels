using BeyondPixels.ColliderEvents;
using BeyondPixels.Components.Characters.Common;
using BeyondPixels.Components.Spells;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.Systems.Spells
{
    public class ApplyDamageSystem : JobComponentSystem
    {
        private struct ApplyDamageJob : IJobProcessComponentDataWithEntity<CollisionInfo, SpellCollisionComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            public ComponentDataFromEntity<DamageComponent> DamageComponents;
            [ReadOnly]
            public ComponentDataFromEntity<CharacterComponent> CharacterComponents;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref CollisionInfo collisionInfo,
                                [ReadOnly] ref SpellCollisionComponent spellCollisionComponent)
            {
                if (CharacterComponents[collisionInfo.Other].CharacterType 
                    == CharacterComponents[collisionInfo.Sender].CharacterType)
                {
                    CommandBuffer.DestroyEntity(entity);
                    return;
                }

                var damageComponent = DamageComponents[spellCollisionComponent.SpellEntity];
                switch (collisionInfo.EventType)
                {
                    case EventType.TriggerEnter:
                        CommandBuffer.CreateEntity();
                        CommandBuffer.AddComponent(collisionInfo);
                        CommandBuffer.AddComponent(
                                new DamageComponent
                                {
                                    DamageType = damageComponent.DamageType,
                                    DamageOnImpact = damageComponent.DamageOnImpact                                    
                                });
                        break;
                    case EventType.TriggerStay:
                        CommandBuffer.CreateEntity();
                        CommandBuffer.AddComponent(collisionInfo);
                        CommandBuffer.AddComponent(
                                new DamageComponent
                                {
                                    DamageType = damageComponent.DamageType,
                                    DamageOnImpact = damageComponent.DamagePerSecond
                                });
                        break;
                }


                CommandBuffer.DestroyEntity(entity);
            }
        }
        [Inject]
        private ComponentDataFromEntity<DamageComponent> _damageComponents;
        [Inject]
        private ComponentDataFromEntity<CharacterComponent> _characterComponents;
        [Inject]
        private ApplyDamageSystemBarrier _barrier;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ApplyDamageJob
            {
                CommandBuffer = _barrier.CreateCommandBuffer(),
                DamageComponents = _damageComponents,
                CharacterComponents = _characterComponents
            }.Schedule(this, inputDeps);
        }

        public class ApplyDamageSystemBarrier : BarrierSystem { }
    }
}
