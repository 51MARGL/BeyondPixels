using BeyondPixels.Components.Characters.Common;
using BeyondPixels.Components.Objects;
using BeyondPixels.Components.Spells;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace BeyondPixels.Systems.Spells
{
    public class LockOnTargetSystem : JobComponentSystem
    {
        public struct MovementJob : IJobParallelForTransform
        {
            [ReadOnly]
            public ComponentDataFromEntity<PositionComponent> PositionComponents;
            [ReadOnly]
            public ComponentDataArray<TargetRequiredComponent> TargetRequiredComponents;

            public void Execute(int index, TransformAccess transform)
            {
                var position = PositionComponents[TargetRequiredComponents[index].Target].CurrentPosition;
                transform.position = new Vector3(position.x, position.y, 0f);
            }
        }
        [Inject]
        private ComponentDataFromEntity<PositionComponent> _positionComponents;
        private ComponentGroup _group;

        protected override void OnCreateManager(int capacity)
        {
            _group = GetComponentGroup(
                ComponentType.ReadOnly(typeof(SpellComponent)),
                ComponentType.ReadOnly(typeof(LockOnTargetComponent)),
                ComponentType.ReadOnly(typeof(TargetRequiredComponent)),
                ComponentType.Subtractive(typeof(DestroyComponent)),
                typeof(UnityEngine.Transform)
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new MovementJob
            {
                TargetRequiredComponents = _group.GetComponentDataArray<TargetRequiredComponent>(),
                PositionComponents = _positionComponents
            }.Schedule(_group.GetTransformAccessArray(), inputDeps);
        }
    }
}