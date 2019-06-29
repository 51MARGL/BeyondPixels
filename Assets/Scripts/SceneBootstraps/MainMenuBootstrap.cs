using BeyondPixels.ECS.Components.Game;
using BeyondPixels.UI;

using UnityEngine;

namespace BeyondPixels.SceneBootstraps
{
    public class MainMenuBootstrap : MonoBehaviour
    {
        private void Start()
        {
            var settings = SettingsManager.Instance;
            UIManager.Instance.MainMenu.InGameMenu = false;
            UIManager.Instance.MainMenu.Show();
            Time.timeScale = 1f;            
        }
    }
}
