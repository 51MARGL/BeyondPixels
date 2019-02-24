using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeyondPixels.UI
{
    public class SpellActionButton : MonoBehaviour, IPointerClickHandler
    {
        public Entity SpellCaster;
        public int SpellIndex;
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
                var inputComponent = entityManager.GetComponentData<InputComponent>(this.SpellCaster);
                inputComponent.ActionButtonPressed = this.SpellIndex + 1;
                entityManager.SetComponentData(this.SpellCaster, inputComponent);
            }
        }
    }
}
