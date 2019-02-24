using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Components.Spells;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace BeyondPixels.ECS.Systems.Spells
{
    public class LockOnTargetSystem : JobComponentSystem
    {
        public struct LockOnTargetJob : IJobParallelForTransform
        {
            [ReadOnly]
            public ComponentDataFromEntity<PositionComponent> PositionComponents;
            [ReadOnly]
            public ComponentDataArray<TargetRequiredComponent> TargetRequiredComponents;

            public void Execute(int index, TransformAccess transform)
            {
                if (!PositionComponents.Exists(TargetRequiredComponents[index].Target))
                    return;

                var position = PositionComponents[TargetRequiredComponents[index].Target].CurrentPosition;
                transform.position = new Vector3(position.x, position.y, 0f);
            }
        }
        [Inject]
        private ComponentDataFromEntity<PositionComponent> _positionComponents;
        private ComponentGroup _group;

        protected override void OnCreateManager()
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
            return new LockOnTargetJob
            {
                TargetRequiredComponents = _group.GetComponentDataArray<TargetRequiredComponent>(),
                PositionComponents = _positionComponents
            }.Schedule(_group.GetTransformAccessArray(), inputDeps);
        }
    }
}