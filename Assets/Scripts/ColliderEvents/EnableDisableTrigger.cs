using BeyondPixels.ECS.Components.Objects;

using Unity.Entities;

using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace BeyondPixels.ColliderEvents
{
    public class EnableDisableTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collider)
        {
            var entityManager = World.Active.EntityManager;
            if (!entityManager.Exists(this.GetComponentInParent<GameObjectEntity>().Entity))
            {
                return;
            }

            if (collider.CompareTag("Enemy") || collider.CompareTag("Ally"))
            {
                var target = collider.GetComponent<GameObjectEntity>().Entity;
                if (!entityManager.Exists(target) || !entityManager.HasComponent<Disabled>(target))
                {
                    return;
                }

                var eventEntity = entityManager.CreateEntity(typeof(EntityEnableComponent));

                entityManager.SetComponentData(eventEntity, new EntityEnableComponent
                {
                    Target = target,
                });
            }
            else if (collider.CompareTag("Light"))
            {
                collider.GetComponent<Light2D>().enabled = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            var entityManager = World.Active.EntityManager;
            if (!entityManager.Exists(this.GetComponentInParent<GameObjectEntity>().Entity))
            {
                return;
            }

            if (collider.CompareTag("Enemy") || collider.CompareTag("Ally"))
            {
                var target = collider.GetComponent<GameObjectEntity>().Entity;
                if (!entityManager.Exists(target) || entityManager.HasComponent<Disabled>(target))
                {
                    return;
                }

                var eventEntity = entityManager.CreateEntity(typeof(EntityDisableComponent));

                entityManager.SetComponentData(eventEntity, new EntityDisableComponent
                {
                    Target = target,
                });
            }
            else if (collider.CompareTag("Light"))
            {
                collider.GetComponent<Light2D>().enabled = false;
            }
        }
    }
}
