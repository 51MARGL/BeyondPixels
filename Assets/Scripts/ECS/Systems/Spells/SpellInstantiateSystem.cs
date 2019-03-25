using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.SceneBootstraps;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static BeyondPixels.ECS.Components.Spells.SpellBookComponent;

namespace BeyondPixels.ECS.Systems.Spells
{
    public class SpellInstantiateSystem : ComponentSystem
    {
        private ComponentGroup _spellToCastGroup;
        protected override void OnCreateManager()
        {
            _spellToCastGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(ActiveSpellComponent), typeof(InstantiateSpellComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            var spellBookComponent = SpellBookManagerComponent.Instance.SpellBook;
            using (var spellChunks = _spellToCastGroup.CreateArchetypeChunkArray(Allocator.TempJob))
                for (int s = 0; s < spellChunks.Length; s++)
                {
                    var spellChunk = spellChunks[s];
                    var spellEntities = spellChunk.GetNativeArray(GetArchetypeChunkEntityType());
                    var activeSpellComponents = spellChunk.GetNativeArray(GetArchetypeChunkComponentType<ActiveSpellComponent>(true));
                    var instantiateSpellComponents = spellChunk.GetNativeArray(GetArchetypeChunkComponentType<InstantiateSpellComponent>(true));
                    for (int i = 0; i < spellChunk.Count; i++)
                    {
                        var spellPrefab = spellBookComponent.Spells[activeSpellComponents[i].SpellIndex];

                        var position = new float2(0, 0);
                        if (spellPrefab.SelfTarget || spellPrefab.TargetRequired)
                            position = EntityManager.GetComponentData<PositionComponent>(
                                            instantiateSpellComponents[i].Target
                                        ).CurrentPosition;

                        PostUpdateCommands.RemoveComponent<SpellCastingComponent>(instantiateSpellComponents[i].Caster);
                        PostUpdateCommands.RemoveComponent<InstantiateSpellComponent>(spellEntities[i]);

                        InstantiateSpellPrefab(spellPrefab,
                            instantiateSpellComponents[i].Caster,
                            instantiateSpellComponents[i].Target,
                            new float3(position.x, position.y, 100));
                    }
                }
        }

        private void InstantiateSpellPrefab(Spell spell, Entity caster, Entity target, float3 position)
        {
            if (spell.SelfTarget)
                position = new float3(position.x, position.y, 100);
            else if (spell.TargetRequired)
                position = new float3(position.x, position.y, 100);

            var spellObject = GameObject.Instantiate(spell.Prefab, position, Quaternion.identity);
            var spellEntity = spellObject.GetComponent<GameObjectEntity>().Entity;
            PostUpdateCommands.AddComponent(spellEntity,
                new SpellComponent
                {
                    Caster = caster,
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
                        Target = caster
                    });
            else if (spell.TargetRequired)
                PostUpdateCommands.AddComponent(spellEntity,
                    new TargetRequiredComponent
                    {
                        Target = target
                    });

            if (spell.LockOnTarget)
                PostUpdateCommands.AddComponent(spellEntity, new LockOnTargetComponent());
        }
    }
}
