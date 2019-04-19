using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.Utilities;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class AttackAnimationSystem : ComponentSystem
    {
        private struct AttackStateInitialComponent : IComponentData { }

        private ComponentGroup _attackStartGroup;
        private ComponentGroup _attackingGroup;

        protected override void OnCreateManager()
        {
            this._attackStartGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(Animator), typeof(AttackStateComponent)
                },
                None = new ComponentType[]
                {
                    typeof(AttackStateInitialComponent)
                }
            });
            this._attackingGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(Animator), typeof(AttackStateComponent), typeof(AttackStateInitialComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._attackStartGroup).ForEach((Entity entity, Animator animatorComponent) =>
            {
                animatorComponent.ActivateLayer("AttackLayer");
                animatorComponent.SetTrigger("Attack");

                this.PostUpdateCommands.AddComponent(entity, new AttackStateInitialComponent());
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
