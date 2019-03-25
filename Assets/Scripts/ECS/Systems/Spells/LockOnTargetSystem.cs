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
        private NativeArray<ComponentType> _spellComponentArray;
        private NativeArray<ComponentType> _targetComponentArray;

        protected override void OnCreateManager()
        {
            _spellComponentArray = new NativeArray<ComponentType>(5, Allocator.Persistent);
            _spellComponentArray[0] = typeof(SpellComponent);
            _spellComponentArray[1] = typeof(LockOnTargetComponent);
            _spellComponentArray[2] = typeof(TargetRequiredComponent);
            _spellComponentArray[3] = typeof(UnityEngine.Transform);
            _spellComponentArray[4] = ComponentType.Exclude(typeof(DestroyComponent));

            _spellGroup = GetComponentGroup(_spellComponentArray);

            _targetComponentArray = new NativeArray<ComponentType>(2, Allocator.Persistent);
            _targetComponentArray[0] = typeof(PositionComponent);
            _targetComponentArray[1] = typeof(CharacterComponent);

            _targetGroup = GetComponentGroup(_targetComponentArray);
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

        protected override void OnDestroyManager()
        {
            _spellComponentArray.Dispose();
            _targetComponentArray.Dispose();
        }
    }
}