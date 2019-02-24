using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.AI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class AggroEventSystem : JobComponentSystem
    {
        [DisableAutoCreation]
        public class AggroEventBarrier : BarrierSystem { }

        private struct AggroEventJob : IJobProcessComponentDataWithEntity<CollisionInfo, AggroRangeCollisionComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref CollisionInfo collisionInfo,
                                [ReadOnly] ref AggroRangeCollisionComponent aggroRangeCollisionComponent)
            {
                switch (collisionInfo.EventType)
                {
                    case EventType.TriggerEnter:
                        CommandBuffer.AddComponent(index, collisionInfo.Sender,
                            new FollowStateComponent
                            {
                                Target = collisionInfo.Other
                            });
                        break;
                    case EventType.TriggerExit:
                        CommandBuffer.RemoveComponent<FollowStateComponent>(index, collisionInfo.Sender);
                        break;
                }
                CommandBuffer.DestroyEntity(index, entity);
            }
        }

        private AggroEventBarrier _aggroEventBarrier;

        protected override void OnCreateManager()
        {
            _aggroEventBarrier = World.Active.GetOrCreateManager<AggroEventBarrier>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new AggroEventJob
            {
                CommandBuffer = _aggroEventBarrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, inputDeps);
            _aggroEventBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
