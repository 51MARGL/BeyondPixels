using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Components.Spells;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace BeyondPixels.ECS.Systems.Spells
{
    public class LockOnTargetSystem : JobComponentSystem
    {
        public struct LockOnTargetJob : IJobParallelForTransform
        {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<TargetRequiredComponent> TargetRequiredComponents;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;
            [ReadOnly]
            public ArchetypeChunkComponentType<PositionComponent> PositionComponentType;

            public void Execute(int index, TransformAccess transform)
            {
                for (int c = 0; c < Chunks.Length; c++)
                {
                    var chunk = Chunks[c];
                    var entities = chunk.GetNativeArray(EntityType);
                    var positionComponents = chunk.GetNativeArray(PositionComponentType);
                    for (int i = 0; i < chunk.Count; i++)
                        if (entities[i] == TargetRequiredComponents[index].Target)
                        {
                            transform.position =
                                new Vector3(positionComponents[i].CurrentPosition.x, 
                                            positionComponents[i].CurrentPosition.y, 0f);
                        }
                }
            }
        }
        private ComponentGroup _spellGroup;
        private ComponentGroup _targetGroup;

        protected override void OnCreateManager()
        {
            _spellGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(SpellComponent), typeof(LockOnTargetComponent),
                    typeof(TargetRequiredComponent), typeof(Transform)
                },
                None = new ComponentType[]
                {
                    typeof(DestroyComponent)
                }
            });
            _targetGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(CharacterComponent), typeof(PositionComponent)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new LockOnTargetJob
            {
                TargetRequiredComponents = _spellGroup.ToComponentDataArray<TargetRequiredComponent>(Allocator.TempJob),
                Chunks = _targetGroup.CreateArchetypeChunkArray(Allocator.TempJob),
                EntityType = GetArchetypeChunkEntityType(),
                PositionComponentType = GetArchetypeChunkComponentType<PositionComponent>()
            }.Schedule(_spellGroup.GetTransformAccessArray(), inputDeps);
        }
    }
}