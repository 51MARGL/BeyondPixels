using BeyondPixels.ECS.Components.Scenes;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class StoryMessageTrigger : MonoBehaviour
    {
        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (!collider.CompareTag("Player"))
            {
                return;
            }

            var entityManager = World.Active.EntityManager;
            var entity = this.GetComponent<GameObjectEntity>().Entity;

            if (!entityManager.HasComponent<PrintStoryComponent>(entity))
            {
                entityManager.AddComponentData(this.GetComponent<GameObjectEntity>().Entity,
                    new PrintStoryComponent());
            }

            GameObject.Destroy(this.GetComponent<BoxCollider2D>());
        }
    }
}
