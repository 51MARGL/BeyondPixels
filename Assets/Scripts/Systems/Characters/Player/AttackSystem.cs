using BeyondPixels.Components.Characters.Common;
using BeyondPixels.Components.Characters.Player;
using Unity.Entities;

namespace BeyondPixels.Systems.Characters.Player
{
    public class AttackSystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public ComponentDataArray<InputComponent> InputComponents;
            public ComponentDataArray<CharacterComponent> CharacterComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private Data _data;
        [Inject]
        private ComponentDataFromEntity<AttackComponent> _attackComponents;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                var input = _data.InputComponents[i];
                if (input.AttackButtonPressed == 1 && !_attackComponents.Exists(_data.EntityArray[i]))
                    PostUpdateCommands.AddComponent(_data.EntityArray[i],
                        new AttackComponent
                        {
                            CurrentComboIndex = 0
                        });
                else if (input.AttackButtonPressed == 1)
                {
                    var attackComponent = EntityManager.GetComponentData<AttackComponent>(_data.EntityArray[i]);
                    PostUpdateCommands.SetComponent(_data.EntityArray[i],
                        new AttackComponent
                        {
                            CurrentComboIndex = (attackComponent.CurrentComboIndex + 1) % 2 // hard coded number of attacks
                        });
                }
            }
        }
    }
}

