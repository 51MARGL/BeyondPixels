using BeyondPixels.ECS.Components.Characters.Common;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    [UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.FixedUpdate))]
    public class CopyTranfromToPosition : JobComponentSystem
    {
        [BurstCompile]
        private struct CopyTransformToPositionJob : IJobParallelForTransform
        {
            public ComponentDataArray<PositionComponent> PositionComponents;

            public void Execute(int index, TransformAccess transform)
            {
                var position = PositionComponents[index];
                position.CurrentPosition = new float2(transform.position.x, transform.position.y);
                PositionComponents[index] = position;
            }
        }

        private ComponentGroup _transformGroup;

        protected override void OnCreateManager()
        {
            _transformGroup = GetComponentGroup(typeof(PositionComponent), typeof(UnityEngine.Transform));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var transformArray = _transformGroup.GetTransformAccessArray();
            return new CopyTransformToPositionJob
            {
                PositionComponents = _transformGroup.GetComponentDataArray<PositionComponent>()
            }.Schedule(transformArray, inputDeps);
        }
    }
}
