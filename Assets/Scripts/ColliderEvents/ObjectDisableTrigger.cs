using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class ObjectDisableTrigger : MonoBehaviour
    {
        public GameObject GameObject;

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                this.GameObject.SetActive(false);
            }
        }
    }
}
