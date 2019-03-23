//using BeyondPixels.ECS.Components.Characters.AI;
//using BeyondPixels.ECS.Components.Characters.Common;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using UnityEngine;

//namespace BeyondPixels.ECS.Systems.Characters.AI
//{
//    [UpdateBefore(typeof(FollowStateSystem))]
//    public class EvadeStateSystem : JobComponentSystem
//    {
//        [DisableAutoCreation]
//        private class EvadeStateBarrier : EntityCommandBufferSystem { }

//        [ExcludeComponent(typeof(AttackStateComponent), typeof(FollowStateComponent))]
//        private struct EvadeStateJob :
//            IJobProcessComponentDataWithEntity<MovementComponent, PositionComponent, EvadeStateComponent>
//        {
//            public EntityCommandBuffer.Concurrent CommandBuffer;
//            public float CurrentTime;

//            public void Execute(Entity entity,
//                                int index,
//                                ref MovementComponent movementComponent,
//                                [ReadOnly] ref PositionComponent positionComponent,
//                                [ReadOnly] ref EvadeStateComponent evadeComponent)
//            {
//                //If the distance is larger than trashold then keep moving                
//                if (math.distance(positionComponent.CurrentPosition, positionComponent.InitialPosition) > 1f)
//                    movementComponent.Direction =
//                        positionComponent.InitialPosition - positionComponent.CurrentPosition;
//                else
//                {
//                    movementComponent.Direction = float2.zero;

//                    CommandBuffer.RemoveComponent(index, entity, typeof(EvadeStateComponent));
//                    CommandBuffer.AddComponent(index, entity,
//                        new IdleStateComponent
//                        {
//                            StartedAt = CurrentTime
//                        });
//                }
//            }
//        }

//        private EvadeStateBarrier _evadeStateBarrier;

//        protected override void OnCreateManager()
//        {
//            _evadeStateBarrier = World.Active.GetOrCreateManager<EvadeStateBarrier>();
//        }

//        protected override JobHandle OnUpdate(JobHandle inputDeps)
//        {
//            var handle = new EvadeStateJob
//            {
//                CommandBuffer = _evadeStateBarrier.CreateCommandBuffer().ToConcurrent(),
//                CurrentTime = Time.time
//            }.Schedule(this, inputDeps);
//            _evadeStateBarrier.AddJobHandleForProducer(handle);
//            return handle;
//        }
//    }
//}
