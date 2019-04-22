using System;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.UI.Buttons;
using BeyondPixels.UI.ECS.Components;
using Unity.Collections;
using Unity.Entities;

using UnityEngine;
using static BeyondPixels.UI.ECS.Components.LootBagMenuUIComponent;

namespace BeyondPixels.UI.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class LootBagMenuUISystem : ComponentSystem
    {
        private struct ItemHashValue
        {
            public Entity Entity;
            public ItemComponent ItemComponent;
        }

        private int LastItemsCount;
        private Entity LastBagEntity;
        private ComponentGroup _bagGroup;
        private ComponentGroup _playerGroup;
        private ComponentGroup _itemsGroup;
        private ComponentGroup _lootButtonEventsGroup;

        protected override void OnCreateManager()
        {
            this._bagGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(LootBagComponent), typeof(OpenLootBagComponent)
                }
            });
            this._itemsGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(ItemComponent),
                    typeof(PickedUpComponent)
                },
                None = new ComponentType[] {
                    typeof(EquipedComponent),
                }
            });
            this._playerGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(PlayerComponent)
                },
                None = new ComponentType[] {
                    typeof(InCutsceneComponent),
                }
            });
            this._lootButtonEventsGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(LootItemButtonPressedComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._bagGroup).ForEach((Entity bagEntity,
                ref OpenLootBagComponent openLootBagComponent) =>
            {
                var lootMenuComponent = UIManager.Instance.LootBagMenuUIComponent;

                if (openLootBagComponent.IsOpened == 0
                    && lootMenuComponent.GetComponent<CanvasGroup>().alpha == 0)
                {
                    UIManager.Instance.CloseAllMenus();
                    lootMenuComponent.GetComponent<CanvasGroup>().alpha = 1;
                    lootMenuComponent.GetComponent<CanvasGroup>().blocksRaycasts = true;
                    openLootBagComponent.IsOpened = 1;
                }
                else if (openLootBagComponent.IsOpened == 1
                        && lootMenuComponent.GetComponent<CanvasGroup>().alpha == 0)
                {
                    this.PostUpdateCommands.RemoveComponent<OpenLootBagComponent>(bagEntity);
                    return;
                }

                if (lootMenuComponent.GetComponent<CanvasGroup>().alpha == 1)
                {
                    var inventoryGroup = lootMenuComponent.LootGroup;
                    var itemCount = this._itemsGroup.CalculateLength();
                    this.SetUpInventoryItems(inventoryGroup, itemCount, bagEntity);
                }
            });

            this.Entities.With(this._lootButtonEventsGroup).ForEach((Entity eventEntity, ref LootItemButtonPressedComponent eventComponent) =>
            {
                this.LastItemsCount = -1;

                var eComponent = eventComponent;
                this.Entities.With(this._playerGroup).ForEach((Entity playerEntity) =>
                {
                    var pickupEventEntity = this.PostUpdateCommands.CreateEntity();
                    this.PostUpdateCommands.RemoveComponent<PickedUpComponent>(eComponent.ItemEntity);
                    this.PostUpdateCommands.AddComponent(pickupEventEntity, new PickUpComponent
                    {
                        ItemEntity = eComponent.ItemEntity,
                        Owner = playerEntity
                    });
                });

                this.PostUpdateCommands.DestroyEntity(eventEntity);
            });
        }

        private void SetUpInventoryItems(LootGroupWrapper inventoryGroup, int itemCount, Entity owner)
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
            this.Entities.With(this._itemsGroup).ForEach((Entity itemEntity, ref ItemComponent itemComponent, ref PickedUpComponent pickedUpComponent) =>
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

            if (itemCounter == 0)
            {
                UIManager.Instance.LootBagMenuUIComponent.GetComponent<CanvasGroup>().alpha = 0;
                UIManager.Instance.LootBagMenuUIComponent.GetComponent<CanvasGroup>().blocksRaycasts = false;

                this.PostUpdateCommands.AddComponent(owner, new DestroyComponent());
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

            if (itemCounter == this.LastItemsCount && this.LastBagEntity == owner)
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
            this.LastBagEntity = owner;

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

        private LootItemButton AddInventoryButton(ItemHashValue hashValue, LootGroupWrapper inventoryGroup)
        {
            var button = GameObject.Instantiate(inventoryGroup.ItemButtonPrefab, inventoryGroup.Grid.transform).GetComponent<LootItemButton>();
            var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[hashValue.ItemComponent.StoreIndex];
            button.ItemEntity = hashValue.Entity;
            button.ItemIcon.enabled = true;
            button.ItemIcon.sprite = item.Icons[hashValue.ItemComponent.IconIndex];
            return button;
        }
    }
}
