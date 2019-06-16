using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Components.Spells;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine;
using UnityEngine.Jobs;

namespace BeyondPixels.ECS.Systems.Spells
{
    public class LockOnTargetSystem : JobComponentSystem
    {
        [BurstCompile]
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
                for (var c = 0; c < this.Chunks.Length; c++)
                {
                    var chunk = this.Chunks[c];
                    var entities = chunk.GetNativeArray(this.EntityType);
                    var positionComponents = chunk.GetNativeArray(this.PositionComponentType);
                    for (var i = 0; i < chunk.Count; i++)
                        if (entities[i] == this.TargetRequiredComponents[index].Target)
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
            this._spellGroup = this.GetComponentGroup(new EntityArchetypeQuery
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
            this._targetGroup = this.GetComponentGroup(new EntityArchetypeQuery
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
                TargetRequiredComponents = this._spellGroup.ToComponentDataArray<TargetRequiredComponent>(Allocator.TempJob),
                Chunks = this._targetGroup.CreateArchetypeChunkArray(Allocator.TempJob),
                EntityType = this.GetArchetypeChunkEntityType(),
                PositionComponentType = this.GetArchetypeChunkComponentType<PositionComponent>()
            }.Schedule(this._spellGroup.GetTransformAccessArray(), inputDeps);
        }
    }
}