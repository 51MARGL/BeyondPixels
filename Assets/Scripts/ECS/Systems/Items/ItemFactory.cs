using System;
using BeyondPixels.ECS.Components.Items;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Items
{
    public class ItemFactory
    {
        public static Entity GetRandomFood()
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());

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

        public static Entity GetHealthPotion()
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());

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

        public static Entity GetRandomWeapon(int level)
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());

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

            if (random.NextInt(0, 100) > 50)
            {
                var randomStat = random.NextInt(0, 3);
                if (randomStat == 0)
                {
                    entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });
                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
                else if (randomStat == 1)
                {
                    entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });
                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
                else if (randomStat == 2)
                {
                    entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });

                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
            }

            return itemEntity;
        }

        public static Entity GetRandomMagicWeapon(int level)
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());

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
            entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent 
            {
                Value = random.NextInt(1, 6)
            });

            if (random.NextInt(0, 100) > 50)
            {
                var randomStat = random.NextInt(0, 3);
                if (randomStat == 0)
                {
                    entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });
                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
                else if (randomStat == 1)
                {
                    entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });
                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
                else if (randomStat == 2)
                {
                    entityManager.AddComponentData(itemEntity, new DefenceStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });

                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
            }

            return itemEntity;
        }

        public static Entity GetRandomHelmet(int level)
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());

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

            if (random.NextInt(0, 100) > 50)
            {
                var randomStat = random.NextInt(0, 3);
                if (randomStat == 0)
                {
                    entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });
                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
                else if (randomStat == 1)
                {
                    entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });
                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
                else if (randomStat == 2)
                {
                    entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });

                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
            }

            return itemEntity;
        }

        public static Entity GetRandomChest(int level)
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());

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

            if (random.NextInt(0, 100) > 50)
            {
                var randomStat = random.NextInt(0, 3);
                if (randomStat == 0)
                {
                    entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });
                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
                else if (randomStat == 1)
                {
                    entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });
                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
                else if (randomStat == 2)
                {
                    entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });

                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
            }

            return itemEntity;
        }

        public static Entity GetRandomBoots(int level)
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());

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

            if (random.NextInt(0, 100) > 50)
            {
                var randomStat = random.NextInt(0, 3);
                if (randomStat == 0)
                {
                    entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });
                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
                else if (randomStat == 1)
                {
                    entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });
                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
                else if (randomStat == 2)
                {
                    entityManager.AddComponentData(itemEntity, new AttackStatModifierComponent
                    {
                        Value = random.NextInt(1, 4)
                    });

                    if (random.NextInt(0, 100) > 75)
                    {
                        randomStat = random.NextInt(0, 2);
                        if (randomStat == 0)
                        {
                            entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                        else if (randomStat == 1)
                        {
                            entityManager.AddComponentData(itemEntity, new HealthStatModifierComponent
                            {
                                Value = random.NextInt(1, 4)
                            });
                            if (random.NextInt(0, 100) > 90)
                                entityManager.AddComponentData(itemEntity, new MagicStatModifierComponent
                                {
                                    Value = random.NextInt(1, 4)
                                });
                        }
                    }
                }
            }

            return itemEntity;
        }
    }
}
