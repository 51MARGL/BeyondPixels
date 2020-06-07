using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.SceneBootstraps;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Items
{
    public class DropLootSystem : ComponentSystem
    {
        private EntityQuery _dropGroup;
        private EntityQuery _lootGroup;

        protected override void OnCreate()
        {
            this._dropGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(DropLootComponent),
                    typeof(PositionComponent)
                }
            });
            this._lootGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(ItemComponent),
                    typeof(PickedUpComponent)
                },
                None = new ComponentType[]
                {
                    typeof(DestroyComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            if (this._dropGroup.CalculateEntityCount() == 0)
            {
                return;
            }

            var dropChunks = this._dropGroup.CreateArchetypeChunkArray(Allocator.TempJob);

            for (var c = 0; c < dropChunks.Length; c++)
            {
                var chunk = dropChunks[c];
                var entities = chunk.GetNativeArray(this.GetArchetypeChunkEntityType());
                var positionComponents = chunk.GetNativeArray(this.GetArchetypeChunkComponentType<PositionComponent>());
                if (entities.Length > 0 && positionComponents.Length > 0)
                {
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        var ownerEntity = entities[i];
                        var position = positionComponents[i].CurrentPosition;
                        var itemsList = new NativeList<Entity>(Allocator.TempJob);
                        this.PostUpdateCommands.RemoveComponent<DropLootComponent>(ownerEntity);
                        this.Entities.With(this._lootGroup).ForEach((Entity itemEntity, ref PickedUpComponent pickedUpComponent) =>
                        {
                            if (pickedUpComponent.Owner == ownerEntity)
                            {
                                if (this.EntityManager.HasComponent<EquipedComponent>(itemEntity))
                                {
                                    this.PostUpdateCommands.RemoveComponent<EquipedComponent>(itemEntity);
                                }

                                this.PostUpdateCommands.RemoveComponent<PickedUpComponent>(itemEntity);

                                itemsList.Add(itemEntity);
                            }
                        });
                        if (itemsList.Length > 0)
                        {
                            var lootBag = GameObject.Instantiate(PrefabManager.Instance.LootBag,
                                new Vector3(position.x, position.y - 0.25f, 0), Quaternion.identity);

                            var lootBagEntity = lootBag.GetComponent<GameObjectEntity>().Entity;

                            this.PostUpdateCommands.AddComponent(lootBagEntity, new LootBagComponent());

                            for (var it = 0; it < itemsList.Length; it++)
                            {
                                this.PostUpdateCommands.AddComponent(itemsList[it], new PickedUpComponent
                                {
                                    Owner = lootBagEntity
                                });
                            }
                        }
                        itemsList.Dispose();
                    }
                }
            }
            dropChunks.Dispose();
        }
    }
}
