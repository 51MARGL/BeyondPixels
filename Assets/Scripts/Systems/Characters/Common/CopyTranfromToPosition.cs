﻿using BeyondPixels.Components.Characters.Common;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace BeyondPixels.Systems.Characters.Common
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
                position.CurrentPosition = transform.position;
                PositionComponents[index] = position;
            }
        }

        private ComponentGroup _transformGroup;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var transformArray = _transformGroup.GetTransformAccessArray();
            return new CopyTransformToPositionJob
            {
                PositionComponents = _transformGroup.GetComponentDataArray<PositionComponent>()
            }.Schedule(transformArray, inputDeps);
        }

        protected override void OnCreateManager(int capacity)
        {
            _transformGroup = GetComponentGroup(typeof(PositionComponent), typeof(UnityEngine.Transform));
        }
    }
}
