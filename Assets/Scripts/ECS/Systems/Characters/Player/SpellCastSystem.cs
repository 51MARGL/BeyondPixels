using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Spells;
using Unity.Entities;
using UnityEngine;
using static BeyondPixels.ECS.Components.Characters.Common.SpellBookComponent;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class SpellCastSystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public ComponentDataArray<InputComponent> InputComponents;
            public ComponentDataArray<SpellCastingComponent> SpellCastingComponents;
            public ComponentArray<SpellBookComponent> SpellBookComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private Data _data;
        [Inject]
        private ComponentDataFromEntity<TargetComponent> _targetComponents;
        [Inject]
        private ComponentDataFromEntity<PositionComponent> _positionComponents;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                var inputComponent = _data.InputComponents[i];

                var casterEntity = _data.EntityArray[i];

                if (inputComponent.AttackButtonPressed == 1
                    || inputComponent.InputDirection != Vector2.zero)
                {
                    PostUpdateCommands.RemoveComponent<SpellCastingComponent>(casterEntity);
                    return;
                }

                var spellCastingComponent = _data.SpellCastingComponents[i];

                var spellIndex = spellCastingComponent.SpellIndex;
                var spellInitializer = _data.SpellBookComponents[i]
                                        .Spells[spellIndex];

                if (spellInitializer.CoolDownTimeLeft > 0
                    || (spellInitializer.TargetRequired && !_targetComponents.Exists(casterEntity)))
                {
                    PostUpdateCommands.RemoveComponent<SpellCastingComponent>(casterEntity);
                    return;
                }

                if (spellCastingComponent.StartedAt + spellInitializer.CastTime > Time.time)
                    return;

                // No clue why, but gets NativeArray deallocation error without these
                (Entity entity, PositionComponent position) target;
                if (spellInitializer.TargetRequired)
                {
                    target.position = _positionComponents[_targetComponents[casterEntity].Target];
                    target.entity = _targetComponents[casterEntity].Target;
                }
                else
                {
                    target.position = _positionComponents[casterEntity];
                    target.entity = casterEntity;
                }

                InstantiateSpellPrefab(_data.SpellBookComponents[i].Spells[spellIndex],
                    (casterEntity, _positionComponents[casterEntity]),
                    target);

                PostUpdateCommands.RemoveComponent<SpellCastingComponent>(casterEntity);

                spellInitializer.CoolDownTimeLeft = spellInitializer.CoolDown;
            }
        }

        private void InstantiateSpellPrefab(Spell spell,
            (Entity entity, PositionComponent position) caster,
            (Entity entity, PositionComponent position) target)
        {
            var position = new Vector3(0, 0, 100);
            if (spell.SelfTarget)
                position = caster.position.CurrentPosition;
            else if (spell.TargetRequired)
                position = target.position.CurrentPosition;

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
