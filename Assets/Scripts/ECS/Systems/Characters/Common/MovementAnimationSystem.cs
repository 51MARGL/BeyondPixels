using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.Utilities;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class MovementAnimationSystem : ComponentSystem
    {
        private ComponentGroup _group;

        protected override void OnCreateManager()
        {
            this._group = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(Animator), typeof(MovementComponent), typeof(Transform)
                },
                None = new ComponentType[] {
                    typeof(AttackComponent), typeof(AttackStateComponent), typeof(SpellCastingComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._group).ForEach((Animator animatorComponent, ref MovementComponent movementComponent) =>
            {
                if (!movementComponent.Direction.Equals(float2.zero))
                {
                    animatorComponent.ActivateLayer("RunLayer");

                    //Sets the animation parameter so that model faces the correct direction
                    animatorComponent.SetFloat("velocity.x", movementComponent.Direction.x);
                    animatorComponent.SetFloat("velocity.y", movementComponent.Direction.y);
                }
                else
                {
                    animatorComponent.ActivateLayer("IdleLayer");
                }
            });
        }
    }
}