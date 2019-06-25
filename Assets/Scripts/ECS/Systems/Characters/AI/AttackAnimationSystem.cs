using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.Utilities;

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class AttackAnimationSystem : ComponentSystem
    {
        private struct AttackStateInitialComponent : IComponentData { }

        private EntityQuery _attackStartGroup;
        private EntityQuery _attackingGroup;

        protected override void OnCreateManager()
        {
            this._attackStartGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(Animator), typeof(AttackStateComponent), typeof(MovementComponent)
                },
                None = new ComponentType[]
                {
                    typeof(AttackStateInitialComponent)
                }
            });
            this._attackingGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(Animator), typeof(AttackStateComponent), typeof(AttackStateInitialComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._attackStartGroup).ForEach((Entity entity, Animator animatorComponent, ref MovementComponent movementComponent) =>
            {
                
                animatorComponent.ActivateLayer("AttackLayer");
                animatorComponent.SetTrigger("Attack");

                var movement = movementComponent;
                movement.Direction = float2.zero;
                this.PostUpdateCommands.AddComponent(entity, new AttackStateInitialComponent());
                this.PostUpdateCommands.SetComponent(entity, movement);
            });
            this.Entities.With(this._attackingGroup).ForEach((Entity entity, Animator animatorComponent) =>
            {
                var attackLayerIndex = animatorComponent.GetLayerIndex("AttackLayer");
                if (animatorComponent.GetCurrentAnimatorStateInfo(attackLayerIndex).IsTag("Finish"))
                {
                    this.PostUpdateCommands.RemoveComponent<AttackStateComponent>(entity);
                    this.PostUpdateCommands.RemoveComponent<AttackStateInitialComponent>(entity);
                }
            });
        }
    }
}
