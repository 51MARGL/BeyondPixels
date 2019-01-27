using BeyondPixels.Components.Characters.Common;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnWeaponTriggerToEntity : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Hitbox")
                && !this.transform.parent.gameObject.CompareTag(collider.transform.parent.tag))
            {
                var entityManager = World.Active.GetExistingManager<EntityManager>();
                var eventEntity = entityManager.CreateEntity(typeof(CollisionInfo), typeof(DamageComponent));

                entityManager.SetComponentData(eventEntity,
                        new CollisionInfo
                        {
                            Sender = GetComponentInParent<GameObjectEntity>().Entity,
                            Other = collider.GetComponentInParent<GameObjectEntity>().Entity,
                            EventType = EventType.TriggerEnter
                        });
                entityManager.SetComponentData(eventEntity,
                        new DamageComponent
                        {
                            DamageType = DamageType.Weapon,
                            DamageValue =
                                entityManager.
                                    GetComponentData<WeaponComponent>(this.GetComponentInParent<GameObjectEntity>().Entity).
                                        DamageValue
                        });
            }
        }
    }
}
