using BeyondPixels.ECS.Components.Game;

using TMPro;

using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnUseTriggerToEntity : MonoBehaviour
    {
        public Canvas Canvas;
        public TextMeshProUGUI Text;

        protected bool IsInside;

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("UseTrigger"))
            {
                this.Canvas.enabled = true;
                this.IsInside = true;

                this.Text.text = "Press "
                    + SettingsManager.Instance.GetKeyBindValue(KeyBindName.Use).ToString();
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
            if (this.IsInside
                && Input.GetKeyDown(SettingsManager.Instance.GetKeyBindValue(KeyBindName.Use)))
            {
                this.Use();
            }
        }

        public virtual void Use()
        {

        }
    }
}
