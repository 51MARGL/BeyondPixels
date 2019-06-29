using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Spells;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

using static BeyondPixels.ECS.Components.Spells.SpellBookComponent;

namespace BeyondPixels.ECS.Systems.Spells
{
    public class SpellInstantiateSystem : ComponentSystem
    {
        private EntityQuery _spellToCastGroup;
        protected override void OnCreate()
        {
            this._spellToCastGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(ActiveSpellComponent), typeof(InstantiateSpellComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            var spellBookComponent = SpellBookManagerComponent.Instance.SpellBook;
            var spellChunks = this._spellToCastGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            for (var s = 0; s < spellChunks.Length; s++)
            {
                var spellChunk = spellChunks[s];
                var spellEntities = spellChunk.GetNativeArray(this.GetArchetypeChunkEntityType());
                var activeSpellComponents = spellChunk.GetNativeArray(this.GetArchetypeChunkComponentType<ActiveSpellComponent>(true));
                var instantiateSpellComponents = spellChunk.GetNativeArray(this.GetArchetypeChunkComponentType<InstantiateSpellComponent>(true));
                for (var i = 0; i < spellChunk.Count; i++)
                {
                    var spellPrefab = spellBookComponent.Spells[activeSpellComponents[i].SpellIndex];

                    var position = new float2(0, 0);
                    if (spellPrefab.SelfTarget || spellPrefab.TargetRequired)
                        position = this.EntityManager.GetComponentData<PositionComponent>(
                                        instantiateSpellComponents[i].Target
                                    ).CurrentPosition;

                    this.PostUpdateCommands.RemoveComponent<SpellCastingComponent>(instantiateSpellComponents[i].Caster);
                    this.PostUpdateCommands.RemoveComponent<InstantiateSpellComponent>(spellEntities[i]);

                    this.InstantiateSpellPrefab(spellPrefab,
                        instantiateSpellComponents[i].Caster,
                        instantiateSpellComponents[i].Target,
                        new float3(position.x, position.y, 100));
                }
            }
            spellChunks.Dispose();
        }

        private void InstantiateSpellPrefab(Spell spell, Entity caster, Entity target, float3 position)
        {
            if (spell.SelfTarget)
                position = new float3(position.x, position.y, 100);
            else if (spell.TargetRequired)
                position = new float3(position.x, position.y, 100);

            var spellObject = GameObject.Instantiate(spell.Prefab, position, Quaternion.identity);
            var spellEntity = spellObject.GetComponent<GameObjectEntity>().Entity;
            this.PostUpdateCommands.AddComponent(spellEntity,
                new SpellComponent
                {
                    Caster = caster,
                });
            this.PostUpdateCommands.AddComponent(spellEntity,
                new DamageComponent
                {
                    DamageOnImpact = spell.DamageOnImpact,
                    DamagePerSecond = spell.DamagePerSecond,
                    DamageType = spell.DamageType
                });

            if (spell.Duration > 0)
                this.PostUpdateCommands.AddComponent(spellEntity,
                new DurationComponent
                {
                    Duration = spell.Duration
                });
            else
                this.PostUpdateCommands.AddComponent(spellEntity, new DestroyOnImpactComponent());

            if (spell.SelfTarget)
                this.PostUpdateCommands.AddComponent(spellEntity,
                    new TargetRequiredComponent
                    {
                        Target = caster
                    });
            else if (spell.TargetRequired)
                this.PostUpdateCommands.AddComponent(spellEntity,
                    new TargetRequiredComponent
                    {
                        Target = target
                    });

            if (spell.LockOnTarget)
                this.PostUpdateCommands.AddComponent(spellEntity, new LockOnTargetComponent());
        }
    }
}
