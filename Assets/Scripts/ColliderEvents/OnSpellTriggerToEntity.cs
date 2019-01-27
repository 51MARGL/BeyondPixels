using BeyondPixels.Components.Characters.Spells;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnSpellTriggerToEntity : MonoBehaviour
    {
        private float totalTime;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Hitbox"))
            {
                var entityManager = World.Active.GetExistingManager<EntityManager>();            
                var eventEntity = entityManager.CreateEntity(typeof(CollisionInfo), typeof(SpellCollisionComponent));

                entityManager.SetComponentData(eventEntity,
                        new CollisionInfo
                        {
                            Sender = GetComponent<GameObjectEntity>().Entity,
                            Other = collider.GetComponentInParent<GameObjectEntity>().Entity,
                            EventType = EventType.TriggerEnter
                        });
                entityManager.SetComponentData(eventEntity,
                        new SpellCollisionComponent
                        {
                            ImpactPoint = this.transform.position,
                            ImpactTime = Time.time
                        });
                totalTime = 0;
            }
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Hitbox"))
            {
                totalTime += Time.deltaTime;
                if (totalTime > 1f)
                {
                    var entityManager = World.Active.GetExistingManager<EntityManager>();            
                    var eventEntity = entityManager.CreateEntity(typeof(CollisionInfo), typeof(SpellCollisionComponent));

                    entityManager.SetComponentData(eventEntity,
                            new CollisionInfo
                            {
                                Sender = GetComponent<GameObjectEntity>().Entity,
                                Other = collider.GetComponentInParent<GameObjectEntity>().Entity,
                                EventType = EventType.TriggerStay
                            });
                    entityManager.SetComponentData(eventEntity,
                            new SpellCollisionComponent
                            {
                                ImpactPoint = this.transform.position,
                                ImpactTime = Time.time
                            });
                    totalTime = 0;
                }
            }
        }
    }
}
