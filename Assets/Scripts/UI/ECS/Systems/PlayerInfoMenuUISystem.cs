using System;
using System.Linq;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.UI.Buttons;
using BeyondPixels.UI.ECS.Components;
using Unity.Collections;
using Unity.Entities;

using UnityEngine;
using static BeyondPixels.UI.ECS.Components.PlayerInfoMenuUIComponent;

namespace BeyondPixels.UI.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class PlayerInfoMenuUISystem : ComponentSystem
    {
        private struct ItemHashValue
        {
            public Entity Entity;
            public ItemComponent ItemComponent;
        }

        private int LastItemsCount;
        private ComponentGroup _playerGroup;
        private ComponentGroup _equipedItemsGroup;
        private ComponentGroup _inventoryItemsGroup;
        private ComponentGroup _addStatButtonEventsGroup;
        private ComponentGroup _inventoryItemButtonEventsGroup;

        protected override void OnCreateManager()
        {
            this._playerGroup = this.GetComponentGroup(new EntityArchetypeQuery
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
            this._equipedItemsGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(ItemComponent),
                    typeof(PickedUpComponent),
                    typeof(EquipedComponent)
                }
            });
            this._inventoryItemsGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(ItemComponent),
                    typeof(PickedUpComponent)
                },
                None = new ComponentType[] {
                    typeof(EquipedComponent),
                }
            });
            this._addStatButtonEventsGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(AddStatButtonPressedComponent)
                }
            });
            this._inventoryItemButtonEventsGroup = this.GetComponentGroup(new EntityArchetypeQuery
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
                if (Input.GetKeyDown(KeyCode.I))
                {
                    if (infoMenuComponent.GetComponent<CanvasGroup>().alpha == 1)
                    {
                        infoMenuComponent.GetComponent<CanvasGroup>().alpha = 0;
                        infoMenuComponent.GetComponent<CanvasGroup>().blocksRaycasts = false;
                    }
                    else
                    {
                        UIManager.Instance.CloseAllMenus();
                        infoMenuComponent.GetComponent<CanvasGroup>().alpha = 1;
                        infoMenuComponent.GetComponent<CanvasGroup>().blocksRaycasts = true;
                    }
                }

                #region playerInfoMenu
                if (infoMenuComponent.GetComponent<CanvasGroup>().alpha == 1)
                {
                    infoMenuComponent.LevelGroup.Level.text = levelComponent.CurrentLevel.ToString();
                    infoMenuComponent.LevelGroup.SkillPoints.text = levelComponent.SkillPoints.ToString();

                    var addPointButtonAlpha = levelComponent.SkillPoints > 0 ? 1 : 0;

                    this.SetUpStatsText(infoMenuComponent.StatsGroup, healthstatComponent, attackStatComponent,
                                        defenceStatComponent, magicStatComponent, addPointButtonAlpha);


                    this.SetUpEquipedGearButtons(infoMenuComponent.GearGroup, this._equipedItemsGroup, playerEntity);

                    var inventoryGroup = infoMenuComponent.InventoryGroup;
                    var itemCount = this._inventoryItemsGroup.CalculateLength();

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

        private void SetUpEquipedGearButtons(EquipedGearGroupWrapper gearGroup, ComponentGroup itemsGroup, Entity owner)
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

        private void SetUpInventoryItems(InventoryGroupWrapper inventoryGroup, int itemCount, Entity owner)
        {
            var foodList = new NativeList<ItemHashValue>(Allocator.TempJob);
            var potionList = new NativeList<ItemHashValue>(Allocator.TempJob);
            var treasureList = new NativeList<ItemHashValue>(Allocator.TempJob);
            var weaponList = new NativeList<ItemHashValue>(Allocator.TempJob);
            var magicWeaponList = new NativeList<ItemHashValue>(Allocator.TempJob);
            var helmetList = new NativeList<ItemHashValue>(Allocator.TempJob);
            var chestList = new NativeList<ItemHashValue>(Allocator.TempJob);
            var bootsList = new NativeList<ItemHashValue>(Allocator.TempJob);

            var itemCounter = 0;
            this.Entities.With(this._inventoryItemsGroup).ForEach((Entity itemEntity, ref ItemComponent itemComponent, ref PickedUpComponent pickedUpComponent) =>
            {
                if (pickedUpComponent.Owner != owner)
                    return;

                itemCounter++;
                var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[itemComponent.StoreIndex];
                switch (item.ItemType)
                {
                    case ItemType.Food:
                        foodList.Add(
                            new ItemHashValue
                            {
                                Entity = itemEntity,
                                ItemComponent = itemComponent
                            });
                        break;
                    case ItemType.Potion:
                        potionList.Add(
                            new ItemHashValue
                            {
                                Entity = itemEntity,
                                ItemComponent = itemComponent
                            });
                        break;
                    case ItemType.Treasure:
                        treasureList.Add(
                            new ItemHashValue
                            {
                                Entity = itemEntity,
                                ItemComponent = itemComponent
                            });
                        break;
                    case ItemType.Gear:
                        switch (item.GearType)
                        {
                            case GearType.Helmet:
                                helmetList.Add(
                                    new ItemHashValue
                                    {
                                        Entity = itemEntity,
                                        ItemComponent = itemComponent
                                    });
                                break;
                            case GearType.Chest:
                                chestList.Add(
                                    new ItemHashValue
                                    {
                                        Entity = itemEntity,
                                        ItemComponent = itemComponent
                                    });
                                break;
                            case GearType.Weapon:
                                weaponList.Add(
                                    new ItemHashValue
                                    {
                                        Entity = itemEntity,
                                        ItemComponent = itemComponent
                                    });
                                break;
                            case GearType.Boots:
                                bootsList.Add(
                                    new ItemHashValue
                                    {
                                        Entity = itemEntity,
                                        ItemComponent = itemComponent
                                    });
                                break;
                            case GearType.Magic:
                                magicWeaponList.Add(
                                    new ItemHashValue
                                    {
                                        Entity = itemEntity,
                                        ItemComponent = itemComponent
                                    });
                                break;
                        }
                        break;
                }
            });

            if (itemCounter == this.LastItemsCount)
            {
                foodList.Dispose();
                potionList.Dispose();
                treasureList.Dispose();
                weaponList.Dispose();
                magicWeaponList.Dispose();
                helmetList.Dispose();
                chestList.Dispose();
                bootsList.Dispose();

                return;
            }

            this.LastItemsCount = itemCounter;

            for (var i = inventoryGroup.Grid.transform.childCount - 1; i >= 0; i--)
                GameObject.Destroy(inventoryGroup.Grid.transform.GetChild(i).gameObject);

            var itemsHashMap = new NativeMultiHashMap<int, ItemHashValue>(foodList.Length + potionList.Length + treasureList.Length, Allocator.TempJob);
            for (var i = 0; i < foodList.Length; i++)
                itemsHashMap.Add(foodList[i].ItemComponent.StoreIndex
                    + foodList[i].ItemComponent.IconIndex,
                    foodList[i]);
            for (var i = 0; i < potionList.Length; i++)
                itemsHashMap.Add(foodList.Length + 1 + potionList[i].ItemComponent.StoreIndex
                    + potionList[i].ItemComponent.IconIndex,
                    potionList[i]);
            for (var i = 0; i < treasureList.Length; i++)
                itemsHashMap.Add(potionList.Length + foodList.Length + 1 + treasureList[i].ItemComponent.StoreIndex
                    + treasureList[i].ItemComponent.IconIndex,
                    treasureList[i]);

            if (itemsHashMap.Length > 0)
            {
                var iterator = new NativeMultiHashMapIterator<int>();
                var (keys, keysLength) = itemsHashMap.GetUniqueKeyArray(Allocator.TempJob);
                for (var keyI = 0; keyI < keysLength; keyI++)
                {
                    if (!itemsHashMap.TryGetFirstValue(keys[keyI], out var hashValue, out iterator))
                        continue;

                    var button = this.AddInventoryButton(hashValue, inventoryGroup);

                    var itemsCount = 1;
                    while (itemsHashMap.TryGetNextValue(out hashValue, ref iterator))
                        itemsCount++;

                    if (itemsCount > 1)
                        button.Amount.text = itemsCount.ToString();
                    else
                        button.Amount.text = string.Empty;

                }
                keys.Dispose();
            }
            itemsHashMap.Dispose();

            for (var i = 0; i < weaponList.Length; i++)
                this.AddInventoryButton(weaponList[i], inventoryGroup);
            for (var i = 0; i < magicWeaponList.Length; i++)
                this.AddInventoryButton(magicWeaponList[i], inventoryGroup);
            for (var i = 0; i < helmetList.Length; i++)
                this.AddInventoryButton(helmetList[i], inventoryGroup);
            for (var i = 0; i < chestList.Length; i++)
                this.AddInventoryButton(chestList[i], inventoryGroup);
            for (var i = 0; i < bootsList.Length; i++)
                this.AddInventoryButton(bootsList[i], inventoryGroup);

            foodList.Dispose();
            potionList.Dispose();
            treasureList.Dispose();
            weaponList.Dispose();
            magicWeaponList.Dispose();
            helmetList.Dispose();
            chestList.Dispose();
            bootsList.Dispose();
        }

        private InventoryItemButton AddInventoryButton(ItemHashValue hashValue, InventoryGroupWrapper inventoryGroup)
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
