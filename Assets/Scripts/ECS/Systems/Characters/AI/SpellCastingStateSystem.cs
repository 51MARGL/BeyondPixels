using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Spells;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.AI;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class SpellCastingStateSystem : ComponentSystem
    {
        private EntityQuery _castingGroup;
        private EntityQuery _activeSpellGroup;

        protected override void OnCreate()
        {
            this._castingGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PositionComponent), typeof(FollowStateComponent), typeof(SpellCastingComponent),
                    typeof(MagicStatComponent)
                },
                None = new ComponentType[]
                {
                    typeof(AttackStateComponent)
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
            this.Entities.With(this._castingGroup).ForEach((Entity entity,
                                                ref PositionComponent positionComponent,
                                                ref FollowStateComponent followStateComponent,
                                                ref SpellCastingComponent spellCastingComponent,
                                                ref MagicStatComponent magicStatComponent) =>
            {
                if (!this.EntityManager.Exists(followStateComponent.Target)
                    || this.EntityManager.HasComponent<InCutsceneComponent>(followStateComponent.Target))
                {
                    this.PostUpdateCommands.RemoveComponent<SpellCastingComponent>(entity);
                    return;
                }

                var currentPosition = positionComponent.CurrentPosition;
                var targetPosition = this.EntityManager.GetComponentData<PositionComponent>(followStateComponent.Target).CurrentPosition;

                using (var spellEntities = this._activeSpellGroup.ToEntityArray(Allocator.TempJob))
                using (var spellComponents = this._activeSpellGroup.ToComponentDataArray<ActiveSpellComponent>(Allocator.TempJob))
                {
                    for (var sI = 0; sI < spellEntities.Length; sI++)
                    {
                        if (spellEntities[sI] == spellCastingComponent.ActiveSpell)
                        {
                            var spellPrefab = SpellBookManagerComponent.Instance.SpellBook.Spells[spellComponents[sI].SpellIndex];

                            var castTime = math.max(0.8f, spellPrefab.CastTime -
                                    (spellPrefab.CastTime / 500f * magicStatComponent.CurrentValue));

                            if (spellCastingComponent.StartedAt + castTime > Time.time)
                                break;

                            var target = Entity.Null;
                            if (spellPrefab.SelfTarget)
                                target = entity;
                            else if (spellPrefab.TargetRequired)
                                target = followStateComponent.Target;

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
