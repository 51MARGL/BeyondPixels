using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnChestOpenEnableObjectTriggerToEntity : OnChestOpenTriggerToEntity
    {
        public GameObject GameObject;

        public override void OnAnimationEnd()
        {
            base.OnAnimationEnd();

            this.GameObject.SetActive(true);
        }
    }
}
