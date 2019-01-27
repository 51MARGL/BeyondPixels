using BeyondPixels.Components.Characters.AI;
using BeyondPixels.Components.Characters.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BeyondPixels.Systems.Characters.AI
{
    [UpdateBefore(typeof(FollowStateSystem))]
    public class EvadeStateSystem : JobComponentSystem
    {
        [RequireSubtractiveComponent(typeof(AttackStateComponent), typeof(FollowStateComponent))]
        private struct EvadeStateJob :
            IJobProcessComponentDataWithEntity<MovementComponent, PositionComponent, EvadeStateComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public float CurrentTime;

            public void Execute(Entity entity,
                                int index,
                                ref MovementComponent movementComponent,
                                [ReadOnly] ref PositionComponent positionComponent,
                                [ReadOnly] ref EvadeStateComponent evadeComponent)
            {
                //If the distance is larger than trashold then keep moving                
                if (Vector2.Distance(positionComponent.CurrentPosition, positionComponent.InitialPosition) > 1f)
                    movementComponent.Direction =
                        positionComponent.InitialPosition - positionComponent.CurrentPosition;
                else
                {
                    movementComponent.Direction = Vector2.zero;

                    CommandBuffer.RemoveComponent(entity, typeof(EvadeStateComponent));
                    CommandBuffer.AddComponent(entity,
                        new IdleStateComponent
                        {
                            StartedAt = CurrentTime
                        });
                }
            }
        }

        [Inject]
        private EvadeStateBarrier _EvadeStateBarrier;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new EvadeStateJob
            {
                CommandBuffer = _EvadeStateBarrier.CreateCommandBuffer(),
                CurrentTime = Time.time
            }.Schedule(this, inputDeps);
        }

        private class EvadeStateBarrier : BarrierSystem { }
    }
}
