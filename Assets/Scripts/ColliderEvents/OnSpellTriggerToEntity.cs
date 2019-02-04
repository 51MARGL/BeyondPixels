using BeyondPixels.ECS.Components.Spells;
using Unity.Entities;
using Unity.Mathematics;
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
                var spellEntity = GetComponent<GameObjectEntity>().Entity;
                var caster = entityManager.GetComponentData<SpellComponent>(spellEntity).Caster;

                entityManager.SetComponentData(eventEntity,
                        new CollisionInfo
                        {
                            Sender = caster,
                            Other = collider.GetComponentInParent<GameObjectEntity>().Entity,
                            EventType = EventType.TriggerEnter
                        });
                entityManager.SetComponentData(eventEntity,
                        new SpellCollisionComponent
                        {
                            SpellEntity = spellEntity,
                            ImpactPoint = new float2(this.transform.position.x, this.transform.position.y),
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
                    var spellEntity = GetComponent<GameObjectEntity>().Entity;
                    var caster = entityManager.GetComponentData<SpellComponent>(spellEntity).Caster;

                    entityManager.SetComponentData(eventEntity,
                            new CollisionInfo
                            {
                                Sender = caster,
                                Other = collider.GetComponentInParent<GameObjectEntity>().Entity,
                                EventType = EventType.TriggerStay
                            });
                    entityManager.SetComponentData(eventEntity,
                            new SpellCollisionComponent
                            {
                                SpellEntity = spellEntity,
                                ImpactPoint = new float2(this.transform.position.x, this.transform.position.y),
                                ImpactTime = Time.time
                            });
                    totalTime = 0;
                }
            }
        }
    }
}
