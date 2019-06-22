using BeyondPixels.UI;

using UnityEngine;

namespace BeyondPixels.SceneBootstraps
{
    public class MainMenuBootstrap : MonoBehaviour
    {
        private void Start()
        {
            UIManager.Instance.MainMenu.InGameMenu = false;
            UIManager.Instance.MainMenu.Show();
        }
    }
}
