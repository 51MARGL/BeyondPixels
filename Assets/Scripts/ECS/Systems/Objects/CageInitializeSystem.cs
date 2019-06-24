﻿using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Objects;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Objects
{
    public class CageInitializeSystem : ComponentSystem
    {
        private ComponentGroup _group;

        protected override void OnCreateManager()
        {
            this._group = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(CageInitializeComponent), typeof(Transform)
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
                CageInitializeComponent cageInitializeComponent,
                Transform transform) =>
            {
                this.PostUpdateCommands.AddComponent(entity, new PositionComponent
                {
                    CurrentPosition = new float2(transform.position.x, transform.position.y),
                    InitialPosition = new float2(transform.position.x, transform.position.y)
                });
                this.PostUpdateCommands.AddComponent(entity, new XPRewardComponent
                {
                    XPAmount = cageInitializeComponent.XPAmount
                });

                var lvlComponent = new LevelComponent
                {
                    CurrentLevel =
                        math.max(0, random.NextInt(playerLvlComponent.CurrentLevel,
                                                   playerLvlComponent.CurrentLevel + 3))
                };
                this.PostUpdateCommands.AddComponent(entity, lvlComponent);

                GameObject.Destroy(cageInitializeComponent);
            });
        }
    }
}
