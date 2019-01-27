using BeyondPixels.Components.Characters.Player;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.Systems.Characters.Player
{
    [UpdateAfter(typeof(AttackSystem))]
    public class AttackAnimationSystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public ComponentArray<Animator> AnimatorComponents;
            public ComponentArray<PlayerInitializeComponent> PlayerInitComponents;
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

                string attackTriggerName = _data.PlayerInitComponents[i]
                                                .AttackComboParams[attackComponent.CurrentComboIndex];
                int attackLayerIndex = animatorComponent.GetLayerIndex("AttackLayer");
                if (!animatorComponent.GetCurrentAnimatorStateInfo(attackLayerIndex).IsName(attackTriggerName))
                    animatorComponent.SetTrigger(attackTriggerName);

                if (animatorComponent.GetCurrentAnimatorStateInfo(attackLayerIndex).IsTag("Finish"))
                {
                    foreach (var comboName in _data.PlayerInitComponents[i].AttackComboParams)
                        animatorComponent.ResetTrigger(comboName);
                    PostUpdateCommands.RemoveComponent<AttackComponent>(_data.EntityArray[i]);
                }
            }
        }
    }
}
