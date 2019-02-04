using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class FollowStateSystem : JobComponentSystem
    {
        [DisableAutoCreation]
        public class FollowStateBarrier : BarrierSystem { }

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
                var distance = math.distance(targetPosition.CurrentPosition, positionComponent.CurrentPosition);

                if (distance <= weaponComponent.AttackRange
                    && CurrentTime - followStateComponent.LastTimeAttacked > weaponComponent.CoolDown)
                {
                    followStateComponent.LastTimeAttacked = CurrentTime;
                    movementComponent.Direction = float2.zero;
                    CommandBuffer.AddComponent(index, entity,
                        new AttackStateComponent
                        {
                            StartedAt = CurrentTime,
                            Target = target
                        });
                }
            }
        }

        private FollowStateBarrier _followStateBarrier;

        [Inject]
        private ComponentDataFromEntity<PositionComponent> _positions;

        protected override void OnCreateManager()
        {
            _followStateBarrier = World.Active.GetOrCreateManager<FollowStateBarrier>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new FollowStateJob
            {
                CommandBuffer = _followStateBarrier.CreateCommandBuffer().ToConcurrent(),
                Positions = _positions,
                CurrentTime = Time.time
            }.Schedule(this, inputDeps);
            _followStateBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
