using BeyondPixels.ECS.Components.Objects;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class EntityEnableDisableTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collider)
        {
            var entityManager = World.Active.GetExistingManager<EntityManager>();
            var target = collider.GetComponent<GameObjectEntity>().Entity;
            if (!entityManager.Exists(target) || !entityManager.HasComponent<Disabled>(target))
                return;

            var eventEntity = entityManager.CreateEntity(typeof(EntityEnableComponent));

            entityManager.SetComponentData(eventEntity, new EntityEnableComponent
            {
                Target = target,
            });
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            var entityManager = World.Active.GetExistingManager<EntityManager>();
            var target = collider.GetComponent<GameObjectEntity>().Entity;
            if (!entityManager.Exists(target) || entityManager.HasComponent<Disabled>(target))
                return;

            var eventEntity = entityManager.CreateEntity(typeof(EntityDisableComponent));

            entityManager.SetComponentData(eventEntity, new EntityDisableComponent
            {
                Target = target,
            });
        }
    }
}
