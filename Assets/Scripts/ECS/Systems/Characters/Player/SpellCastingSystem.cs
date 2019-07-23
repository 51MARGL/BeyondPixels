using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Spells;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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
                    ComponentType.ReadOnly<MagicStatComponent>(),
                    ComponentType.ReadOnly<CharacterComponent>(),
                    ComponentType.ReadOnly<SpellCastingComponent>()
                },
                None = new ComponentType[]
                {
                     ComponentType.ReadOnly<AttackComponent>()
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
            this.Entities.With(this._playerCastingGroup).ForEach((Entity playerEntity, ref InputComponent inputComponent,
                    ref SpellCastingComponent spellCastingComponent, ref MagicStatComponent magicStatComponent) =>
            {
                if (inputComponent.AttackButtonPressed == 1
                    || !inputComponent.InputDirection.Equals(float2.zero))
                {
                    this.PostUpdateCommands.RemoveComponent<SpellCastingComponent>(playerEntity);
                    return;
                }

                var spellEntities = this._activeSpellGroup.ToEntityArray(Allocator.TempJob);
                var spellComponents = this._activeSpellGroup.ToComponentDataArray<ActiveSpellComponent>(Allocator.TempJob);

                for (var sI = 0; sI < spellEntities.Length; sI++)
                {
                    if (spellEntities[sI] == spellCastingComponent.ActiveSpell)
                    {
                        var spellPrefab = SpellBookManagerComponent.Instance.SpellBook.Spells[spellComponents[sI].SpellIndex];
                        if (spellPrefab.TargetRequired
                            && !this.EntityManager.HasComponent<TargetComponent>(playerEntity))
                        {
                            this.PostUpdateCommands.RemoveComponent<SpellCastingComponent>(playerEntity);
                            break;
                        }

                        var castTime = math.max(0.8f, spellPrefab.CastTime -
                                (spellPrefab.CastTime / 500f * magicStatComponent.CurrentValue));

                        if (spellCastingComponent.StartedAt + castTime > Time.time)
                            break;

                        var target = Entity.Null;
                        if (spellPrefab.SelfTarget)
                            target = playerEntity;
                        else if (spellPrefab.TargetRequired)
                            target = this.EntityManager.GetComponentData<TargetComponent>(playerEntity).Target;

                        if (target != Entity.Null && !this.EntityManager.Exists(target))
                        {
                            this.PostUpdateCommands.RemoveComponent<SpellCastingComponent>(playerEntity);
                            break;
                        }

                        var coolDown = math.max(3f, spellPrefab.CoolDown -
                                        (spellPrefab.CoolDown / 500f * magicStatComponent.CurrentValue));

                        this.PostUpdateCommands.RemoveComponent<SpellCastingComponent>(playerEntity);
                        this.PostUpdateCommands.AddComponent(spellEntities[sI], new CoolDownComponent
                        {
                            CoolDownTime = coolDown
                        });
                        this.PostUpdateCommands.AddComponent(spellEntities[sI], new InstantiateSpellComponent
                        {
                            Caster = playerEntity,
                            Target = target
                        });
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