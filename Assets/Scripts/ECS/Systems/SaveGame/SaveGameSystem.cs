using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.SaveGame;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.SaveGame
{
    public class SaveGameSystem : ComponentSystem
    {
        private ComponentGroup _saveGroup;

        protected override void OnCreateManager()
        {
            this._saveGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(SaveGameComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._saveGroup).ForEach((EntityQueryBuilder.F_E)((Entity entity) =>
            {
                SaveData playerData = null;
                this.Entities.WithAll<PlayerComponent>().ForEach((EntityQueryBuilder.F_E)((Entity playerEntity) =>
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
                    this.Entities.WithAll<ItemComponent, PickedUpComponent>().ForEach((EntityQueryBuilder.F_EDD<ItemComponent, PickedUpComponent>)((Entity itemEntity, 
                        ref ItemComponent itemComponent, ref PickedUpComponent pickedUpComponent) =>
                    {
                        if (pickedUpComponent.Owner == playerEntity)
                            playerData.ItemDataList.Add(new ItemData
                            {
                                IsEquiped = EntityManager.HasComponent<EquipedComponent>(itemEntity),
                                ItemComponent = itemComponent,
                                AttackModifier = EntityManager.HasComponent<AttackStatModifierComponent>(itemEntity) ?
                                                  EntityManager.GetComponentData<AttackStatModifierComponent>(itemEntity) :
                                                  new AttackStatModifierComponent(),
                                DefenceModifier = EntityManager.HasComponent<DefenceStatModifierComponent>(itemEntity) ?
                                                  EntityManager.GetComponentData<DefenceStatModifierComponent>(itemEntity) :
                                                  new DefenceStatModifierComponent(),
                                HealthModifier = EntityManager.HasComponent<HealthStatModifierComponent>(itemEntity) ?
                                                  EntityManager.GetComponentData<HealthStatModifierComponent>(itemEntity) :
                                                  new HealthStatModifierComponent(),
                                MagicModifier = EntityManager.HasComponent<MagickStatModifierComponent>(itemEntity) ?
                                                  EntityManager.GetComponentData<MagickStatModifierComponent>(itemEntity) :
                                                  new MagickStatModifierComponent(),
                            });
                    }));
                }));
                if (playerData != null)
                    SaveGameManager.SaveData(playerData);

                this.PostUpdateCommands.DestroyEntity(entity);
            }));
        }
    }
}
