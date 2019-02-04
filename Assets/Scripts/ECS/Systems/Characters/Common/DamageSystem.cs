using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    public class DamageSystem : JobComponentSystem
    {
        [DisableAutoCreation]
        public class DamageBarrier : BarrierSystem { }

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
                if (healthComponent.CurrentValue <= 0)
                    healthComponent.CurrentValue = 0;
                else if (healthComponent.CurrentValue > healthComponent.MaxValue)
                    healthComponent.CurrentValue = healthComponent.MaxValue;
                CommandBuffer.SetComponent(index, collisionInfo.Other, healthComponent);

                CommandBuffer.DestroyEntity(index, entity);
            }
        }
        private DamageBarrier _damageBarrier;

        [Inject]
        private ComponentDataFromEntity<HealthComponent> _healthComponents;

        protected override void OnCreateManager()
        {
            _damageBarrier = World.Active.GetOrCreateManager<DamageBarrier>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new DamageJob
            {
                CommandBuffer = _damageBarrier.CreateCommandBuffer().ToConcurrent(),
                HealthComponents = _healthComponents,
            }.Schedule(this, inputDeps);
            _damageBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
