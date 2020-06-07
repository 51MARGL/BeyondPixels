using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.Utilities;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Spells
{
    [UpdateInGroup(typeof(FixedUpdateSystemGroup))]
    public class ThrowOnTargetSystem : ComponentSystem
    {
        private EntityQuery _group;

        protected override void OnCreate()
        {
            this._group = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
{
                    ComponentType.ReadOnly(typeof(MovementComponent)),
                    typeof(PositionComponent),
                    typeof(ThrowOnTargetComponent),
                    typeof(TargetRequiredComponent),
                    typeof(Transform)
}
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._group).ForEach((Entity entity,
                Transform transform,
                ref PositionComponent positionComponent,
                ref MovementComponent movementComponent,
                ref TargetRequiredComponent targetRequiredComponent) =>
            {
                if (!this.EntityManager.Exists(targetRequiredComponent.Target))
                {
                    this.PostUpdateCommands.AddComponent(entity, new DestroyComponent());
                    return;
                }

                var targetPosition = this.EntityManager.GetComponentData<PositionComponent>(targetRequiredComponent.Target);

                if (math.distance(targetPosition.CurrentPosition, positionComponent.CurrentPosition) > 0.1f)
                {
                    movementComponent.Direction = targetPosition.CurrentPosition - positionComponent.CurrentPosition;
                }
                else
                {
                    movementComponent.Direction = float2.zero;
                }

                transform.right = new Vector3(movementComponent.Direction.x, movementComponent.Direction.y, 0f);
            });
        }
    }
}