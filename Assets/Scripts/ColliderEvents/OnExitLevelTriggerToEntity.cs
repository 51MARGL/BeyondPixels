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
                var gameObject = new GameObject("Exit");
                var component = gameObject.AddComponent<PlayerExitCutsceneComponent>();
                component.ExitCaveDoor = this.transform.GetChild(0).gameObject;
                gameObject.AddComponent<GameObjectEntity>();

                this.Canvas.enabled = false;
                this.IsInside = false;
            }
        }
    }
}
