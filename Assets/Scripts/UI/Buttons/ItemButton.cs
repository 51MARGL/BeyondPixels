using System;
using System.Linq;
using System.Text;
using BeyondPixels.ECS.Components.Items;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeyondPixels.UI.Buttons
{
    public abstract class ItemButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image ItemIcon;
        public Entity ItemEntity;
        public TextMeshProUGUI Amount;

        public abstract void OnPointerClick(PointerEventData eventData);

        public abstract void OnPointerEnter(PointerEventData eventData);

        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager.Instance.HideTooltip();
        }

        public virtual void GetItemDescriptionWithComparison(EntityManager entityManager, ItemComponent itemComponent, ItemsStoreComponent.Item item, out string header, out StringBuilder sb)
        {
            header = item.Name;
            sb = new StringBuilder();
            sb.Append($"Item Level: {itemComponent.Level}");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            if (item.ItemType == ItemType.Gear)
            {
                var equipped = UIManager.Instance.PlayerInfoMenuUIComponent.GearGroup.GearSlots
                                        .Where(slot => slot.GearType == item.GearType).First();
                var equippedEntity = equipped.ItemEntity;
                var eqAttackValue = 0;
                var eqDefenceValue = 0;
                var eqHealthValue = 0;
                var eqMagicValue = 0;

                if (equipped.HasItem)
                {
                    var eqItemComponent = entityManager.GetComponentData<ItemComponent>(equippedEntity);
                    if (entityManager.HasComponent<AttackStatModifierComponent>(equippedEntity))
                    {
                        var eqStatComponent = entityManager.GetComponentData<AttackStatModifierComponent>(equippedEntity);
                        eqAttackValue = eqStatComponent.Value * eqItemComponent.Level;
                    }
                    if (entityManager.HasComponent<DefenceStatModifierComponent>(equippedEntity))
                    {
                        var eqStatComponent = entityManager.GetComponentData<DefenceStatModifierComponent>(equippedEntity);
                        eqDefenceValue = eqStatComponent.Value * eqItemComponent.Level;
                    }
                    if (entityManager.HasComponent<HealthStatModifierComponent>(equippedEntity))
                    {
                        var eqStatComponent = entityManager.GetComponentData<HealthStatModifierComponent>(equippedEntity);
                        eqHealthValue = eqStatComponent.Value * eqItemComponent.Level;
                    }
                    if (entityManager.HasComponent<MagickStatModifierComponent>(equippedEntity))
                    {
                        var eqStatComponent = entityManager.GetComponentData<MagickStatModifierComponent>(equippedEntity);
                        eqMagicValue = eqStatComponent.Value * eqItemComponent.Level;
                    }
                }

                if (entityManager.HasComponent<AttackStatModifierComponent>(this.ItemEntity))
                {
                    var statComponent = entityManager.GetComponentData<AttackStatModifierComponent>(this.ItemEntity);
                    var totalValue = statComponent.Value * itemComponent.Level;
                    sb.Append($"Attack: {totalValue}");

                    var difference = totalValue - eqAttackValue;
                    this.AddDifferenceText(sb, difference);

                    sb.Append(Environment.NewLine);
                }
                else
                {
                    if (eqAttackValue > 0)
                    {
                        sb.Append($"Attack: 0 (-{eqAttackValue})");
                        sb.Append(Environment.NewLine);
                    }
                }

                if (entityManager.HasComponent<DefenceStatModifierComponent>(this.ItemEntity))
                {
                    var statComponent = entityManager.GetComponentData<DefenceStatModifierComponent>(this.ItemEntity);
                    var totalValue = statComponent.Value * itemComponent.Level;
                    sb.Append($"Defence: {totalValue}");

                    var difference = totalValue - eqDefenceValue;
                    this.AddDifferenceText(sb, difference);

                    sb.Append(Environment.NewLine);
                }
                else
                {
                    if (eqDefenceValue > 0)
                    {
                        sb.Append($"Attack: 0 (-{eqDefenceValue})");
                        sb.Append(Environment.NewLine);
                    }
                }

                if (entityManager.HasComponent<HealthStatModifierComponent>(this.ItemEntity))
                {
                    var statComponent = entityManager.GetComponentData<HealthStatModifierComponent>(this.ItemEntity);
                    var totalValue = statComponent.Value * itemComponent.Level;
                    sb.Append($"Health: {totalValue}");

                    var difference = totalValue - eqHealthValue;
                    this.AddDifferenceText(sb, difference);

                    sb.Append(Environment.NewLine);
                }
                else
                {
                    if (eqHealthValue > 0)
                    {
                        sb.Append($"Attack: 0 (-{eqHealthValue})");
                        sb.Append(Environment.NewLine);
                    }
                }

                if (entityManager.HasComponent<MagickStatModifierComponent>(this.ItemEntity))
                {
                    var statComponent = entityManager.GetComponentData<MagickStatModifierComponent>(this.ItemEntity);
                    var totalValue = statComponent.Value * itemComponent.Level;
                    sb.Append($"Magic: {totalValue}");

                    var difference = totalValue - eqMagicValue;
                    this.AddDifferenceText(sb, difference);

                    sb.Append(Environment.NewLine);
                }
                else
                {
                    if (eqMagicValue > 0)
                    {
                        sb.Append($"Attack: 0 (-{eqMagicValue})");
                        sb.Append(Environment.NewLine);
                    }
                }
            }
            sb.Append(Environment.NewLine);
            sb.Append(item.Description.Replace("{value}", item.ModifierValue.ToString()));
        }

        private void AddDifferenceText(StringBuilder sb, int difference)
        {
            if (math.abs(difference) > 0)
            {
                sb.Append(" (");
                if (difference > 0)
                    sb.Append("+");

                sb.Append($"{difference})");
            }
        }
    }
}
