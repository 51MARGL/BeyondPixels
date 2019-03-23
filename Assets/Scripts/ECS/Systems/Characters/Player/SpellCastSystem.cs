using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Spells;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static BeyondPixels.ECS.Components.Characters.Common.SpellBookComponent;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class SpellCastSystem : ComponentSystem
    {
        private ComponentGroup _spellCasterGroup;
        protected override void OnCreateManager()
        {
            _spellCasterGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(InputComponent), typeof(SpellCastingComponent),
                    typeof(SpellBookComponent), typeof(PositionComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            using (var chunks = _spellCasterGroup.CreateArchetypeChunkArray(Allocator.TempJob))
                for (int c = 0; c < chunks.Length; c++)
                {
                    var chunk = chunks[c];
                    var entities = chunk.GetNativeArray(GetArchetypeChunkEntityType());
                    var inputComponents = chunk.GetNativeArray(GetArchetypeChunkComponentType<InputComponent>(true));
                    var positionComponents = chunk.GetNativeArray(GetArchetypeChunkComponentType<PositionComponent>(true));
                    var spellCastingComponents = chunk.GetNativeArray(GetArchetypeChunkComponentType<SpellCastingComponent>(true));
                    for (int i = 0; i < chunk.Count; i++)
                    {
                        var entity = entities[i];
                        var positionComponent = positionComponents[i];
                        var spellCastingComponent = spellCastingComponents[i];
                        var spellBookComponent = EntityManager.GetComponentObject<SpellBookComponent>(entity);

                        if (inputComponents[i].AttackButtonPressed == 1
                            || !(inputComponents[i].InputDirection.Equals(float2.zero)))
                        {
                            PostUpdateCommands.RemoveComponent<SpellCastingComponent>(entity);
                            return;
                        }

                        var spellIndex = spellCastingComponent.SpellIndex;
                        var spellInitializer = spellBookComponent.Spells[spellIndex];

                        if (spellInitializer.CoolDownTimeLeft > 0
                            || (spellInitializer.TargetRequired
                            && !EntityManager.HasComponent<TargetComponent>(entity)))
                        {
                            PostUpdateCommands.RemoveComponent<SpellCastingComponent>(entity);
                            return;
                        }

                        if (spellCastingComponent.StartedAt + spellInitializer.CastTime > Time.time)
                            return;

                        (Entity entity, PositionComponent position) target;
                        if (spellInitializer.TargetRequired)
                        {
                            var targetComponent = EntityManager.GetComponentData<TargetComponent>(entity);
                            target.position =
                                EntityManager.GetComponentData<PositionComponent>(targetComponent.Target);
                            target.entity = targetComponent.Target;
                        }
                        else
                        {
                            target.position = positionComponent;
                            target.entity = entity;
                        }

                        InstantiateSpellPrefab(spellBookComponent.Spells[spellIndex],
                            (entity, positionComponent),
                            target);

                        PostUpdateCommands.RemoveComponent<SpellCastingComponent>(entity);

                        spellInitializer.CoolDownTimeLeft = spellInitializer.CoolDown;
                    }
                }
        }

        private void InstantiateSpellPrefab(Spell spell,
            (Entity entity, PositionComponent position) caster,
            (Entity entity, PositionComponent position) target)
        {
            var position = new float3(0, 0, 100);
            if (spell.SelfTarget)
                position = new float3(caster.position.CurrentPosition.x, caster.position.CurrentPosition.y, 100);
            else if (spell.TargetRequired)
                position = new float3(target.position.CurrentPosition.x, target.position.CurrentPosition.y, 100);

            var spellObject = GameObject.Instantiate(spell.Prefab, position, Quaternion.identity);
            var spellEntity = spellObject.GetComponent<GameObjectEntity>().Entity;
            PostUpdateCommands.AddComponent(spellEntity,
                new SpellComponent
                {
                    Caster = caster.entity,
                    CoolDown = spell.CoolDown
                });
            PostUpdateCommands.AddComponent(spellEntity,
                new DamageComponent
                {
                    DamageOnImpact = spell.DamageOnImpact,
                    DamagePerSecond = spell.DamagePerSecond,
                    DamageType = spell.DamageType
                });

            if (spell.Duration > 0)
                PostUpdateCommands.AddComponent(spellEntity,
                new DurationComponent
                {
                    Duration = spell.Duration
                });
            else
                PostUpdateCommands.AddComponent(spellEntity, new DestroyOnImpactComponent());

            if (spell.SelfTarget)
                PostUpdateCommands.AddComponent(spellEntity,
                    new TargetRequiredComponent
                    {
                        Target = caster.entity
                    });
            else if (spell.TargetRequired)
                PostUpdateCommands.AddComponent(spellEntity,
                    new TargetRequiredComponent
                    {
                        Target = target.entity
                    });

            if (spell.LockOnTarget)
                PostUpdateCommands.AddComponent(spellEntity, new LockOnTargetComponent());
        }
    }
}
