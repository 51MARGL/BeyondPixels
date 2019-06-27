using System.Linq;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Game;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.UI.Buttons;
using BeyondPixels.UI.ECS.Components;
using Unity.Entities;

using UnityEngine;
using static BeyondPixels.UI.ECS.Components.PlayerInfoMenuUIComponent;

namespace BeyondPixels.UI.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class PlayerInfoMenuUISystem : ItemSorterSystem
    {
        private EntityQuery _playerGroup;
        private EntityQuery _equipedItemsGroup;
        private EntityQuery _addStatButtonEventsGroup;
        private EntityQuery _inventoryItemButtonEventsGroup;
        private bool _inventoryInitialized;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();

            this._inventoryInitialized = false;

            this._playerGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(PlayerComponent),
                    typeof(HealthStatComponent),typeof(AttackStatComponent),
                    typeof(DefenceStatComponent), typeof(MagicStatComponent), typeof(LevelComponent)
                },
                None = new ComponentType[] {
                    typeof(InCutsceneComponent),
                }
            });
            this._equipedItemsGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(ItemComponent),
                    typeof(PickedUpComponent),
                    typeof(EquipedComponent)
                }
            });
            this._addStatButtonEventsGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(AddStatButtonPressedComponent)
                }
            });
            this._inventoryItemButtonEventsGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(InventoryItemButtonPressedComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._playerGroup).ForEach((Entity playerEntity,
                ref HealthStatComponent healthstatComponent,
                ref AttackStatComponent attackStatComponent,
                ref DefenceStatComponent defenceStatComponent,
                ref MagicStatComponent magicStatComponent,
                ref LevelComponent levelComponent) =>
            {

                var infoMenuComponent = UIManager.Instance.PlayerInfoMenuUIComponent;
                if (Input.GetKeyDown(SettingsManager.Instance.GetKeyBindValue(KeyBindName.Inventory)))
                {
                    if (infoMenuComponent.IsVisible)
                    {
                        infoMenuComponent.Hide();
                    }
                    else
                    {
                        UIManager.Instance.CloseAllMenus();
                        infoMenuComponent.Show();
                    }
                }

                #region playerInfoMenu
                if (infoMenuComponent.IsVisible || !this._inventoryInitialized)
                {
                    this._inventoryInitialized = true;

                    infoMenuComponent.LevelGroup.Level.text = levelComponent.CurrentLevel.ToString();
                    infoMenuComponent.LevelGroup.SkillPoints.text = levelComponent.SkillPoints.ToString();

                    var addPointButtonAlpha = levelComponent.SkillPoints > 0 ? 1 : 0;

                    this.SetUpStatsText(infoMenuComponent.StatsGroup, healthstatComponent, attackStatComponent,
                                        defenceStatComponent, magicStatComponent, addPointButtonAlpha);


                    this.SetUpEquipedGearButtons(infoMenuComponent.GearGroup, this._equipedItemsGroup, playerEntity);

                    var inventoryGroup = infoMenuComponent.InventoryGroup;
                    var itemCount = this._itemsGroup.CalculateLength();

                    this.SetUpInventoryItems(inventoryGroup, itemCount, playerEntity);
                }

                this.Entities.With(this._addStatButtonEventsGroup).ForEach((Entity eventEntity, ref AddStatButtonPressedComponent eventComponent) =>
                {
                    this.PostUpdateCommands.AddComponent(playerEntity, new AddStatPointComponent
                    {
                        StatTarget = eventComponent.StatTarget
                    });

                    this.PostUpdateCommands.DestroyEntity(eventEntity);
                });

                this.Entities.With(this._inventoryItemButtonEventsGroup).ForEach((Entity eventEntity, ref InventoryItemButtonPressedComponent eventComponent) =>
                {
                    this.LastItemsCount = -1;

                    if (eventComponent.MouseButton == 0)
                        this.PostUpdateCommands.AddComponent(eventComponent.ItemEntity, new UseComponent());
                    else if (eventComponent.MouseButton == 1)
                        this.PostUpdateCommands.AddComponent(eventComponent.ItemEntity, new DestroyComponent());

                    this.PostUpdateCommands.DestroyEntity(eventEntity);
                });
                #endregion
            });
        }

        private void SetUpStatsText(StatsGroupWrapper statsGroup, HealthStatComponent healthstatComponent, AttackStatComponent attackStatComponent, DefenceStatComponent defenceStatComponent, MagicStatComponent magicStatComponent, int addPointButtonAlpha)
        {
            statsGroup.HealthStat.PointsSpent.text = healthstatComponent.CurrentValue.ToString();
            statsGroup.HealthStat.AddButton.GetComponent<CanvasGroup>().alpha = addPointButtonAlpha;

            statsGroup.AttackStat.PointsSpent.text = attackStatComponent.CurrentValue.ToString();
            statsGroup.AttackStat.AddButton.GetComponent<CanvasGroup>().alpha = addPointButtonAlpha;

            statsGroup.DefenceStat.PointsSpent.text = defenceStatComponent.CurrentValue.ToString();
            statsGroup.DefenceStat.AddButton.GetComponent<CanvasGroup>().alpha = addPointButtonAlpha;

            statsGroup.MagicStat.PointsSpent.text = magicStatComponent.CurrentValue.ToString();
            statsGroup.MagicStat.AddButton.GetComponent<CanvasGroup>().alpha = addPointButtonAlpha;
        }

        private void SetUpEquipedGearButtons(EquipedGearGroupWrapper gearGroup, EntityQuery itemsGroup, Entity owner)
        {
            for (var i = 0; i < gearGroup.GearSlots.Length; i++)
            {
                gearGroup.GearSlots[i].ItemIcon.enabled = false;
                gearGroup.GearSlots[i].HasItem = false;
            }

            this.Entities.With(itemsGroup).ForEach((Entity itemEntity, ref ItemComponent itemComponent, ref PickedUpComponent pickedUpComponent) =>
            {
                if (pickedUpComponent.Owner != owner)
                    return;

                var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[itemComponent.StoreIndex];
                if (item.ItemType == ItemType.Gear)
                    this.SetUpGearButton(gearGroup.GearSlots.Where(slot => slot.GearType == item.GearType).First(),
                                         itemEntity, itemComponent);
            });
        }

        private void SetUpGearButton(EquipedGearButton button, Entity itemEntity, ItemComponent itemComponent)
        {
            var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[itemComponent.StoreIndex];
            button.HasItem = true;
            button.ItemIcon.enabled = true;
            button.ItemIcon.sprite = item.Icons[itemComponent.IconIndex];
            button.ItemEntity = itemEntity;
        }

        protected override ItemButton AddInventoryButton(ItemHashValue hashValue, InventoryGroupWrapper inventoryGroup)
        {
            var button = GameObject.Instantiate(inventoryGroup.ItemButtonPrefab, inventoryGroup.Grid.transform).GetComponent<InventoryItemButton>();
            var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[hashValue.ItemComponent.StoreIndex];
            button.ItemEntity = hashValue.Entity;
            button.ItemIcon.enabled = true;
            button.ItemIcon.sprite = item.Icons[hashValue.ItemComponent.IconIndex];
            return button;
        }
    }
}
