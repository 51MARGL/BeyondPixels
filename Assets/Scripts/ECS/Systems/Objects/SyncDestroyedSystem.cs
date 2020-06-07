using BeyondPixels.ECS.Components.Objects;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Objects
{
    public class SyncDestroyedSystem : ComponentSystem
    {
        private EntityQuery _syncGroup;

        protected override void OnCreate()
        {
            this._syncGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(SyncDestroyedComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            if (this._syncGroup.CalculateEntityCount() > 0)
            {
                var gameObjects = Object.FindObjectsOfType<GameObjectEntity>();
                this.Entities.With(this._syncGroup).ForEach((Entity entity, ref SyncDestroyedComponent syncComponent) =>
                {
                    foreach (var gameObject in gameObjects)
                    {
                        if (syncComponent.EntityID == gameObject.Entity.Index
                            && !this.EntityManager.Exists(gameObject.Entity))
                        {
                            Object.Destroy(gameObject.gameObject);
                        }
                    }

                    this.PostUpdateCommands.DestroyEntity(entity);
                });
            }
        }
    }
}
