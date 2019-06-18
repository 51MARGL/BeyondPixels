using BeyondPixels.UI.ECS.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeyondPixels.UI.Buttons
{
    public class ResumeButton : SubmitButton
    {
        protected override void Submit()
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            var eventEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(eventEntity, new ResumeButtonPressedComponent());
        }
    }
}
