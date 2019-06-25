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
            var itemComponent = entityManager.GetComponentData<ItemComponent>(ItemEntity);
            var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[itemComponent.StoreIndex];
            var header = item.Name;            
            var sb = new StringBuilder();
            sb.Append($"Item Level: {itemComponent.Level}");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            if (entityManager.HasComponent<AttackStatModifierComponent>(ItemEntity))
            {
                var statComponent = entityManager.GetComponentData<AttackStatModifierComponent>(ItemEntity);
                sb.Append($"Attack: +{statComponent.Value * itemComponent.Level}");
                sb.Append(Environment.NewLine);
            }
            if (entityManager.HasComponent<DefenceStatModifierComponent>(ItemEntity))
            {
                var statComponent = entityManager.GetComponentData<DefenceStatModifierComponent>(ItemEntity);
                sb.Append($"Defence: +{statComponent.Value * itemComponent.Level}");
                sb.Append(Environment.NewLine);
            }
            if (entityManager.HasComponent<HealthStatModifierComponent>(ItemEntity))
            {
                var statComponent = entityManager.GetComponentData<HealthStatModifierComponent>(ItemEntity);
                sb.Append($"Health: +{statComponent.Value * itemComponent.Level}");
                sb.Append(Environment.NewLine);
            }
            if (entityManager.HasComponent<MagickStatModifierComponent>(ItemEntity))
            {
                var statComponent = entityManager.GetComponentData<MagickStatModifierComponent>(ItemEntity);
                sb.Append($"Magic: +{statComponent.Value * itemComponent.Level}");
                sb.Append(Environment.NewLine);
            }
            sb.Append(Environment.NewLine);
            sb.Append(item.Description.Replace("{value}", item.ModifierValue.ToString()));

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
