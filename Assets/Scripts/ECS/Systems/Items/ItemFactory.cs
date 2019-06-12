using System;
using BeyondPixels.ECS.Components.Items;
using Unity.Collections;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Items
{
    public class ItemFactory
    {
        private enum WeaponStatsModifiers
        {
            Attack = 1,
            Defence = 2,
            Magick = 3,
            Health = 4
        }

        public static Entity GetRandomFood(ref Unity.Mathematics.Random random)
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            var itemEntity = entityManager.CreateEntity();
            var storeIndex = Array.FindIndex(ItemsManagerComponent.Instance.ItemsStoreComponent.Items,
                            item => item.ItemType == ItemType.Food);
            var iconIndex = random.NextInt(0,
                ItemsManagerComponent.Instance.ItemsStoreComponent.Items[storeIndex].Icons.Length);

            entityManager.AddComponentData(itemEntity, new ItemComponent
            {
                StoreIndex = storeIndex,
                IconIndex = iconIndex,
                Level = 1
            });

            return itemEntity;
        }

        public static Entity GetHealthPotion(ref Unity.Mathematics.Random random)
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            var itemEntity = entityManager.CreateEntity();
            var storeIndex = Array.FindIndex(ItemsManagerComponent.Instance.ItemsStoreComponent.Items,
                            item => item.ItemType == ItemType.Potion);
            var iconIndex = random.NextInt(0,
                ItemsManagerComponent.Instance.ItemsStoreComponent.Items[storeIndex].Icons.Length);

            entityManager.AddComponentData(itemEntity, new ItemComponent
            {
                StoreIndex = storeIndex,
                IconIndex = iconIndex,
                Level = 1
            });

            return itemEntity;
        }

        public static Entity GetRandomWeapon(int level, ref Unity.Mathematics.Random random)
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            var itemEntity = entityManager.CreateEntity();
            var storeIndex = Array.FindIndex(ItemsManagerComponent.Instance.ItemsStoreComponent.Items,
                            item => item.ItemType == ItemType.Gear && item.GearType == GearType.Weapon);
            var iconIndex = random.NextInt(0,
                ItemsManagerComponent.Instance.ItemsStoreComponent.Items[storeIndex].Icons.Length);

            entityManager.AddComponentData(itemEntity, new ItemComponent
            {
                StoreIndex = storeIndex,
                IconIndex = iconIndex,
                Level = level
            });
            entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
            {
                Value = random.NextInt(1, 6)
            });

            var randomStatModifiersArray = new NativeArray<WeaponStatsModifiers>(3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            randomStatModifiersArray[0] = WeaponStatsModifiers.Defence;
            randomStatModifiersArray[1] = WeaponStatsModifiers.Magick;
            randomStatModifiersArray[2] = WeaponStatsModifiers.Health;
            AddRandomStats(itemEntity, entityManager, randomStatModifiersArray, ref random);

            return itemEntity;
        }

        public static Entity GetRandomMagicWeapon(int level, ref Unity.Mathematics.Random random)
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            var itemEntity = entityManager.CreateEntity();
            var storeIndex = Array.FindIndex(ItemsManagerComponent.Instance.ItemsStoreComponent.Items,
                            item => item.ItemType == ItemType.Gear && item.GearType == GearType.Magic);
            var iconIndex = random.NextInt(0,
                ItemsManagerComponent.Instance.ItemsStoreComponent.Items[storeIndex].Icons.Length);

            entityManager.AddComponentData(itemEntity, new ItemComponent
            {
                StoreIndex = storeIndex,
                IconIndex = iconIndex,
                Level = level
            });
            entityManager.AddComponentData(itemEntity, new MagickStatModifierComponent 
            {
                Value = random.NextInt(1, 6)
            });

            var randomStatModifiersArray = new NativeArray<WeaponStatsModifiers>(3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            randomStatModifiersArray[0] = WeaponStatsModifiers.Defence;
            randomStatModifiersArray[1] = WeaponStatsModifiers.Attack;
            randomStatModifiersArray[2] = WeaponStatsModifiers.Health;
            AddRandomStats(itemEntity, entityManager, randomStatModifiersArray, ref random);

            return itemEntity;
        }

        public static Entity GetRandomHelmet(int level, ref Unity.Mathematics.Random random)
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            var itemEntity = entityManager.CreateEntity();
            var storeIndex = Array.FindIndex(ItemsManagerComponent.Instance.ItemsStoreComponent.Items,
                            item => item.ItemType == ItemType.Gear && item.GearType == GearType.Helmet);
            var iconIndex = random.NextInt(0,
                ItemsManagerComponent.Instance.ItemsStoreComponent.Items[storeIndex].Icons.Length);

            entityManager.AddComponentData(itemEntity, new ItemComponent
            {
                StoreIndex = storeIndex,
                IconIndex = iconIndex,
                Level = level
            });
            entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
            {
                Value = random.NextInt(1, 6)
            });

            var randomStatModifiersArray = new NativeArray<WeaponStatsModifiers>(3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            randomStatModifiersArray[0] = WeaponStatsModifiers.Magick;
            randomStatModifiersArray[1] = WeaponStatsModifiers.Attack;
            randomStatModifiersArray[2] = WeaponStatsModifiers.Health;
            AddRandomStats(itemEntity, entityManager, randomStatModifiersArray, ref random);

            return itemEntity;
        }

        public static Entity GetRandomChest(int level, ref Unity.Mathematics.Random random)
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            var itemEntity = entityManager.CreateEntity();
            var storeIndex = Array.FindIndex(ItemsManagerComponent.Instance.ItemsStoreComponent.Items,
                            item => item.ItemType == ItemType.Gear && item.GearType == GearType.Chest);
            var iconIndex = random.NextInt(0,
                ItemsManagerComponent.Instance.ItemsStoreComponent.Items[storeIndex].Icons.Length);

            entityManager.AddComponentData(itemEntity, new ItemComponent
            {
                StoreIndex = storeIndex,
                IconIndex = iconIndex,
                Level = level
            });
            entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
            {
                Value = random.NextInt(1, 6)
            });

            var randomStatModifiersArray = new NativeArray<WeaponStatsModifiers>(3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            randomStatModifiersArray[0] = WeaponStatsModifiers.Magick;
            randomStatModifiersArray[1] = WeaponStatsModifiers.Attack;
            randomStatModifiersArray[2] = WeaponStatsModifiers.Health;
            AddRandomStats(itemEntity, entityManager, randomStatModifiersArray, ref random);

            return itemEntity;
        }

        public static Entity GetRandomBoots(int level, ref Unity.Mathematics.Random random)
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            var itemEntity = entityManager.CreateEntity();
            var storeIndex = Array.FindIndex(ItemsManagerComponent.Instance.ItemsStoreComponent.Items,
                            item => item.ItemType == ItemType.Gear && item.GearType == GearType.Boots);
            var iconIndex = random.NextInt(0,
                ItemsManagerComponent.Instance.ItemsStoreComponent.Items[storeIndex].Icons.Length);

            entityManager.AddComponentData(itemEntity, new ItemComponent
            {
                StoreIndex = storeIndex,
                IconIndex = iconIndex,
                Level = level
            });
            entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
            {
                Value = random.NextInt(1, 6)
            });

            var randomStatModifiersArray = new NativeArray<WeaponStatsModifiers>(3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            randomStatModifiersArray[0] = WeaponStatsModifiers.Magick;
            randomStatModifiersArray[1] = WeaponStatsModifiers.Attack;
            randomStatModifiersArray[2] = WeaponStatsModifiers.Health;
            AddRandomStats(itemEntity, entityManager, randomStatModifiersArray, ref random);

            return itemEntity;
        }

        private static void ShuffleStatsArray(NativeArray<WeaponStatsModifiers> array, ref Unity.Mathematics.Random random)
        {
            for (int i = 0; i < array.Length; i++)
            {
                var rIndex = random.NextInt(0, array.Length);
                var tmp = array[rIndex];
                array[rIndex] = array[i];
                array[i] = tmp;
            }
        }

        private static void AddRandomStats(Entity itemEntity, EntityManager entityManager, NativeArray<WeaponStatsModifiers> randomStatModifiersArray, ref Unity.Mathematics.Random random)
        {
            var chance = 50;
            ShuffleStatsArray(randomStatModifiersArray, ref random);
            for (int i = 0; i < randomStatModifiersArray.Length; i++)
            {
                if (random.NextInt(0, 100) > chance)
                {
                    chance += 20;
                    switch (randomStatModifiersArray[i])
                    {
                        case WeaponStatsModifiers.Attack:
                            entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            break;
                        case WeaponStatsModifiers.Defence:
                            entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            break;
                        case WeaponStatsModifiers.Magick:
                            entityManager.AddComponentData(itemEntity, new MagickStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            break;
                        case WeaponStatsModifiers.Health:
                            entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            break;
                    }
                }
                else
                    return;
            }
        }
    }
}
