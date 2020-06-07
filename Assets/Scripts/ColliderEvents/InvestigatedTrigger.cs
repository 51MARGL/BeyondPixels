using BeyondPixels.ECS.Components.Scenes;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class InvestigatedTrigger : MonoBehaviour
    {
        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (!collider.CompareTag("Player"))
            {
                return;
            }

            var entityManager = World.Active.EntityManager;
            var entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new InvestigatedComponent());
        }
    }
}
