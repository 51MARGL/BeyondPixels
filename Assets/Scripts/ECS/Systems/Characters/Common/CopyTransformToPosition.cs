using BeyondPixels.ECS.Components.Characters.Common;

using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    public class CopyTransformToPosition : JobComponentSystem
    {
        [BurstCompile]
        private struct CopyTransformToPositionJob : IJobParallelForTransform
        {
            public ComponentDataArray<PositionComponent> PositionComponents;

            public void Execute(int index, TransformAccess transform)
            {
                var position = this.PositionComponents[index];
                position.CurrentPosition = new float2(transform.position.x, transform.position.y);
                this.PositionComponents[index] = position;
            }
        }

        private ComponentGroup _transformGroup;

        protected override void OnCreateManager()
        {
            this._transformGroup = this.GetComponentGroup(typeof(PositionComponent), typeof(UnityEngine.Transform));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var transformArray = this._transformGroup.GetTransformAccessArray();
            return new CopyTransformToPositionJob
            {
                //no other method for now
                PositionComponents = this._transformGroup.GetComponentDataArray<PositionComponent>()
            }.Schedule(transformArray, inputDeps);
        }
    }
}
