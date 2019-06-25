using System;

using BeyondPixels.UI.ECS.Components;

using TMPro;

using Unity.Entities;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeyondPixels.UI.Buttons
{
    public class SpellActionButton : MonoBehaviour, IPointerClickHandler
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
    }
}
