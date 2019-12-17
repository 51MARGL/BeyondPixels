using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
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
    public class GameUISystem : ComponentSystem
    {
        private EntityQuery _playerGroup;
        private EntityQuery _activeSpellGroup;
        private EntityQuery _spellButtonEventsGroup;
        protected override void OnCreate()
        {
            this._playerGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(HealthComponent), typeof(PlayerComponent),
                    typeof(MagicStatComponent), typeof(LevelComponent), typeof(XPComponent)
                }
            });
            this._activeSpellGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(ActiveSpellComponent)
                }
            });
            this._spellButtonEventsGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(ActionButtonPressedComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.deltaTime;
            var uiComponent = UIManager.Instance.GameUIComponent;
            var spellBook = SpellBookManagerComponent.Instance.SpellBook;

            this.Entities.With(this._playerGroup).ForEach((Entity playerEntity,
                ref HealthComponent healthComponent,
                ref MagicStatComponent magicStatComponent,
                ref LevelComponent levelComponent,
                ref XPComponent xpComponent) =>
            {
                this.ProcessHealth(healthComponent, uiComponent.HealthGroup, deltaTime);

                this.ProcessLevel(levelComponent, xpComponent, uiComponent.LevelGroup, deltaTime);

                this.ProcessSpells(playerEntity, uiComponent.SpellButtonsGroup, spellBook);
            });


            this.Entities.With(this._spellButtonEventsGroup).ForEach((Entity eventEntity, ref ActionButtonPressedComponent eventComponent) =>
            {
                var actionIndex = eventComponent.ActionIndex + 1;
                this.Entities.With(this._playerGroup).ForEach((ref InputComponent inputComponent) =>
                {
                    inputComponent.ActionButtonPressed = actionIndex;
                });
                this.PostUpdateCommands.DestroyEntity(eventEntity);
            });
        }

        private void ProcessSpells(Entity entity, SpellButtonsGroupWrapper spellButtonsGroup, SpellBookComponent spellBook)
        {
            this.Entities.With(this._activeSpellGroup).ForEach((Entity spellEntity, ref ActiveSpellComponent activeSpellComponent) =>
            {
                if (activeSpellComponent.Owner != entity)
                {
                    return;
                }

                var button = spellButtonsGroup.ActionButtons[activeSpellComponent.ActionIndex - 1];
                var spell = spellBook.Spells[activeSpellComponent.SpellIndex];

                if (button.SpellIcon.sprite != spell.Icon)
                {
                    button.SpellIcon.sprite = spell.Icon;
                }

                if (this.EntityManager.HasComponent<CoolDownComponent>(spellEntity))
                {
                    var coolDownComponent = this.EntityManager.GetComponentData<CoolDownComponent>(spellEntity);
                    if (coolDownComponent.CoolDownTime > 0)
                    {
                        var magicStat = this.EntityManager.GetComponentData<MagicStatComponent>(activeSpellComponent.Owner);
                        var coolDownTime = math.max(3f, spell.CoolDown -
                               (spell.CoolDown / 500f * magicStat.CurrentValue));

                        button.CoolDownImage.enabled = true;
                        button.CoolDownText.enabled = true;
                        button.CoolDownImage.fillAmount -= 1.0f / coolDownTime * Time.deltaTime;
                        button.CoolDownText.text = coolDownComponent.CoolDownTime.ToString("F1");
                    }
                }
                else if (button.CoolDownImage.enabled || button.CoolDownText.enabled)
                {
                    button.CoolDownImage.enabled = false;
                    button.CoolDownText.enabled = false;
                    button.CoolDownImage.fillAmount = 1;
                }
                if (spell.TargetRequired)
                {
                    if (this.EntityManager.HasComponent<TargetComponent>(activeSpellComponent.Owner))
                    {
                        button.SpellIcon.color = new Color(1, 1, 1, 1);
                    }
                    else
                    {
                        button.SpellIcon.color = new Color(1, 1, 1, 0.25f);
                    }
                }
            });
        }

        private void ProcessLevel(LevelComponent levelComponent, XPComponent xpComponent, LevelGroupWrapper levelGroup, float deltaTime)
        {
            var currentLevel = levelComponent.CurrentLevel;
            var xpToNextLevel = levelComponent.NextLevelXP;
            var currentXP = xpComponent.CurrentXP;
            var prevLevelXP = xpToNextLevel / 2f;
            var currentXPFill = (currentXP - prevLevelXP) / (xpToNextLevel - prevLevelXP);
            if (currentLevel == 1)
            {
                currentXPFill = currentXP / (float)xpToNextLevel;
            }

            levelGroup.XPProgressImage.fillAmount
                = math.lerp(levelGroup.XPProgressImage.fillAmount, currentXPFill, deltaTime * 10f);
            levelGroup.LevelText.text = currentLevel.ToString();
        }

        private void ProcessHealth(HealthComponent healthComponent, HealthGroupWrapper healthGroup, float deltaTime)
        {
            var currentHealth = healthComponent.CurrentValue;
            var maxHealth = healthComponent.MaxValue;
            var currentHPFill = currentHealth / maxHealth;

            healthGroup.HealthImage.fillAmount
                = math.lerp(healthGroup.HealthImage.fillAmount, currentHPFill, deltaTime * 10f);
            healthGroup.HealthText.text = currentHealth.ToString("F1") + "/" + maxHealth.ToString("F1");
        }
    }
}
