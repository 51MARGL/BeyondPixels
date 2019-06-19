using System;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.UI.Buttons;
using Unity.Collections;
using Unity.Entities;

using UnityEngine;
using static BeyondPixels.UI.ECS.Components.PlayerInfoMenuUIComponent;

namespace BeyondPixels.UI.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public abstract class ItemSorterSystem : ComponentSystem
    {
        protected struct ItemHashValue
        {
            public Entity Entity;
            public ItemComponent ItemComponent;
        }

        protected int LastItemsCount;
        protected Entity LastOwnerEntity;
        protected ComponentGroup _itemsGroup;

        protected override void OnCreateManager()
        {
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
        }

        protected override void OnUpdate()
        {

        }

        protected virtual int SetUpInventoryItems(InventoryGroupWrapper inventoryGroup, int itemCount, Entity owner)
        {
            var foodList = new NativeList<ItemHashValue>(Allocator.Temp);
            var potionList = new NativeList<ItemHashValue>(Allocator.Temp);
            var treasureList = new NativeList<ItemHashValue>(Allocator.Temp);
            var weaponList = new NativeList<ItemHashValue>(Allocator.Temp);
            var magicWeaponList = new NativeList<ItemHashValue>(Allocator.Temp);
            var helmetList = new NativeList<ItemHashValue>(Allocator.Temp);
            var chestList = new NativeList<ItemHashValue>(Allocator.Temp);
            var bootsList = new NativeList<ItemHashValue>(Allocator.Temp);

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

            if (itemCounter != this.LastItemsCount || this.LastOwnerEntity != owner)
            {
                this.LastItemsCount = itemCounter;
                this.LastOwnerEntity = owner;

                for (var i = inventoryGroup.Grid.transform.childCount - 1; i >= 0; i--)
                    GameObject.Destroy(inventoryGroup.Grid.transform.GetChild(i).gameObject);

                var itemsHashMap = new NativeMultiHashMap<int, ItemHashValue>(foodList.Length + potionList.Length + treasureList.Length, Allocator.Temp);
                for (var i = 0; i < foodList.Length; i++)
                    itemsHashMap.Add(this.GetItemHash(foodList[i]), foodList[i]);
                for (var i = 0; i < potionList.Length; i++)
                    itemsHashMap.Add(this.GetItemHash(potionList[i]), potionList[i]);
                for (var i = 0; i < treasureList.Length; i++)
                    itemsHashMap.Add(this.GetItemHash(treasureList[i]), treasureList[i]);

                if (itemsHashMap.Length > 0)
                {
                    var iterator = new NativeMultiHashMapIterator<int>();
                    var (keys, keysLength) = itemsHashMap.GetUniqueKeyArray(Allocator.Temp);
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
            }

            foodList.Dispose();
            potionList.Dispose();
            treasureList.Dispose();
            weaponList.Dispose();
            magicWeaponList.Dispose();
            helmetList.Dispose();
            chestList.Dispose();
            bootsList.Dispose();

            return itemCounter;
        }

        protected int GetItemHash(ItemHashValue hashValue)
        {
            var item = 
                ItemsManagerComponent.Instance.ItemsStoreComponent.Items[hashValue.ItemComponent.StoreIndex];

            return (item.Name + hashValue.ItemComponent.IconIndex).GetHashCode();
        }

        protected abstract ItemButton AddInventoryButton(ItemHashValue hashValue, InventoryGroupWrapper inventoryGroup);
    }
}
