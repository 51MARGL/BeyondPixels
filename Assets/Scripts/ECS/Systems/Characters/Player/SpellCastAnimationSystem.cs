using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.Utilities;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SpellCastAnimationSystem : ComponentSystem
    {
        private struct SpellStateComponent : IComponentData { }

        private ComponentGroup _addedGroup;
        private ComponentGroup _removedGroup;

        protected override void OnCreateManager()
        {
            _addedGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(Animator), typeof(SpellCastingComponent)
                },
                None = new ComponentType[] {
                    typeof(SpellStateComponent)
                }
            });
            _removedGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(Animator), typeof(SpellStateComponent)
                },
                None = new ComponentType[] {
                    typeof(SpellCastingComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(_addedGroup).ForEach((Entity entity, Animator animatorComponent) =>
            {
                animatorComponent.ActivateLayer("CastSpellLayer");
                animatorComponent.SetBool("spellCasting", true);

                PostUpdateCommands.AddComponent(entity, new SpellStateComponent());
            });
            Entities.With(_removedGroup).ForEach((Entity entity, Animator animatorComponent) =>
            {
                animatorComponent.ActivateLayer("CastSpellLayer");
                animatorComponent.SetBool("spellCasting", false);

                PostUpdateCommands.RemoveComponent<SpellStateComponent>(entity);
            });
        }
    }
}
