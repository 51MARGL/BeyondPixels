using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnEnemyAggroRangeTriggerToEntity : OnAggroRangeTriggerToEntity
    {
        protected override void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                base.OnTriggerEnter2D(collider);
            }
        }

        protected override void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                base.OnTriggerExit2D(collider);
            }
        }

        protected override void OnTriggerStay2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                base.OnTriggerStay2D(collider);
            }
        }
    }
}
