using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Spells;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class SpellValidateSystem : ComponentSystem
    {
        private EntityQuery _playerCastGroup;
        private EntityQuery _activeSpellGroup;

        protected override void OnCreate()
        {
            this._playerCastGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(InputComponent), ComponentType.ReadOnly<CharacterComponent>()
                },
                None = new ComponentType[]
                {
                     ComponentType.ReadOnly<AttackComponent>(),
                     ComponentType.ReadOnly<SpellCastingComponent>()
                }
            });
            this._activeSpellGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    ComponentType.ReadOnly<ActiveSpellComponent>()
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<CoolDownComponent>(),
                    ComponentType.ReadOnly<InstantiateSpellComponent>()
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._playerCastGroup).ForEach((Entity playerEntity, ref InputComponent inputComponent) =>
            {
                if (inputComponent.ActionButtonPressed == 0)
                    return;

                var spellEntities = this._activeSpellGroup.ToEntityArray(Allocator.TempJob);
                var spellComponents = this._activeSpellGroup.ToComponentDataArray<ActiveSpellComponent>(Allocator.TempJob);

                for (var sI = 0; sI < spellEntities.Length; sI++)
                {
                    if (spellComponents[sI].Owner == playerEntity
                        && inputComponent.ActionButtonPressed == spellComponents[sI].ActionIndex)
                    {
                        var spellPrefab = SpellBookManagerComponent.Instance.SpellBook.Spells[spellComponents[sI].SpellIndex];
                        if (spellPrefab.TargetRequired
                            && !this.EntityManager.HasComponent<TargetComponent>(playerEntity))
                        {
                            inputComponent.ActionButtonPressed = 0;
                            break;
                        }

                        this.PostUpdateCommands.AddComponent(playerEntity, new SpellCastingComponent
                        {
                            SpellIndex = spellComponents[sI].SpellIndex,
                            ActiveSpell = spellEntities[sI],
                            StartedAt = Time.time
                        });

                        inputComponent.ActionButtonPressed = 0;
                        break;
                    }
                }
                inputComponent.ActionButtonPressed = 0;
                spellEntities.Dispose();
                spellComponents.Dispose();
            });
        }
    }
}
