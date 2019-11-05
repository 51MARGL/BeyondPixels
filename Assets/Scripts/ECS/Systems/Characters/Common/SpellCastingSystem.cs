using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Spells;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Common
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
                    ComponentType.ReadOnly<MagicStatComponent>(),
                    ComponentType.ReadOnly<CharacterComponent>(),
                    ComponentType.ReadOnly<SpellCastingComponent>()
                },
                Any = new ComponentType[] {
                    ComponentType.ReadOnly<FollowStateComponent>(),
                    ComponentType.ReadOnly<TargetComponent>()
                },
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
            this.Entities.With(this._playerCastingGroup).ForEach((Entity entity,
                ref SpellCastingComponent spellCastingComponent,
                ref MagicStatComponent magicStatComponent) =>
            {
                using (var spellEntities = this._activeSpellGroup.ToEntityArray(Allocator.TempJob))
                using (var spellComponents = this._activeSpellGroup.ToComponentDataArray<ActiveSpellComponent>(Allocator.TempJob))
                {
                    for (var sI = 0; sI < spellEntities.Length; sI++)
                    {
                        if (spellEntities[sI] == spellCastingComponent.ActiveSpell)
                        {
                            var spellPrefab = SpellBookManagerComponent.Instance.SpellBook.Spells[spellComponents[sI].SpellIndex];
                            if (spellPrefab.TargetRequired
                                && !this.EntityManager.HasComponent<TargetComponent>(entity)
                                && !this.EntityManager.HasComponent<FollowStateComponent>(entity))
                            {
                                this.PostUpdateCommands.RemoveComponent<SpellCastingComponent>(entity);
                                break;
                            }

                            var castTime = math.max(0.8f, spellPrefab.CastTime -
                                    (spellPrefab.CastTime / 500f * magicStatComponent.CurrentValue));

                            if (spellCastingComponent.StartedAt + castTime > Time.time)
                                break;

                            var target = Entity.Null;
                            if (spellPrefab.SelfTarget)
                            {
                                target = entity;
                            }
                            else if (spellPrefab.TargetRequired)
                            {
                                if (this.EntityManager.HasComponent<FollowStateComponent>(entity))
                                {
                                    target = this.EntityManager.GetComponentData<FollowStateComponent>(entity).Target;
                                }
                                else if (this.EntityManager.HasComponent<TargetComponent>(entity))
                                {
                                    target = this.EntityManager.GetComponentData<TargetComponent>(entity).Target;
                                }
                            }

                            if (target != Entity.Null && !this.EntityManager.Exists(target))
                            {
                                this.PostUpdateCommands.RemoveComponent<SpellCastingComponent>(entity);
                                break;
                            }

                            var coolDown = math.max(3f, spellPrefab.CoolDown -
                                            (spellPrefab.CoolDown / 500f * magicStatComponent.CurrentValue));

                            this.PostUpdateCommands.RemoveComponent<SpellCastingComponent>(entity);
                            this.PostUpdateCommands.AddComponent(spellEntities[sI], new CoolDownComponent
                            {
                                CoolDownTime = coolDown
                            });
                            this.PostUpdateCommands.AddComponent(spellEntities[sI], new InstantiateSpellComponent
                            {
                                Caster = entity,
                                Target = target
                            });
                            break;
                        }
                    }
                }
            });
        }
    }
}