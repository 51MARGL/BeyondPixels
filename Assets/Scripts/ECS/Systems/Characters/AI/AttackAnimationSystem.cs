using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.Utilities;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class AttackAnimationSystem : ComponentSystem
    {
        private struct AttackStateInitialComponent : IComponentData { }

        private struct AddedData
        {
            public readonly int Length;
            public ComponentArray<Animator> AnimatorComponents;
            public ComponentDataArray<AttackStateComponent> AttackStateComponents;
            public SubtractiveComponent<AttackStateInitialComponent> CompStates;
            public EntityArray EntityArray;
        }
        [Inject]
        private AddedData _added;

        private struct ChangedData
        {
            public readonly int Length;
            public ComponentArray<Animator> AnimatorComponents;
            public ComponentDataArray<AttackStateComponent> AttackStateComponents;
            public ComponentDataArray<AttackStateInitialComponent> CompStates;
            public EntityArray EntityArray;
        }
        [Inject]
        private ChangedData _changed;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _added.Length; i++)
            {
                var animatorComponent = _added.AnimatorComponents[i];
                animatorComponent.ActivateLayer("AttackLayer");
                animatorComponent.SetTrigger("Attack");

                PostUpdateCommands.AddComponent(_added.EntityArray[i], new AttackStateInitialComponent());
            }

            for (int i = 0; i < _changed.Length; i++)
            {
                var animatorComponent = _changed.AnimatorComponents[i];
                int attackLayerIndex = animatorComponent.GetLayerIndex("AttackLayer");
                if (animatorComponent.GetCurrentAnimatorStateInfo(attackLayerIndex).IsTag("Finish"))
                {
                    PostUpdateCommands.RemoveComponent<AttackStateComponent>(_changed.EntityArray[i]);
                    PostUpdateCommands.RemoveComponent<AttackStateInitialComponent>(_changed.EntityArray[i]);
                }
            }
        }
    }
}
