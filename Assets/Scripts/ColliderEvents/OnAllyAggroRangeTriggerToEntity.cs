using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnAllyAggroRangeTriggerToEntity : OnAggroRangeTriggerToEntity
    {
        protected override void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Enemy"))
            {
                base.OnTriggerEnter2D(collider);
            }
        }

        protected override void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Enemy"))
            {
                base.OnTriggerExit2D(collider);
            }
        }

        protected override void OnTriggerStay2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Enemy"))
            {
                base.OnTriggerStay2D(collider);
            }
        }
    }
}
