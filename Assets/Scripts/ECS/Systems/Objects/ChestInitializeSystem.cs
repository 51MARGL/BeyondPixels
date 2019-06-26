using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Systems.Items;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Objects
{
    public class ChestInitializeSystem : ComponentSystem
    {
        private EntityQuery _group;

        protected override void OnCreateManager()
        {
            this._group = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(ChestInitializeComponent), typeof(Transform)
                },
                None = new ComponentType[]
                {
                    typeof(PositionComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());
            var playerEntity = GameObject.FindGameObjectWithTag("Player").GetComponent<GameObjectEntity>().Entity;

            if (!this.EntityManager.HasComponent<LevelComponent>(playerEntity))
                return;

            var playerLvlComponent = this.EntityManager.GetComponentData<LevelComponent>(playerEntity);

            this.Entities.With(this._group).ForEach((Entity entity,
                ChestInitializeComponent chestInitializeComponent,
                Transform transform) =>
            {
                this.PostUpdateCommands.AddComponent(entity, new PositionComponent
                {
                    CurrentPosition = new float2(transform.position.x, transform.position.y),
                    InitialPosition = new float2(transform.position.x, transform.position.y)
                });
                this.PostUpdateCommands.AddComponent(entity, new XPRewardComponent
                {
                    XPAmount = chestInitializeComponent.XPAmount
                });

                int currLevel = playerLvlComponent.CurrentLevel == 1 ? 1 :
                                    random.NextInt(playerLvlComponent.CurrentLevel,
                                                   playerLvlComponent.CurrentLevel + 3);

                var lvlComponent = new LevelComponent
                {
                    CurrentLevel = currLevel
                };
                this.PostUpdateCommands.AddComponent(entity, lvlComponent);


                #region items
                if (random.NextInt(0, 100) > 50)
                {
                    var weaponEntity = ItemFactory.GetRandomWeapon(lvlComponent.CurrentLevel, ref random, this.PostUpdateCommands);
                    this.PostUpdateCommands.AddComponent(weaponEntity, new PickedUpComponent
                    {
                        Owner = entity
                    });
                }
                if (random.NextInt(0, 100) > 50)
                {
                    var spellBookEntity = ItemFactory.GetRandomMagicWeapon(lvlComponent.CurrentLevel, ref random, this.PostUpdateCommands);
                    this.PostUpdateCommands.AddComponent(spellBookEntity, new PickedUpComponent
                    {
                        Owner = entity
                    });
                }
                if (random.NextInt(0, 100) > 50)
                {
                    var helmetEntity = ItemFactory.GetRandomHelmet(lvlComponent.CurrentLevel, ref random, this.PostUpdateCommands);
                    this.PostUpdateCommands.AddComponent(helmetEntity, new PickedUpComponent
                    {
                        Owner = entity
                    });
                }
                if (random.NextInt(0, 100) > 50)
                {
                    var chestEntity = ItemFactory.GetRandomChest(lvlComponent.CurrentLevel, ref random, this.PostUpdateCommands);
                    this.PostUpdateCommands.AddComponent(chestEntity, new PickedUpComponent
                    {
                        Owner = entity
                    });
                }
                if (random.NextInt(0, 100) > 50)
                {
                    var bootsEntity = ItemFactory.GetRandomBoots(lvlComponent.CurrentLevel, ref random, this.PostUpdateCommands);
                    this.PostUpdateCommands.AddComponent(bootsEntity, new PickedUpComponent
                    {
                        Owner = entity
                    });
                }
                if (random.NextInt(0, 100) > 25)
                {
                    var randomCount = random.NextInt(1, 3);
                    for (var i = 0; i < randomCount; i++)
                    {
                        var foodEntity = ItemFactory.GetRandomFood(ref random, this.PostUpdateCommands);
                        this.PostUpdateCommands.AddComponent(foodEntity, new PickedUpComponent
                        {
                            Owner = entity
                        });
                    }
                }
                if (random.NextInt(0, 100) > 25)
                {
                    var randomCount = random.NextInt(1, 3);
                    for (var i = 0; i < randomCount; i++)
                    {
                        var potionEntity = ItemFactory.GetHealthPotion(ref random, this.PostUpdateCommands);
                        this.PostUpdateCommands.AddComponent(potionEntity, new PickedUpComponent
                        {
                            Owner = entity
                        });
                    }
                }
                #endregion

                GameObject.Destroy(chestInitializeComponent);
            });
        }
    }
}
