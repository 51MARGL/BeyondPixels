using BeyondPixels.UI.ECS.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeyondPixels.UI.Buttons
{
    public class LoadLastButton : MonoBehaviour, IPointerClickHandler, ISubmitHandler
    {
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                this.Submit();
            }
        }

        private void Submit()
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            var eventEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(eventEntity, new LoadLastButtonPressedComponent());
        }

        public void OnSubmit(BaseEventData eventData)
        {
            this.Submit();
        }
    }
}
