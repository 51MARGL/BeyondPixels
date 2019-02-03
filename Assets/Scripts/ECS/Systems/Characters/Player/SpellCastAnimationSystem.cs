using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.Utilities;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class SpellCastAnimationSystem : ComponentSystem
    {
        private struct SpellStateComponent : IComponentData { }

        private struct AddedData
        {
            public readonly int Length;
            public ComponentArray<Animator> AnimatorComponents;
            public ComponentArray<SpellBookComponent> SpellBookComponents;
            public ComponentDataArray<SpellCastingComponent> SpellCastingComponents;
            public SubtractiveComponent<SpellStateComponent> CompStates;
            public EntityArray EntityArray;
        }
        [Inject]
        private AddedData _added;

        private struct RemovedData
        {
            public readonly int Length;
            public ComponentArray<Animator> AnimatorComponents;
            public ComponentArray<SpellBookComponent> SpellBookComponents;
            public SubtractiveComponent<SpellCastingComponent> SpellCastingComponents;
            public ComponentDataArray<SpellStateComponent> CompStates;
            public EntityArray EntityArray;
        }
        [Inject]
        private RemovedData _removed;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _added.Length; i++)
            {
                var animatorComponent = _added.AnimatorComponents[i];

                animatorComponent.ActivateLayer("CastSpellLayer");
                animatorComponent.SetBool("spellCasting", true);

                PostUpdateCommands.AddComponent(_added.EntityArray[i], new SpellStateComponent());
            }

            for (int i = 0; i < _removed.Length; i++)
            {
                var animatorComponent = _removed.AnimatorComponents[i];
                animatorComponent.ActivateLayer("CastSpellLayer");
                animatorComponent.SetBool("spellCasting", false);

                PostUpdateCommands.RemoveComponent<SpellStateComponent>(_removed.EntityArray[i]);
            }
        }
    }
}
