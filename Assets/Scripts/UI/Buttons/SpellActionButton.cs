using System;
using System.Text;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.UI.ECS.Components;

using TMPro;

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeyondPixels.UI.Buttons
{
    public class SpellActionButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image CoolDownImage;
        public TextMeshProUGUI CoolDownText;

        private Image _spellIcon;

        public Image SpellIcon
        {
            get
            {
                if (this._spellIcon == null)
                    this._spellIcon = this.GetComponent<Image>();

                return this._spellIcon;
            }
            set => this._spellIcon = value;
        }

        private void Start()
        {
            this.SpellIcon = this.GetComponent<Image>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                var entityManager = World.Active.EntityManager;
                var eventEntity = entityManager.CreateEntity();
                var index = Array.IndexOf(UIManager.Instance.GameUIComponent.SpellButtonsGroup.ActionButtons, this);
                entityManager.AddComponentData(eventEntity, new ActionButtonPressedComponent
                {
                    ActionIndex = index
                });
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var entityManager = World.Active.EntityManager;
                var playerEntity = player.GetComponent<GameObjectEntity>().Entity;
                var magicStatComponent = entityManager.GetComponentData<MagicStatComponent>(playerEntity);
                var index = Array.IndexOf(UIManager.Instance.GameUIComponent.SpellButtonsGroup.ActionButtons, this);
                var spell = SpellBookManagerComponent.Instance.SpellBook.Spells[index];

                var header = spell.Name;
                var sb = new StringBuilder();
                sb.Append(Environment.NewLine);
                sb.Append(spell.Description);
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);

                var castTime = math.max(0.8f, spell.CastTime -
                            (spell.CastTime / 500f * magicStatComponent.CurrentValue));
                var coolDown = math.max(3f, spell.CoolDown -
                            (spell.CoolDown / 500f * magicStatComponent.CurrentValue));

                var damageOnImpact = spell.DamageOnImpact + 
                    (spell.DamageOnImpact / 100f * magicStatComponent.CurrentValue);
                var damagePerSecond = spell.DamagePerSecond +
                    (spell.DamagePerSecond / 100f * magicStatComponent.CurrentValue);

                sb.Append($"Cast time: {castTime.ToString("F1")}");
                sb.Append(Environment.NewLine);
                sb.Append($"Cooldown: {coolDown.ToString("F1")}");
                sb.Append(Environment.NewLine);

                if (damageOnImpact > 0)
                {
                    sb.Append($"Impact damage: {damageOnImpact.ToString("F1")}");
                    sb.Append(Environment.NewLine);
                }
                if (damagePerSecond > 0)
                {
                    sb.Append($"Per second damage: {damagePerSecond.ToString("F1")}");
                    sb.Append(Environment.NewLine);
                }

                UIManager.Instance.ShowTooltip(this.transform.position, header, sb.ToString(), "LMB: Cast"); 
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager.Instance.HideTooltip();
        }
    }
}
