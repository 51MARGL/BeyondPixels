using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.UI.ECS.Components;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

using static BeyondPixels.UI.ECS.Components.GameUIComponent;

namespace BeyondPixels.UI.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SpellCastBarUISystem : ComponentSystem
    {
        private EntityQuery _group;

        protected override void OnCreate()
        {
            this._group = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(CharacterComponent),
                    typeof(MagicStatComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.deltaTime;
            var uiComponent = UIManager.Instance.GameUIComponent;
            var spellBook = SpellBookManagerComponent.Instance.SpellBook;

            this.Entities.With(this._group).ForEach((Entity entity,
                ref MagicStatComponent magicStatComponent,
                ref CharacterComponent characterComponent) =>
            {
                var barGroup = characterComponent.CharacterType == CharacterType.Player
                    ? UIManager.Instance.GameUIComponent.SpellCastBarGroup
                    : this.EntityManager.GetComponentObject<EnemyUIComponent>(entity).SpellCastBarGroup;

                this.ProcessCastBar(entity, magicStatComponent, barGroup, spellBook);
            });
        }

        private void ProcessCastBar(Entity entity, MagicStatComponent magicStatComponent, SpellCastBarGroupWrapper spellBarGroup, SpellBookComponent spellBook)
        {
            if (this.EntityManager.HasComponent<SpellCastingComponent>(entity))
            {
                var spellCastingComponent = this.EntityManager.GetComponentData<SpellCastingComponent>(entity);
                var spellIndex = spellCastingComponent.SpellIndex;
                var spell = spellBook.Spells[spellIndex];
                var castTime = math.max(0.8f, spell.CastTime -
                            (spell.CastTime / 500f * magicStatComponent.CurrentValue));

                var timePassed = (castTime - (Time.time - spellCastingComponent.StartedAt));

                spellBarGroup.SpellCastCanvasGroup.alpha = 1;
                spellBarGroup.SpellCastBar.color = spell.BarColor;
                spellBarGroup.SpellCastIcon.sprite = spell.Icon;
                spellBarGroup.SpellCastName.text = spell.Name;

                spellBarGroup.SpellCastBar.fillAmount += 1.0f / castTime * Time.deltaTime;
                if (timePassed < 0)
                {
                    spellBarGroup.SpellCastTime.text = "0";
                }
                else
                {
                    spellBarGroup.SpellCastTime.text = timePassed.ToString("F1");
                }
            }
            else
            {
                spellBarGroup.SpellCastCanvasGroup.alpha = 0;
                spellBarGroup.SpellCastBar.fillAmount = 0;
            }
        }
    }
}
