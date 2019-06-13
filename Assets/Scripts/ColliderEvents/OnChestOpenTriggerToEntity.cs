using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Items;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnChestOpenTriggerToEntity : MonoBehaviour
    {
        public Canvas Canvas;
        public Animator Animator;
        private bool IsInside;
        private bool IsOpened;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("UseTrigger") && !this.IsOpened)
            {
                this.Canvas.enabled = true;
                this.IsInside = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("UseTrigger") && !this.IsOpened)
            {
                this.Canvas.enabled = false;
                this.IsInside = false;
            }
        }

        public void Update()
        {
            if (!this.IsOpened && this.IsInside && Input.GetKeyDown(KeyCode.E))
            {
                this.Animator.SetTrigger("Open");

                this.Canvas.enabled = false;
                this.IsInside = false;
                this.IsOpened = true;
            }
        }

        public void OnChestAnimationEnd()
        {
            var entityManager = World.Active.GetExistingManager<EntityManager>();
            var entity = this.GetComponent<GameObjectEntity>().Entity;
            entityManager.AddComponentData(entity, new DropLootComponent());
            entityManager.AddComponentData(entity, new CollectXPRewardComponent());
            Object.Destroy(this.GetComponent<CircleCollider2D>());
        }
    }
}
