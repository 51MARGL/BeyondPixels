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
    public class LootItemButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
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
                entityManager.AddComponentData(eventEntity, new LootItemButtonPressedComponent
                {
                    ItemEntity = ItemEntity
                });

                UIManager.Instance.HideTooltip();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            var itemComponent = entityManager.GetComponentData<ItemComponent>(this.ItemEntity);
            var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[itemComponent.StoreIndex];
            var header = item.Name;
            var sb = new StringBuilder();
            sb.Append($"Item Level: {itemComponent.Level}");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            if (entityManager.HasComponent<AttackStatModifierComponent>(this.ItemEntity))
            {
                var statComponent = entityManager.GetComponentData<AttackStatModifierComponent>(this.ItemEntity);
                sb.Append($"Attack: +{statComponent.Value * itemComponent.Level}");
                sb.Append(Environment.NewLine);
            }
            if (entityManager.HasComponent<DefenceStatModifierComponent>(this.ItemEntity))
            {
                var statComponent = entityManager.GetComponentData<DefenceStatModifierComponent>(this.ItemEntity);
                sb.Append($"Defence: +{statComponent.Value * itemComponent.Level}");
                sb.Append(Environment.NewLine);
            }
            if (entityManager.HasComponent<HealthStatModifierComponent>(this.ItemEntity))
            {
                var statComponent = entityManager.GetComponentData<HealthStatModifierComponent>(this.ItemEntity);
                sb.Append($"Health: +{statComponent.Value * itemComponent.Level}");
                sb.Append(Environment.NewLine);
            }
            if (entityManager.HasComponent<MagickStatModifierComponent>(this.ItemEntity))
            {
                var statComponent = entityManager.GetComponentData<MagickStatModifierComponent>(this.ItemEntity);
                sb.Append($"Magic: +{statComponent.Value * itemComponent.Level}");
                sb.Append(Environment.NewLine);
            }
            sb.Append(Environment.NewLine);
            sb.Append(item.Description.Replace("{value}", item.ModifierValue.ToString()));

            UIManager.Instance.ShowTooltip(this.transform.position, header, sb.ToString(), "LMB: Pick Up", true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager.Instance.HideTooltip();
        }
    }
}
