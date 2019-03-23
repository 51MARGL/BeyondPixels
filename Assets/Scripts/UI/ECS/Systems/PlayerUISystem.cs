using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.UI.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.UI.ECS.Systems
{
    public class PlayerUISystem : ComponentSystem
    {
        private struct HealthData
        {
            public readonly int Length;
            public ComponentArray<PlayerUIComponent> PlayerUIComponents;
            public ComponentDataArray<HealthComponent> HealthComponents;
        }
        [Inject]
        private HealthData _healthData;

        private struct SpellStateComponent : IComponentData
        {
            public float TimePassed;
            public float CastTime;
        }
        private struct SpellAddedData
        {
            public readonly int Length;
            public ComponentArray<PlayerUIComponent> PlayerUIComponents;
            public ComponentArray<SpellBookComponent> SpellBookComponents;
            public ComponentDataArray<SpellCastingComponent> SpellCastingComponents;
            public ExcludeComponent<SpellStateComponent> CompStates;
            public EntityArray EntityArray;
        }
        [Inject]
        private SpellAddedData _spellAdded;
        private struct SpellChangedData
        {
            public readonly int Length;
            public ComponentArray<PlayerUIComponent> PlayerUIComponents;
            public ComponentArray<SpellBookComponent> SpellBookComponents;
            public ComponentDataArray<SpellCastingComponent> SpellCastingComponents;
            public ComponentDataArray<SpellStateComponent> CompStates;
            public EntityArray EntityArray;
        }
        [Inject]
        private SpellChangedData _spellChanged;
        private struct SpellRemovedData
        {
            public readonly int Length;
            public ComponentArray<PlayerUIComponent> PlayerUIComponents;
            public ComponentArray<SpellBookComponent> SpellBookComponents;
            public ExcludeComponent<SpellCastingComponent> SpellCastingComponents;
            public ComponentDataArray<SpellStateComponent> CompStates;
            public EntityArray EntityArray;
        }
        [Inject]
        private SpellRemovedData _spellRemoved;

        private struct SpellCoolDownData
        {
            public readonly int Length;
            public ComponentArray<PlayerUIComponent> PlayerUIComponents;
            public ComponentArray<SpellBookComponent> SpellBookComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private SpellCoolDownData _spellCoolDown;

        protected override void OnUpdate()
        {
            var deltaTime = Time.deltaTime;
            for (int i = 0; i < _healthData.Length; i++)
            {
                var playerUIHealthGroup = _healthData.PlayerUIComponents[i].HealthGroup;

                var currentHealth = _healthData.HealthComponents[i].CurrentValue;
                var maxHealth = _healthData.HealthComponents[i].MaxValue;
                var currentFill = (float)currentHealth / maxHealth;

                playerUIHealthGroup.HealthImage.fillAmount
                    = math.lerp(playerUIHealthGroup.HealthImage.fillAmount, currentFill, deltaTime * 10f);
                playerUIHealthGroup.HealthText.text = currentHealth + " / " + maxHealth;
            }



            for (int i = 0; i < _spellAdded.Length; i++)
            {
                var playerUISpellBarGroup = _spellAdded.PlayerUIComponents[i].SpellCastBarGroup;

                playerUISpellBarGroup.SpellCastCanvasGroup.alpha = 1;

                var spellIndex = _spellAdded.SpellCastingComponents[i].SpellIndex;
                var spellInitializer = _spellAdded.SpellBookComponents[i].Spells[spellIndex];

                playerUISpellBarGroup.SpellCastBar.fillAmount = 0;
                playerUISpellBarGroup.SpellCastBar.color = spellInitializer.BarColor;
                playerUISpellBarGroup.SpellCastIcon.sprite = spellInitializer.Icon;
                playerUISpellBarGroup.SpellCastName.text = spellInitializer.Name;

                PostUpdateCommands.AddComponent(_spellAdded.EntityArray[i],
                    new SpellStateComponent
                    {
                        TimePassed = Time.deltaTime,
                        CastTime = spellInitializer.CastTime
                    });
            }
            for (int i = 0; i < _spellChanged.Length; i++)
            {
                var playerUISpellBarGroup = _spellChanged.PlayerUIComponents[i].SpellCastBarGroup;

                var spellIndex = _spellChanged.SpellCastingComponents[i].SpellIndex;

                var spellStateComponent = _spellChanged.CompStates[i];

                playerUISpellBarGroup.SpellCastBar.fillAmount += 1.0f / spellStateComponent.CastTime * Time.deltaTime;
                spellStateComponent.TimePassed += Time.deltaTime;
                playerUISpellBarGroup.SpellCastTime.text =
                    (spellStateComponent.CastTime - spellStateComponent.TimePassed).ToString("F1");

                if (spellStateComponent.CastTime - spellStateComponent.TimePassed < 0)
                    playerUISpellBarGroup.SpellCastTime.text = "0";

                PostUpdateCommands.SetComponent(_spellChanged.EntityArray[i], spellStateComponent);
            }
            for (int i = 0; i < _spellRemoved.Length; i++)
            {
                var playerUISpellBarGroup = _spellRemoved.PlayerUIComponents[i].SpellCastBarGroup;
                playerUISpellBarGroup.SpellCastCanvasGroup.alpha = 0;

                PostUpdateCommands.RemoveComponent<SpellStateComponent>(_spellRemoved.EntityArray[i]);
            }

            for (int i = 0; i < _spellCoolDown.Length; i++)
            {
                var playerUISpellButtonsGroup = _spellCoolDown.PlayerUIComponents[i].SpellButtonsGroup;
                foreach (var button in playerUISpellButtonsGroup.ActionButtons)
                {
                    var spell = _spellCoolDown.SpellBookComponents[i].Spells[button.SpellIndex];
                    if (spell.CoolDownTimeLeft > 0)
                    {
                        button.CoolDownImage.enabled = true;
                        button.CoolDownText.enabled = true;
                        button.CoolDownImage.fillAmount -= 1.0f / spell.CoolDown * Time.deltaTime;
                        button.CoolDownText.text = spell.CoolDownTimeLeft.ToString("F1");
                    }
                    else if (button.CoolDownImage.enabled || button.CoolDownText.enabled)
                    {
                        button.CoolDownImage.enabled = false;
                        button.CoolDownText.enabled = false;
                        button.CoolDownImage.fillAmount = 1;
                    }
                    if (spell.TargetRequired)
                    {
                        if (EntityManager.HasComponent<TargetComponent>(_spellCoolDown.EntityArray[i]))
                            button.SpellIcon.color = new Color(1, 1, 1, 1);
                        else
                            button.SpellIcon.color = new Color(1, 1, 1, 0.25f);
                    }

                }
            }
        }
    }
}
