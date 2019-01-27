using BeyondPixels.ColliderEvents;
using BeyondPixels.Components.Characters.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.Systems.Characters.Common
{
    public class DamageSystem : JobComponentSystem
    {
        private struct DamageJob : IJobProcessComponentDataWithEntity<CollisionInfo, DamageComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            public ComponentDataFromEntity<HealthComponent> HealthComponents;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref CollisionInfo collisionInfo,
                                [ReadOnly] ref DamageComponent damageComponent)
            {
                var healthComponent = HealthComponents[collisionInfo.Other];
                healthComponent.CurrentValue -= damageComponent.DamageValue;
                if (healthComponent.CurrentValue < 0)
                    healthComponent.CurrentValue = 0;
                else if (healthComponent.CurrentValue > healthComponent.MaxValue)
                    healthComponent.CurrentValue = healthComponent.MaxValue;
                CommandBuffer.SetComponent(collisionInfo.Other, healthComponent);

                CommandBuffer.DestroyEntity(entity);
            }
        }
        [Inject]
        private DamageBarrier _barrier;
        [Inject]
        private ComponentDataFromEntity<HealthComponent> _healthComponents;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new DamageJob
            {
                CommandBuffer = _barrier.CreateCommandBuffer(),
                HealthComponents = _healthComponents,
            }.Schedule(this, inputDeps);
        }

        public class DamageBarrier : BarrierSystem { }
    }
}
