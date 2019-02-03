using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.AI;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class AggroEventSystem : JobComponentSystem
    {
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
        [Inject]
        private AggroEventBarrier _aggroEventBarrier;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new AggroEventJob
            {
                CommandBuffer = _aggroEventBarrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, inputDeps);
        }

        public class AggroEventBarrier : BarrierSystem { }
    }
}
