﻿using System.Collections.Generic;

using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.SaveGame;

using Unity.Entities;

namespace BeyondPixels.ECS.Systems.SaveGame
{
    public class SaveGameSystem : ComponentSystem
    {
        private EntityQuery _saveGroup;

        protected override void OnCreateManager()
        {
            this._saveGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(SaveGameComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._saveGroup).ForEach((Entity entity) =>
            {
                SaveData playerData = null;
                this.Entities.WithAll<PlayerComponent>().ForEach((Entity playerEntity) =>
                {
                    playerData = new SaveData
                    {
                        LevelComponent = this.EntityManager.GetComponentData<LevelComponent>(playerEntity),
                        HealthComponent = this.EntityManager.GetComponentData<HealthComponent>(playerEntity),
                        XPComponent = this.EntityManager.GetComponentData<XPComponent>(playerEntity),
                        HealthStatComponent = this.EntityManager.GetComponentData<HealthStatComponent>(playerEntity),
                        AttackStatComponent = this.EntityManager.GetComponentData<AttackStatComponent>(playerEntity),
                        DefenceStatComponent = this.EntityManager.GetComponentData<DefenceStatComponent>(playerEntity),
                        MagicStatComponent = this.EntityManager.GetComponentData<MagicStatComponent>(playerEntity)
                    };

                    playerData.ItemDataList = new List<ItemData>();
                    this.Entities.WithAll<ItemComponent, PickedUpComponent>().ForEach((Entity itemEntity,
                        ref ItemComponent itemComponent, ref PickedUpComponent pickedUpComponent) =>
                    {
                        if (pickedUpComponent.Owner == playerEntity)
                            playerData.ItemDataList.Add(new ItemData
                            {
                                IsEquiped = this.EntityManager.HasComponent<EquipedComponent>(itemEntity),
                                ItemComponent = itemComponent,
                                AttackModifier = this.EntityManager.HasComponent<AttackStatModifierComponent>(itemEntity) ?
                                                  this.EntityManager.GetComponentData<AttackStatModifierComponent>(itemEntity) :
                                                  new AttackStatModifierComponent(),
                                DefenceModifier = this.EntityManager.HasComponent<DefenceStatModifierComponent>(itemEntity) ?
                                                  this.EntityManager.GetComponentData<DefenceStatModifierComponent>(itemEntity) :
                                                  new DefenceStatModifierComponent(),
                                HealthModifier = this.EntityManager.HasComponent<HealthStatModifierComponent>(itemEntity) ?
                                                  this.EntityManager.GetComponentData<HealthStatModifierComponent>(itemEntity) :
                                                  new HealthStatModifierComponent(),
                                MagicModifier = this.EntityManager.HasComponent<MagickStatModifierComponent>(itemEntity) ?
                                                  this.EntityManager.GetComponentData<MagickStatModifierComponent>(itemEntity) :
                                                  new MagickStatModifierComponent(),
                            });
                    });
                });
                if (playerData != null)
                    SaveGameManager.SaveData(playerData);

                this.PostUpdateCommands.DestroyEntity(entity);
            });
        }
    }
}
