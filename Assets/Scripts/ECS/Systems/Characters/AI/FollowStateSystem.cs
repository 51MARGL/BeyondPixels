using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Objects;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class FollowStateSystem : JobComponentSystem
    {
        [ExcludeComponent(typeof(AttackStateComponent))]
        private struct FollowStateJob :
            IJobProcessComponentDataWithEntity<MovementComponent, FollowStateComponent, WeaponComponent, PositionComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> TargetChunks;
            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;
            [ReadOnly]
            public ArchetypeChunkComponentType<PositionComponent> PositionComponentType;
            [ReadOnly]
            public float CurrentTime;

            public void Execute(Entity entity,
                                int index,
                                ref MovementComponent movementComponent,
                                ref FollowStateComponent followStateComponent,
                                [ReadOnly] ref WeaponComponent weaponComponent,
                                [ReadOnly] ref PositionComponent positionComponent)
            {
                if (TargetChunks.Length == 0)
                {
                    CommandBuffer.RemoveComponent<FollowStateComponent>(index, entity);
                    return;
                }

                for (int c = 0; c < TargetChunks.Length; c++)
                {
                    var chunk = TargetChunks[c];
                    var entities = chunk.GetNativeArray(EntityType);
                    var positionComponents = chunk.GetNativeArray(PositionComponentType);
                    for (int i = 0; i < chunk.Count; i++)
                        if (entities[i] == followStateComponent.Target)
                        {
                            var targetPosition = positionComponents[i];

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
                                        Target = followStateComponent.Target
                                    });
                            }
                            return;
                        }
                }
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
                    typeof(PositionComponent), typeof(PlayerComponent)
                },
                None = new ComponentType[]
                {
                    typeof(DestroyComponent)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new FollowStateJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                TargetChunks = _targetGroup.CreateArchetypeChunkArray(Allocator.TempJob),
                EntityType = GetArchetypeChunkEntityType(),
                PositionComponentType = GetArchetypeChunkComponentType<PositionComponent>(),
                CurrentTime = Time.time
            }.Schedule(this, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
