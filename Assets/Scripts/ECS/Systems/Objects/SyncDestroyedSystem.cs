using BeyondPixels.ECS.Components.Objects;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Objects
{
    public class SyncDestroyedSystem : ComponentSystem
    {
        private ComponentGroup _syncGroup;

        protected override void OnCreateManager()
        {
            this._syncGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(SyncDestroyedComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            if (this._syncGroup.CalculateLength() > 0)
            {
                var gameObjects = Object.FindObjectsOfType<GameObjectEntity>();
                this.Entities.With(this._syncGroup).ForEach((Entity entity, ref SyncDestroyedComponent syncComponent) =>
                {
                    foreach (var gameObject in gameObjects)
                        if (syncComponent.EntityID == gameObject.Entity.Index
                            && !this.EntityManager.Exists(gameObject.Entity))
                        {
                            Object.Destroy(gameObject.gameObject);
                        }

                    this.PostUpdateCommands.DestroyEntity(entity);
                });
            }
        }
    }
}
