using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnAggroRangeTriggerToEntity : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                var entityManager = World.Active.GetExistingManager<EntityManager>();
                var eventEntity = entityManager.CreateEntity(typeof(CollisionInfo), typeof(AggroRangeCollisionComponent));

                entityManager.SetComponentData(eventEntity, new CollisionInfo
                {
                    Sender = GetComponentInParent<GameObjectEntity>().Entity,
                    Other = collider.GetComponentInParent<GameObjectEntity>().Entity,
                    EventType = EventType.TriggerEnter
                });
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Player")
                && this.gameObject.layer == collider.gameObject.layer)
            {
                var entityManager = World.Active.GetExistingManager<EntityManager>();
                var eventEntity = entityManager.CreateEntity(typeof(CollisionInfo), typeof(AggroRangeCollisionComponent));

                entityManager.SetComponentData(eventEntity, new CollisionInfo
                {
                    Sender = GetComponentInParent<GameObjectEntity>().Entity,
                    Other = collider.GetComponentInParent<GameObjectEntity>().Entity,
                    EventType = EventType.TriggerExit
                });
            }
        }
    }
}
