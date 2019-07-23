using BeyondPixels.UI.ECS.Components;
using Unity.Entities;

namespace BeyondPixels.UI.Buttons
{
    public class QuitButton : SubmitConfirmButton
    {
        protected override void InitConfirmDialog()
        {
            base.InitConfirmDialog();

            this.ConfirmDialog.YesButton.OnSubmitEvent += () =>
            {
                var entityManager = World.Active.EntityManager;
                var eventEntity = entityManager.CreateEntity();
                entityManager.AddComponentData(eventEntity, new QuitButtonPressedComponent());
            };
        }
    }
}
