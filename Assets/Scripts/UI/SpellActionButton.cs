using System;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.UI.ECS.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeyondPixels.UI
{
    public class SpellActionButton : MonoBehaviour, IPointerClickHandler
    {
        public Image CoolDownImage;
        public Text CoolDownText;

        private Image _spellIcon;

        public Image SpellIcon
        {
            get
            {
                if (_spellIcon == null)
                    _spellIcon = GetComponent<Image>();

                return _spellIcon;
            }
            set => _spellIcon = value;
        }

        private void Start()
        {
            SpellIcon = GetComponent<Image>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                var entityManager = World.Active.GetOrCreateManager<EntityManager>();
                var eventEntity = entityManager.CreateEntity();
                var index = Array.IndexOf(UIManager.Instance.UIComponent.SpellButtonsGroup.ActionButtons, this);
                entityManager.AddComponentData(eventEntity, new ActionButtonPressedComponent
                {
                    ActionIndex = index
                });
            }
        }
    }
}
