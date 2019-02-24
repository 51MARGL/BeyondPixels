using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.Utilities;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    [UpdateAfter(typeof(AttackSystem))]
    public class AttackAnimationSystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public ComponentArray<Animator> AnimatorComponents;
            public ComponentDataArray<CharacterComponent> CharacterComponents;
            public ComponentDataArray<AttackComponent> AttackComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private Data _data;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                var attackComponent = _data.AttackComponents[i];
                var animatorComponent = _data.AnimatorComponents[i];

                animatorComponent.ActivateLayer("AttackLayer");

                string attackTriggerName = "Attack" + (attackComponent.CurrentComboIndex + 1);
                int attackLayerIndex = animatorComponent.GetLayerIndex("AttackLayer");
                if (!animatorComponent.GetCurrentAnimatorStateInfo(attackLayerIndex).IsName(attackTriggerName))
                    animatorComponent.SetTrigger(attackTriggerName);

                if (animatorComponent.GetCurrentAnimatorStateInfo(attackLayerIndex).IsTag("Finish"))
                {
                    foreach (var comboName in new[] { "Attack1", "Attack2" })
                        animatorComponent.ResetTrigger(comboName);
                    PostUpdateCommands.RemoveComponent<AttackComponent>(_data.EntityArray[i]);
                }
            }
        }
    }
}
