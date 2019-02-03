using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.Utilities;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    [UpdateAfter(typeof(MovementSystem))]
    public class MovementAnimationSystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public ComponentArray<Animator> AnimatorComponents;
            public ComponentDataArray<MovementComponent> MovementComponents;
            public ComponentArray<Transform> TransformComponents;
            public SubtractiveComponent<AttackComponent> AttackComponents;
            public SubtractiveComponent<AttackStateComponent> AttackStateComponents;
            public SubtractiveComponent<SpellCastingComponent> SpellCastingComponents;
        }
        [Inject]
        private Data _data;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                var movementComponent = _data.MovementComponents[i];
                var animatorComponent = _data.AnimatorComponents[i];
                if (movementComponent.Direction != Vector2.zero)
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
            }
        }
    }

}