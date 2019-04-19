using BeyondPixels.UI.ECS.Components;
using TMPro;
using Unity.Entities;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeyondPixels.UI.Buttons
{
    public class InventoryItemButton : MonoBehaviour, IPointerClickHandler
    {
        public Image ItemIcon;
        public Entity ItemEntity;
        public TextMeshProUGUI Amount;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                var entityManager = World.Active.GetOrCreateManager<EntityManager>();
                var eventEntity = entityManager.CreateEntity();
                entityManager.AddComponentData(eventEntity, new InventoryItemButtonPressedComponent
                {
                    ItemEntity = ItemEntity
                });
            }
        }
    }
}
