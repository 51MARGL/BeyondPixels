using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class FollowStateSystem : JobComponentSystem
    {
        [RequireSubtractiveComponent(typeof(AttackStateComponent))]
        private struct FollowStateJob :
            IJobProcessComponentDataWithEntity<MovementComponent, FollowStateComponent, WeaponComponent, PositionComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            public ComponentDataFromEntity<PositionComponent> Positions;
            public float CurrentTime;

            public void Execute(Entity entity,
                                int index,
                                ref MovementComponent movementComponent,
                                ref FollowStateComponent followStateComponent,
                                [ReadOnly] ref WeaponComponent weaponComponent,
                                [ReadOnly] ref PositionComponent positionComponent)
            {
                var target = followStateComponent.Target;
                var targetPosition = Positions[target];

                movementComponent.Direction = targetPosition.CurrentPosition - positionComponent.CurrentPosition;
                var distance = Vector2.Distance(targetPosition.CurrentPosition, positionComponent.CurrentPosition);

                if (distance <= weaponComponent.AttackRange
                    && CurrentTime - followStateComponent.LastTimeAttacked > weaponComponent.CoolDown)
                {
                    followStateComponent.LastTimeAttacked = CurrentTime;
                    movementComponent.Direction = Vector2.zero;
                    CommandBuffer.AddComponent(index, entity,
                        new AttackStateComponent
                        {
                            StartedAt = CurrentTime,
                            Target = target
                        });
                }
            }
        }

        [Inject]
        private FollowStateBarrier _barrier;
        [Inject]
        private ComponentDataFromEntity<PositionComponent> _positions;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new FollowStateJob
            {
                CommandBuffer = _barrier.CreateCommandBuffer().ToConcurrent(),
                Positions = _positions,
                CurrentTime = Time.time
            }.Schedule(this, inputDeps);
        }

        public class FollowStateBarrier : BarrierSystem { }
    }
}
