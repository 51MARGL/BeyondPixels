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

        private EntityQuery _addedGroup;
        private EntityQuery _removedGroup;

        protected override void OnCreateManager()
        {
            this._addedGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(Animator), typeof(SpellCastingComponent)
                },
                None = new ComponentType[] {
                    typeof(SpellStateComponent)
                }
            });
            this._removedGroup = this.GetEntityQuery(new EntityQueryDesc
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
            this.Entities.With(this._addedGroup).ForEach((Entity entity, Animator animatorComponent) =>
            {
                animatorComponent.ActivateLayer("CastSpellLayer");
                animatorComponent.SetBool("spellCasting", true);

                this.PostUpdateCommands.AddComponent(entity, new SpellStateComponent());
            });
            this.Entities.With(this._removedGroup).ForEach((Entity entity, Animator animatorComponent) =>
            {
                animatorComponent.ActivateLayer("CastSpellLayer");
                animatorComponent.SetBool("spellCasting", false);

                this.PostUpdateCommands.RemoveComponent<SpellStateComponent>(entity);
            });
        }
    }
}
