using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Items;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnUseTriggerToEntity : MonoBehaviour
    {
        public Canvas Canvas;
        protected bool IsInside;

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("UseTrigger"))
            {
                this.Canvas.enabled = true;
                this.IsInside = true;
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("UseTrigger"))
            {
                this.Canvas.enabled = false;
                this.IsInside = false;
            }
        }

        public virtual void Update()
        {
            if (this.IsInside && Input.GetKeyDown(KeyCode.E))
            {
                this.Use();
            }
        }

        public virtual void Use()
        {

        }
    }
}
