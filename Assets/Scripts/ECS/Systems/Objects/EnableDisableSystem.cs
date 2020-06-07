using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Objects;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Objects
{
    public class EnableDisableSystem : ComponentSystem
    {
        private EntityQuery _enableGroup;
        private EntityQuery _disableGroup;

        protected override void OnCreate()
        {
            this._enableGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(EntityEnableComponent)
                }
            });
            this._disableGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(EntityDisableComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._enableGroup).ForEach((Entity eventEntity, ref EntityEnableComponent entityEnableComponent) =>
            {
                this.PostUpdateCommands.RemoveComponent<Disabled>(entityEnableComponent.Target);
                this.PostUpdateCommands.DestroyEntity(eventEntity);
            });
            this.Entities.With(this._disableGroup).ForEach((Entity eventEntity, ref EntityDisableComponent entityDisaleComponent) =>
            {
                if (this.EntityManager.HasComponent<MovementComponent>(entityDisaleComponent.Target))
                {
                    var movementComponent = this.EntityManager.GetComponentData<MovementComponent>(entityDisaleComponent.Target);
                    var rigidBody = this.EntityManager.GetComponentObject<Rigidbody2D>(entityDisaleComponent.Target);
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
