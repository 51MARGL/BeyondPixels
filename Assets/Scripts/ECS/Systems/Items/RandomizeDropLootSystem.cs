﻿using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Objects;
using Unity.Collections;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Items
{
    [UpdateBefore(typeof(DropLootSystem))]
    public class RandomizeDropLootSystem : ComponentSystem
    {
        private EntityQuery _dropGroup;
        private EntityQuery _lootGroup;
        private Unity.Mathematics.Random _random;

        protected override void OnCreate()
        {
            this._random = new Unity.Mathematics.Random((uint)System.Guid.NewGuid().GetHashCode());

            this._dropGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(DropLootComponent),
                    typeof(CharacterComponent)
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
                return;

            var dropChunks = this._dropGroup.CreateArchetypeChunkArray(Allocator.TempJob);

            for (var c = 0; c < dropChunks.Length; c++)
            {
                var chunk = dropChunks[c];
                var entities = chunk.GetNativeArray(this.GetArchetypeChunkEntityType());
                if (entities.Length > 0)
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        var ownerEntity = entities[i];
                        this.Entities.With(this._lootGroup).ForEach((Entity itemEntity, ref PickedUpComponent pickedUpComponent) =>
                        {
                            if (pickedUpComponent.Owner == ownerEntity && _random.NextInt(0, 100) < 85)
                                this.PostUpdateCommands.AddComponent(itemEntity, new DestroyComponent());
                        });
                    }
            }
            dropChunks.Dispose();
        }
    }
}
