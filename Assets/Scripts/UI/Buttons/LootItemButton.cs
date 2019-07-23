using System;
using System.Text;

using BeyondPixels.ECS.Components.Items;
using BeyondPixels.UI.ECS.Components;
using TMPro;
using Unity.Entities;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeyondPixels.UI.Buttons
{
    public class LootItemButton : ItemButton
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                var entityManager = World.Active.EntityManager;
                var eventEntity = entityManager.CreateEntity();
                entityManager.AddComponentData(eventEntity, new LootItemButtonPressedComponent
                {
                    ItemEntity = ItemEntity
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

            UIManager.Instance.ShowTooltip(this.transform.position, header, sb.ToString(), "LMB: Pick Up", true);
        }
    }
}
