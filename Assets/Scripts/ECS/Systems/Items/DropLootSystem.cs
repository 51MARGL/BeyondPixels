using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.SceneBootstraps;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Level
{
    public class DropLootSystem : ComponentSystem
    {
        private ComponentGroup _dropGroup;
        private ComponentGroup _lootGroup;

        protected override void OnCreateManager()
        {
            this._dropGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(DropLootComponent)
                }
            });
            this._lootGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(ItemComponent),
                    typeof(PickedUpComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            if (this._dropGroup.CalculateLength() == 0)
                return;

            var dropChunks = this._dropGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());

            for (var c = 0; c < dropChunks.Length; c++)
            {
                var chunk = dropChunks[c];
                var entities = chunk.GetNativeArray(this.GetArchetypeChunkEntityType());
                var positionComponents = chunk.GetNativeArray(this.GetArchetypeChunkComponentType<PositionComponent>());
                if (entities.Length > 0 && positionComponents.Length > 0)
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
                                if (random.NextInt(0, 100) > 75)
                                {
                                    if (this.EntityManager.HasComponent<EquipedComponent>(itemEntity))
                                        this.PostUpdateCommands.RemoveComponent<EquipedComponent>(itemEntity);

                                    this.PostUpdateCommands.RemoveComponent<PickedUpComponent>(itemEntity);

                                    itemsList.Add(itemEntity);
                                }
                                else
                                {
                                    this.PostUpdateCommands.AddComponent(itemEntity, new DestroyComponent());
                                }
                            }
                        });
                        if (itemsList.Length > 0)
                        {
                            var lootBag = GameObject.Instantiate(PrefabManager.Instance.LootBag,
                                new Vector3(position.x, position.y, 0), Quaternion.identity);

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
                    }
            }
            dropChunks.Dispose();
        }
    }
}
