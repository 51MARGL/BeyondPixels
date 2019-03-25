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
            _syncGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(SyncDestroyedComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            if (_syncGroup.CalculateLength() > 0)
            {
                var gameObjects = Object.FindObjectsOfType<GameObjectEntity>();
                Entities.With(_syncGroup).ForEach((Entity entity, ref SyncDestroyedComponent syncComponent) =>
                {
                    foreach (var gameObject in gameObjects)
                        if (syncComponent.EntityID == gameObject.Entity.Index
                            && !EntityManager.Exists(gameObject.Entity))
                        {
                            Object.Destroy(gameObject.gameObject);
                            PostUpdateCommands.DestroyEntity(entity);
                        }
                });
            }
        }
    }
}
