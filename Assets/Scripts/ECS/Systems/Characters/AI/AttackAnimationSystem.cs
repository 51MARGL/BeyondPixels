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
            _attackStartGroup = GetComponentGroup(new EntityArchetypeQuery
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
            _attackingGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(Animator), typeof(AttackStateComponent), typeof(AttackStateInitialComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(_attackStartGroup).ForEach((Entity entity, Animator animatorComponent) => {
                animatorComponent.ActivateLayer("AttackLayer");
                animatorComponent.SetTrigger("Attack");

                PostUpdateCommands.AddComponent(entity, new AttackStateInitialComponent());
            });
            Entities.With(_attackingGroup).ForEach((Entity entity, Animator animatorComponent) => {
                int attackLayerIndex = animatorComponent.GetLayerIndex("AttackLayer");
                if (animatorComponent.GetCurrentAnimatorStateInfo(attackLayerIndex).IsTag("Finish"))
                {
                    PostUpdateCommands.RemoveComponent<AttackStateComponent>(entity);
                    PostUpdateCommands.RemoveComponent<AttackStateInitialComponent>(entity);
                }
            });
        }
    }
}
