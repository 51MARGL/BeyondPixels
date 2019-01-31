using BeyondPixels.Components.Characters.AI;
using BeyondPixels.Components.Characters.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BeyondPixels.Systems.Characters.AI
{
    public class AttackStateSystem : JobComponentSystem
    {
        private struct AttackStateJob :
            IJobProcessComponentDataWithEntity<AttackStateComponent, WeaponComponent, PositionComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            public ComponentDataFromEntity<PositionComponent> Positions;
            public float CurrentTime;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref AttackStateComponent attackComponent,
                                [ReadOnly] ref WeaponComponent weaponComponent,
                                [ReadOnly] ref PositionComponent positionComponent)
            {
                //var targetPosition = Positions[attackComponent.Target];
                //var distance = Vector2.Distance(targetPosition.CurrentPosition, positionComponent.CurrentPosition);

                //if (distance > weaponComponent.AttackRange)
                //{
                //    CommandBuffer.RemoveComponent<AttackStateComponent>(entity);
                //    return;
                //}

                if (CurrentTime - attackComponent.StartedAt < weaponComponent.CoolDown)
                    return;

                CommandBuffer.RemoveComponent<AttackStateComponent>(entity);
            }
        }

        [Inject]
        private AttackStateBarrier _attackStateBarrier;
        [Inject]
        private ComponentDataFromEntity<PositionComponent> _positions;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new AttackStateJob
            {
                CommandBuffer = _attackStateBarrier.CreateCommandBuffer(),
                Positions = _positions,
                CurrentTime = Time.time
            }.Schedule(this, inputDeps);
        }

        public class AttackStateBarrier : BarrierSystem { }
    }
}
