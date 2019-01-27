using BeyondPixels.Components.Characters.Common;
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

                if (!entityManager.HasComponent<SpellCastingComponent>(this.SpellCaster))
                    entityManager.AddComponentData(this.SpellCaster, new SpellCastingComponent
                    {
                        SpellIndex = this.SpellIndex,
                        StartedAt = Time.time
                    });
            }
        }
    }
}
