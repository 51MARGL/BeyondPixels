using BeyondPixels.Components.Characters.AI;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.Systems.Characters.AI
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

        private struct RemovedData
        {
            public readonly int Length;
            public SubtractiveComponent<AttackStateComponent> AttackStateComponents;
            public ComponentDataArray<AttackStateInitialComponent> CompStates;
            public EntityArray EntityArray;
        }
        [Inject]
        private RemovedData _removed;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _added.Length; i++)
            {
                var animatorComponent = _added.AnimatorComponents[i];
                animatorComponent.ActivateLayer("AttackLayer");
                animatorComponent.SetTrigger("Attack");

                PostUpdateCommands.AddComponent(_added.EntityArray[i], new AttackStateInitialComponent());
            }

            for (int i = 0; i < _removed.Length; i++)
                PostUpdateCommands.RemoveComponent<AttackStateInitialComponent>(_removed.EntityArray[i]);            
        }
    }
}
