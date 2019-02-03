using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Common
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
                healthComponent.CurrentValue -= damageComponent.DamageOnImpact;
                if (healthComponent.CurrentValue < 0)
                    healthComponent.CurrentValue = 0;
                else if (healthComponent.CurrentValue > healthComponent.MaxValue)
                    healthComponent.CurrentValue = healthComponent.MaxValue;
                CommandBuffer.SetComponent(index, collisionInfo.Other, healthComponent);

                CommandBuffer.DestroyEntity(index, entity);
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
                CommandBuffer = _barrier.CreateCommandBuffer().ToConcurrent(),
                HealthComponents = _healthComponents,
            }.Schedule(this, inputDeps);
        }

        public class DamageBarrier : BarrierSystem { }
    }
}
