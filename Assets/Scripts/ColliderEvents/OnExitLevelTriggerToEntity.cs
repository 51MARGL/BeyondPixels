using BeyondPixels.ECS.Components.Scenes;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnExitLevelTriggerToEntity : MonoBehaviour
    {
        public Canvas Canvas;
        private bool IsInside;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("UseTrigger"))
            {
                this.Canvas.enabled = true;
                this.IsInside = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("UseTrigger"))
            {
                this.Canvas.enabled = false;
                this.IsInside = false;
            }

        }
        public void Update()
        {
            if (this.IsInside && Input.GetKeyDown(KeyCode.E))
            {
                var entityManager = World.Active.GetExistingManager<EntityManager>();
                var entity = this.GetComponent<GameObjectEntity>().Entity;
                entityManager.AddComponentData(entity, new PlayerExitCutsceneComponent());

                this.Canvas.enabled = false;
                this.IsInside = false;
            }
        }
    }
}
