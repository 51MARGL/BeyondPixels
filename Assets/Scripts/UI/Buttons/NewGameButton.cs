using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.UI.ECS.Components;

using Unity.Entities;

namespace BeyondPixels.UI.Buttons
{
    public class NewGameButton : SubmitConfirmButton
    {
        protected override void InitConfirmDialog()
        {
            if (SaveGameManager.SaveExists)
            {
                base.InitConfirmDialog();

                this.ConfirmDialog.YesButton.OnSubmitEvent += this.StartNewGame;
            }
            else
            {
                this.StartNewGame();
            }
        }

        protected virtual void StartNewGame()
        {
            var entityManager = World.Active.EntityManager;
            var eventEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(eventEntity, new NewGameButtonPressedComponent());
        }
    }
}
