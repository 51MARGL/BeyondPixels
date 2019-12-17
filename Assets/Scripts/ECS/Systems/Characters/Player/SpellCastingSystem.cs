using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class SpellCastingSystem : ComponentSystem
    {
        private EntityQuery _playerCastingGroup;
        private EntityQuery _activeSpellGroup;

        protected override void OnCreate()
        {
            this._playerCastingGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(InputComponent),
                    ComponentType.ReadOnly<CharacterComponent>(),
                    ComponentType.ReadOnly<SpellCastingComponent>()
                },
                None = new ComponentType[]
                {
                     ComponentType.ReadOnly<AttackComponent>()
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._playerCastingGroup).ForEach((Entity playerEntity, ref InputComponent inputComponent) =>
            {
                if (inputComponent.AttackButtonPressed == 1
                    || !inputComponent.InputDirection.Equals(float2.zero))
                {
                    this.PostUpdateCommands.RemoveComponent<SpellCastingComponent>(playerEntity);
                    return;
                }
            });
        }
    }
}