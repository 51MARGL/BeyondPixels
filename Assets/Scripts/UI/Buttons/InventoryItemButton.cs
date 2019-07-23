using System;
using System.Linq;
using System.Text;

using BeyondPixels.ECS.Components.Items;
using BeyondPixels.UI.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.EventSystems;

namespace BeyondPixels.UI.Buttons
{
    public class InventoryItemButton : ItemButton
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left
                || eventData.button == PointerEventData.InputButton.Right)
            {
                var entityManager = World.Active.EntityManager;
                var eventEntity = entityManager.CreateEntity();
                entityManager.AddComponentData(eventEntity, new InventoryItemButtonPressedComponent
                {
                    ItemEntity = ItemEntity,
                    MouseButton = (int)eventData.button
                });

                UIManager.Instance.HideTooltip();
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            var entityManager = World.Active.EntityManager;
            var itemComponent = entityManager.GetComponentData<ItemComponent>(this.ItemEntity);
            var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[itemComponent.StoreIndex];

            this.GetItemDescriptionWithComparison(entityManager, itemComponent, item, out var header, out var sb);

            var btnDesc = string.Empty;
            switch (item.ItemType)
            {
                case ItemType.Food:
                case ItemType.Potion:
                    btnDesc = "LMB: Use    RMB: Destroy";
                    break;
                case ItemType.Gear:
                    btnDesc = "LMB: Equip    RMB: Destroy";
                    break;
            }
            UIManager.Instance.ShowTooltip(this.transform.position, header, sb.ToString(), btnDesc);
        }        
    }
}
