using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.UI.ECS.Components;

using Unity.Entities;

using UnityEngine;
using UnityEngine.EventSystems;

namespace BeyondPixels.UI.Buttons
{
    public class AddStatButton : MonoBehaviour, IPointerClickHandler
    {
        public StatTarget StatTarget;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                var entityManager = World.Active.GetOrCreateManager<EntityManager>();
                var eventEntity = entityManager.CreateEntity();
                entityManager.AddComponentData(eventEntity, new AddStatButtonPressedComponent
                {
                    StatTarget = StatTarget
                });
            }
        }
    }
}
