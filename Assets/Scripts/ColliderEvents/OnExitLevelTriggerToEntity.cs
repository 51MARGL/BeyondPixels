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
                Canvas.enabled = true;
                IsInside = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("UseTrigger"))
            {
                Canvas.enabled = false;
                IsInside = false;
            }

        }
        public void Update()
        {
            if (IsInside && Input.GetKeyDown(KeyCode.E))
            {
                var entityManager = World.Active.GetExistingManager<EntityManager>();

                var eventEntity = entityManager.CreateEntity();

                var gameObject = new GameObject("Exit");
                var component = gameObject.AddComponent<PlayerExitCutsceneComponent>();
                component.ExitCaveDoor = this.transform.GetChild(0).gameObject;
                gameObject.AddComponent<GameObjectEntity>();

                Canvas.enabled = false;
                IsInside = false;
            }
        }
    }
}
