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
                for (var c = 0; c < this.TargetChunks.Length; c++)
                {
                    var chunk = this.TargetChunks[c];
                    var entities = chunk.GetNativeArray(this.EntityType);
                    for (var i = 0; i < chunk.Count; i++)
                        if (entities[i] == collisionInfo.Sender)
                        {
                            switch (collisionInfo.EventType)
                            {
                                case EventType.TriggerEnter:
                                    if (!chunk.Has(this.FollowStateComponentType))
                                        this.CommandBuffer.AddComponent(index, collisionInfo.Sender,
                                            new FollowStateComponent
                                            {
                                                Target = collisionInfo.Target
                                            });
                                    break;
                                case EventType.TriggerExit:
                                    if (chunk.Has(this.FollowStateComponentType))
                                        this.CommandBuffer.RemoveComponent<FollowStateComponent>(index, collisionInfo.Sender);
                                    break;
                            }
                            this.CommandBuffer.DestroyEntity(index, entity);
                            return;
                        }
                }
                this.CommandBuffer.DestroyEntity(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        private ComponentGroup _targetGroup;

        protected override void OnCreateManager()
        {
            this._endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            this._targetGroup = this.GetComponentGroup(new EntityArchetypeQuery
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
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                TargetChunks = this._targetGroup.CreateArchetypeChunkArray(Allocator.TempJob),
                EntityType = this.GetArchetypeChunkEntityType(),
                FollowStateComponentType = this.GetArchetypeChunkComponentType<FollowStateComponent>()
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
