using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.Utilities;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    [UpdateInGroup(typeof(FixedUpdateSystemGroup))]
    public class MovementSystem : ComponentSystem
    {
        private EntityQuery _group;

        protected override void OnCreate()
        {
            this._group = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly(typeof(MovementComponent)),
                    typeof(UnityEngine.Transform),
                    typeof(UnityEngine.Rigidbody2D)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._group).ForEach((Entity entity, ref MovementComponent movementComponent, Rigidbody2D rigidbody, Transform transform) =>
            {
                var velocity = new float2();

                if (!this.EntityManager.HasComponent<AttackComponent>(entity)
                    && !movementComponent.Direction.Equals(float2.zero))
                {
                    velocity = math.normalize(movementComponent.Direction) *
                        movementComponent.Speed;

                    var scale = math.abs(transform.localScale.x);
                    if (velocity.x < 0f)
                        transform.localScale = new Vector3(-scale, transform.localScale.y, transform.localScale.z);
                    else if (velocity.x > 0f)
                        transform.localScale = new Vector3(scale, transform.localScale.y, transform.localScale.z);
                }

                rigidbody.velocity = velocity;
            });
        }
    }
}