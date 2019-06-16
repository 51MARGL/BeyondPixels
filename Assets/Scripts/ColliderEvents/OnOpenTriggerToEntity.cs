using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnOpenTriggerToEntity : OnUseTriggerToEntity
    {
        public Animator Animator;
        protected bool IsOpened;

        protected override void OnTriggerEnter2D(Collider2D collider)
        {
            if (!this.IsOpened)
            {
                base.OnTriggerEnter2D(collider);
            }
        }

        protected override void OnTriggerExit2D(Collider2D collider)
        {
            if (!this.IsOpened)
            {
                base.OnTriggerExit2D(collider);
            }
        }

        public override void Update()
        {
            if (!this.IsOpened)
            {
                base.Update();
            }
        }

        public override void Use()
        {
            base.Use();

            this.Animator.SetTrigger("Open");

            this.Canvas.enabled = false;
            this.IsInside = false;
            this.IsOpened = true;
        }

        public virtual void OnAnimationEnd()
        {

        }
    }
}
