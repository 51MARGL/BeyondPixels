using BeyondPixels.UI.ECS.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeyondPixels.UI.Buttons
{
    public class ResumeButton : SubmitButton
    {
        public void Start()
        {
            this.OnSubmitEvent += this.Submit;
        }

        protected void Submit()
        {
            var entityManager = World.Active.EntityManager;
            var eventEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(eventEntity, new ResumeButtonPressedComponent());
        }
    }
}
