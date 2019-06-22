using BeyondPixels.UI.ECS.Components;
using Unity.Entities;

namespace BeyondPixels.UI.Buttons
{
    public class RestartButton : SubmitButton
    {
        public void Start()
        {
            this.OnSubmitEvent += this.Submit;
        }

        protected void Submit()
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            var eventEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(eventEntity, new LoadLastButtonPressedComponent());
        }
    }
}
