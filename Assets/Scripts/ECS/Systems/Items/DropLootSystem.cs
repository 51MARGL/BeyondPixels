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
        private ComponentGroup _destroyGroup;
        private ComponentGroup _lootGroup;

        protected override void OnCreateManager()
        {
            this._destroyGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(DestroyComponent),
                    typeof(CharacterComponent),
                    typeof(PositionComponent)
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
            if (this._destroyGroup.CalculateLength() == 0)
                return;

            var destroyChunks = this._destroyGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());

            for (var c = 0; c < destroyChunks.Length; c++)
            {
                var chunk = destroyChunks[c];
                var entities = chunk.GetNativeArray(this.GetArchetypeChunkEntityType());
                var positionComponents = chunk.GetNativeArray(this.GetArchetypeChunkComponentType<PositionComponent>());
                for (var i = 0; i < chunk.Count; i++)
                {
                    var ownerEntity = entities[i];
                    var position = positionComponents[i].CurrentPosition;
                    var itemsList = new NativeList<Entity>(Allocator.TempJob);

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
            destroyChunks.Dispose();
        }
    }
}
