using BeyondPixels.ColliderEvents;
using BeyondPixels.Components.Characters.Spells;
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
            public ComponentDataFromEntity<TargetRequiredComponent> TargetComponents;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref CollisionInfo collisionInfo,
                                [ReadOnly] ref SpellCollisionComponent spellCollisionComponent)
            {
                if (TargetComponents.Exists(collisionInfo.Sender)
                    && collisionInfo.Other != TargetComponents[collisionInfo.Sender].Target)
                {
                    CommandBuffer.DestroyEntity(entity);
                    return;
                }

                var damageComponent = DamageComponents[collisionInfo.Sender];
                switch (collisionInfo.EventType)
                {
                    case EventType.TriggerEnter:
                        CommandBuffer.CreateEntity();
                        CommandBuffer.AddComponent(collisionInfo);
                        CommandBuffer.AddComponent(
                                new Components.Characters.Common.DamageComponent
                                {
                                    DamageType = damageComponent.DamageType,
                                    DamageValue = damageComponent.DamageOnImpact                                    
                                });
                        break;
                    case EventType.TriggerStay:
                        CommandBuffer.CreateEntity();
                        CommandBuffer.AddComponent(collisionInfo);
                        CommandBuffer.AddComponent(
                                new Components.Characters.Common.DamageComponent
                                {
                                    DamageType = damageComponent.DamageType,
                                    DamageValue = damageComponent.DamagePerSecond
                                });
                        break;
                }


                CommandBuffer.DestroyEntity(entity);
            }
        }
        [Inject]
        private ComponentDataFromEntity<DamageComponent> _damageComponents;
        [Inject]
        private ComponentDataFromEntity<TargetRequiredComponent> _targetComponents;
        [Inject]
        private ApplyDamageSystemBarrier _barrier;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ApplyDamageJob
            {
                CommandBuffer = _barrier.CreateCommandBuffer(),
                DamageComponents = _damageComponents,
                TargetComponents = _targetComponents
            }.Schedule(this, inputDeps);
        }

        public class ApplyDamageSystemBarrier : BarrierSystem { }
    }
}
