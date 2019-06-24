using BeyondPixels.ECS.Components.Game;
using BeyondPixels.UI;
using BeyondPixels.Utilities;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.SceneBootstraps
{
    public class TutorialBootstrap : MonoBehaviour
    {
        private FixedUpdateSystemGroup FixedGroup;

        private void Start()
        {
            this.FixedGroup = World.Active.GetOrCreateManager<FixedUpdateSystemGroup>();

            var settings = SettingsManager.Instance;
            UIManager.Instance.MainMenu.InGameMenu = true;
        }

        public void FixedUpdate()
        {
            this.FixedGroup.Update();
        }
    }
}
