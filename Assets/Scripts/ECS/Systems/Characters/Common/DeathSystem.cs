﻿using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.SceneBootstraps;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    public class DeathSystem : ComponentSystem
    {
        private ComponentGroup _group;

        protected override void OnCreateManager()
        {
            this._group = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(HealthComponent), typeof(CharacterComponent),
                    typeof(PositionComponent)
                },
                None = new ComponentType[]
                {
                    typeof(KilledComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._group).ForEach((Entity entity, ref HealthComponent healthComponent, ref PositionComponent positionComponent) =>
            {
                if (healthComponent.CurrentValue <= 0)
                {
                    this.PostUpdateCommands.AddComponent(entity, new KilledComponent());
                }
            });
        }
    }
}
