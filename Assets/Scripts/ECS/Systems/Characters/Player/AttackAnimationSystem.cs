using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.Utilities;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class AttackAnimationSystem : ComponentSystem
    {
        private EntityQuery _group;

        protected override void OnCreate()
        {
            this._group = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(Animator), typeof(CharacterComponent), typeof(AttackComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._group).ForEach((Entity entity, Animator animatorComponent, ref AttackComponent attackComponent) =>
           {
               animatorComponent.ActivateLayer("AttackLayer");

               var attackTriggerName = "Attack" + (attackComponent.CurrentComboIndex + 1);
               var attackLayerIndex = animatorComponent.GetLayerIndex("AttackLayer");
               if (!animatorComponent.GetCurrentAnimatorStateInfo(attackLayerIndex).IsName(attackTriggerName))
               {
                   animatorComponent.SetTrigger(attackTriggerName);
               }

               if (animatorComponent.GetCurrentAnimatorStateInfo(attackLayerIndex).IsTag("Finish"))
               {
                   foreach (var comboName in new[] { "Attack1", "Attack2" })
                   {
                       animatorComponent.ResetTrigger(comboName);
                   }

                   this.PostUpdateCommands.RemoveComponent<AttackComponent>(entity);
               }

           });
        }
    }
}
