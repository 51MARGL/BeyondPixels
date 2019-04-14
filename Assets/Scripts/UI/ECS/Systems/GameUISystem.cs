﻿using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.UI.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.UI.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class GameUISystem : ComponentSystem
    {
        private ComponentGroup _playerGroup;
        private ComponentGroup _playerSpellCastingGroup;
        private ComponentGroup _activeSpellGroup;
        private ComponentGroup _spellButtonEventsGroup;
        protected override void OnCreateManager()
        {
            _playerGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(HealthComponent), typeof(PlayerComponent),
                    typeof(MagicStatComponent), typeof(LevelComponent), typeof(XPComponent)
                }
            });
            _playerSpellCastingGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(SpellCastingComponent)
                }
            });
            _activeSpellGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(ActiveSpellComponent)
                }
            });
            _spellButtonEventsGroup = GetComponentGroup(new EntityArchetypeQuery
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

            Entities.With(_playerGroup).ForEach((Entity playerEntity, 
                ref HealthComponent healthComponent, 
                ref MagicStatComponent magicStatComponent, 
                ref LevelComponent levelComponent, 
                ref XPComponent xpComponent) =>
            {
                var playerUIHealthGroup = uiComponent.HealthGroup;

                var currentHealth = healthComponent.CurrentValue;
                var maxHealth = healthComponent.MaxValue;
                var currentHPFill = currentHealth / maxHealth;

                playerUIHealthGroup.HealthImage.fillAmount
                    = math.lerp(playerUIHealthGroup.HealthImage.fillAmount, currentHPFill, deltaTime * 10f);
                playerUIHealthGroup.HealthText.text = currentHealth.ToString("F1") + "/" + maxHealth.ToString("F1");


                var playerUILevelGroup = uiComponent.LevelGroup;

                var currentLevel =  levelComponent.CurrentLevel;
                var xpToNextLevel = levelComponent.NextLevelXP;
                var currentXP = xpComponent.CurrentXP;
                var prevLevelXP = xpToNextLevel / 2f;
                var currentXPFill = (currentXP - prevLevelXP) / (xpToNextLevel - prevLevelXP);
                if (currentLevel == 1)
                    currentXPFill = currentXP / (float)xpToNextLevel;

                playerUILevelGroup.XPProgressImage.fillAmount
                    = math.lerp(playerUILevelGroup.XPProgressImage.fillAmount, currentXPFill, deltaTime * 10f);
                playerUILevelGroup.LevelText.text = currentLevel.ToString();

                if (EntityManager.HasComponent<SpellCastingComponent>(playerEntity))
                {
                    var playerUISpellBarGroup = uiComponent.SpellCastBarGroup;
                    var spellCastingComponent = EntityManager.GetComponentData<SpellCastingComponent>(playerEntity);
                    var spellIndex = spellCastingComponent.SpellIndex;
                    var spell = spellBook.Spells[spellIndex];
                    var castTime = math.max(1f, spell.CastTime -
                                (spell.CastTime / 100f * magicStatComponent.CurrentValue));

                    var timePassed = (castTime - (Time.time - spellCastingComponent.StartedAt));

                    playerUISpellBarGroup.SpellCastCanvasGroup.alpha = 1;
                    playerUISpellBarGroup.SpellCastBar.color = spell.BarColor;
                    playerUISpellBarGroup.SpellCastIcon.sprite = spell.Icon;
                    playerUISpellBarGroup.SpellCastName.text = spell.Name;

                    playerUISpellBarGroup.SpellCastBar.fillAmount += 1.0f / castTime * Time.deltaTime;
                    if (timePassed < 0)
                        playerUISpellBarGroup.SpellCastTime.text = "0";
                    else
                        playerUISpellBarGroup.SpellCastTime.text = timePassed.ToString("F1");
                }
                else
                {
                    var playerUISpellBarGroup = uiComponent.SpellCastBarGroup;
                    playerUISpellBarGroup.SpellCastCanvasGroup.alpha = 0;
                    playerUISpellBarGroup.SpellCastBar.fillAmount = 0;
                }
            });

            Entities.With(_activeSpellGroup).ForEach((Entity spellEntity, ref ActiveSpellComponent activeSpellComponent) =>
            {
                var playerUISpellButtonsGroup = uiComponent.SpellButtonsGroup;
                var button = playerUISpellButtonsGroup.ActionButtons[activeSpellComponent.ActionIndex - 1];
                var spell = spellBook.Spells[activeSpellComponent.SpellIndex];

                if (button.SpellIcon.sprite != spell.Icon)
                    button.SpellIcon.sprite = spell.Icon;

                if (EntityManager.HasComponent<CoolDownComponent>(spellEntity))
                {
                    var coolDownComponent = EntityManager.GetComponentData<CoolDownComponent>(spellEntity);
                    if (coolDownComponent.CoolDownTime > 0)
                    {
                        button.CoolDownImage.enabled = true;
                        button.CoolDownText.enabled = true;
                        button.CoolDownImage.fillAmount -= 1.0f / spell.CoolDown * Time.deltaTime;
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
                    if (EntityManager.HasComponent<TargetComponent>(activeSpellComponent.Owner))
                        button.SpellIcon.color = new Color(1, 1, 1, 1);
                    else
                        button.SpellIcon.color = new Color(1, 1, 1, 0.25f);
                }
            });

            Entities.With(_spellButtonEventsGroup).ForEach((Entity eventEntity, ref ActionButtonPressedComponent eventComponent) =>
            {
                var actionIndex = eventComponent.ActionIndex + 1;
                Entities.With(_playerGroup).ForEach((ref InputComponent inputComponent) =>
                {
                    inputComponent.ActionButtonPressed = actionIndex;
                });
                PostUpdateCommands.DestroyEntity(eventEntity);
            });
        }
    }
}