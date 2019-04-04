using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
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
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> TargetChunks;
            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;
            [ReadOnly]
            public ArchetypeChunkComponentType<FollowStateComponent> FollowStateComponentType;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref CollisionInfo collisionInfo,
                                [ReadOnly] ref AggroRangeCollisionComponent aggroRangeCollisionComponent)
            {
                for (int c = 0; c < TargetChunks.Length; c++)
                {
                    var chunk = TargetChunks[c];
                    var entities = chunk.GetNativeArray(EntityType);
                    for (int i = 0; i < chunk.Count; i++)
                        if (entities[i] == collisionInfo.Sender)
                        {
                            switch (collisionInfo.EventType)
                            {
                                case EventType.TriggerEnter:
                                    if (!chunk.Has(FollowStateComponentType))
                                        CommandBuffer.AddComponent(index, collisionInfo.Sender,
                                            new FollowStateComponent
                                            {
                                                Target = collisionInfo.Target
                                            });
                                    break;
                                case EventType.TriggerExit:
                                    if (chunk.Has(FollowStateComponentType))
                                        CommandBuffer.RemoveComponent<FollowStateComponent>(index, collisionInfo.Sender);
                                    break;
                            }
                            CommandBuffer.DestroyEntity(index, entity);
                            return;
                        }
                }
                CommandBuffer.DestroyEntity(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        private ComponentGroup _targetGroup;

        protected override void OnCreateManager()
        {
            _endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            _targetGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(CharacterComponent), typeof(PositionComponent)
                },
                None = new ComponentType[]
                {
                    typeof(PlayerComponent)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new AggroEventJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                TargetChunks = _targetGroup.CreateArchetypeChunkArray(Allocator.TempJob),
                EntityType = GetArchetypeChunkEntityType(),
                FollowStateComponentType = GetArchetypeChunkComponentType<FollowStateComponent>()
            }.Schedule(this, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
