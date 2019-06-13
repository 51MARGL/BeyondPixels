using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Objects;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Objects
{
    public class EnableDisableSystem : ComponentSystem
    {
        private ComponentGroup _enableGroup;
        private ComponentGroup _disableGroup;

        protected override void OnCreateManager()
        {
            this._enableGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(EntityEnableComponent)
                }
            });
            this._disableGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(EntityDisableComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(this._enableGroup).ForEach((Entity eventEntity, ref EntityEnableComponent entityEnableComponent) => {
                this.PostUpdateCommands.RemoveComponent<Disabled>(entityEnableComponent.Target);
                this.PostUpdateCommands.DestroyEntity(eventEntity);
            });
            Entities.With(this._disableGroup).ForEach((Entity eventEntity, ref EntityDisableComponent entityDisaleComponent) => {
                if (EntityManager.HasComponent<MovementComponent>(entityDisaleComponent.Target))
                {
                    var movementComponent = EntityManager.GetComponentData<MovementComponent>(entityDisaleComponent.Target);
                    var rigidBody = EntityManager.GetComponentObject<Rigidbody2D>(entityDisaleComponent.Target);
                    rigidBody.velocity = Vector2.zero;
                    movementComponent.Direction = float2.zero;
                    this.PostUpdateCommands.SetComponent(entityDisaleComponent.Target, movementComponent);

                }
                this.PostUpdateCommands.AddComponent(entityDisaleComponent.Target, new Disabled());
                this.PostUpdateCommands.DestroyEntity(eventEntity);
            });
        }
    }
}
