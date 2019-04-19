using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnAggroRangeTriggerToEntity : MonoBehaviour
    {
        private bool hasTarget;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                var wallLayer = LayerMask.GetMask("World");
                var distance = math.distance(this.transform.position, collider.transform.position);
                var hits = Physics2D.RaycastAll(this.transform.position,
                                                collider.transform.position - this.transform.position,
                                                distance, wallLayer);

                foreach (var hit in hits)
                {
                    if (hit.transform.tag == "Wall")
                        return;
                }

                var entityManager = World.Active.GetExistingManager<EntityManager>();
                var sender = this.GetComponentInParent<GameObjectEntity>().Entity;
                var target = collider.GetComponentInParent<GameObjectEntity>().Entity;
                if (!entityManager.Exists(sender) || !entityManager.Exists(target))
                    return;

                var eventEntity = entityManager.CreateEntity(typeof(CollisionInfo),
                                                             typeof(AggroRangeCollisionComponent));

                entityManager.SetComponentData(eventEntity, new CollisionInfo
                {
                    Sender = sender,
                    Target = target,
                    EventType = EventType.TriggerEnter
                });

                this.hasTarget = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                var entityManager = World.Active.GetExistingManager<EntityManager>();
                var sender = this.GetComponentInParent<GameObjectEntity>().Entity;
                var target = collider.GetComponentInParent<GameObjectEntity>().Entity;
                if (!entityManager.Exists(sender) || !entityManager.Exists(target))
                    return;

                var eventEntity = entityManager.CreateEntity(typeof(CollisionInfo), typeof(AggroRangeCollisionComponent));

                entityManager.SetComponentData(eventEntity, new CollisionInfo
                {
                    Sender = sender,
                    Target = target,
                    EventType = EventType.TriggerExit
                });

                this.hasTarget = false;
            }
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (this.hasTarget)
                return;

            if (collider.gameObject.CompareTag("Player"))
            {
                var wallLayer = LayerMask.GetMask("World");
                var distance = math.distance(this.transform.position, collider.transform.position);
                var hits = Physics2D.RaycastAll(this.transform.position,
                                                collider.transform.position - this.transform.position,
                                                distance, wallLayer);

                foreach (var hit in hits)
                {
                    if (hit.transform.tag == "Wall")
                        return;
                }

                var entityManager = World.Active.GetExistingManager<EntityManager>();
                var sender = this.GetComponentInParent<GameObjectEntity>().Entity;
                var target = collider.GetComponentInParent<GameObjectEntity>().Entity;
                if (!entityManager.Exists(sender) || !entityManager.Exists(target))
                    return;

                var eventEntity = entityManager.CreateEntity(typeof(CollisionInfo),
                                                             typeof(AggroRangeCollisionComponent));

                entityManager.SetComponentData(eventEntity, new CollisionInfo
                {
                    Sender = sender,
                    Target = target,
                    EventType = EventType.TriggerEnter
                });

                this.hasTarget = true;
            }
        }
    }
}
