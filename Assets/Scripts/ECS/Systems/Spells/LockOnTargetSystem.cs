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
                var position = float2.zero;
                for (int c = 0; c < Chunks.Length; c++)
                {
                    var chunk = Chunks[c];
                    var entities = chunk.GetNativeArray(EntityType);
                    var positionComponents = chunk.GetNativeArray(PositionComponentType);
                    for (int i = 0; i < chunk.Count; i++)
                        if (entities[i] == TargetRequiredComponents[index].Target)
                            position = positionComponents[i].CurrentPosition;
                }

                if (position.Equals(float2.zero))
                    return;

                transform.position = new Vector3(position.x, position.y, 0f);
            }
        }
        private ComponentGroup _spellGroup;
        private ComponentGroup _targetGroup;

        protected override void OnCreateManager()
        {
            _spellGroup = GetComponentGroup(
                ComponentType.ReadOnly(typeof(SpellComponent)),
                ComponentType.ReadOnly(typeof(LockOnTargetComponent)),
                ComponentType.ReadOnly(typeof(TargetRequiredComponent)),
                ComponentType.Exclude(typeof(DestroyComponent)),
                typeof(UnityEngine.Transform)
            );
            _targetGroup = GetComponentGroup(
                ComponentType.ReadOnly(typeof(PositionComponent)),
                ComponentType.ReadOnly(typeof(CharacterComponent)),
                typeof(UnityEngine.Transform)
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var chunks = _targetGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            return new LockOnTargetJob
            {
                TargetRequiredComponents = _spellGroup.ToComponentDataArray<TargetRequiredComponent>(Allocator.TempJob),
                Chunks = chunks,
                EntityType = GetArchetypeChunkEntityType(),
                PositionComponentType = GetArchetypeChunkComponentType<PositionComponent>()
            }.Schedule(_spellGroup.GetTransformAccessArray(), inputDeps);
        }
    }
}