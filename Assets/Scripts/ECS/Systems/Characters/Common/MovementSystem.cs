using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    [UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.FixedUpdate))]
    public class MovementSystem : JobComponentSystem
    {
        public struct MovementJob : IJobParallelForTransform
        {
            [ReadOnly]
            public ComponentDataArray<MovementComponent> MovementComponents;
            public float DeltaTime;

            public void Execute(int index, TransformAccess transform)
            {
                if (MovementComponents[index].Direction.Equals(float2.zero))
                    return;

                var velocity =
                    math.normalize(MovementComponents[index].Direction) *
                    MovementComponents[index].Speed *
                    DeltaTime;

                transform.position += new Vector3(velocity.x, velocity.y, 0f);

                var scale = math.abs(transform.localScale.x);
                if (velocity.x < 0f)
                    transform.localScale = new Vector3(-scale, transform.localScale.y, transform.localScale.z);
                else if (velocity.x > 0f)
                    transform.localScale = new Vector3(scale, transform.localScale.y, transform.localScale.z);
            }
        }

        private ComponentGroup _group;

        protected override void OnCreateManager()
        {
            _group = GetComponentGroup(
                ComponentType.Subtractive(typeof(AttackComponent)),
                ComponentType.ReadOnly(typeof(MovementComponent)),
                typeof(UnityEngine.Transform)
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var transformArray = _group.GetTransformAccessArray();
            var deltaTime = Time.deltaTime;
            return new MovementJob
            {
                MovementComponents = _group.GetComponentDataArray<MovementComponent>(),
                DeltaTime = deltaTime
            }.Schedule(transformArray, inputDeps);
        }
    }
}