using UnityEngine;

namespace BeyondPixels.SceneBootstraps
{
    public class AppCloseFix : MonoBehaviour
    {
        private void OnApplicationQuit()
        {
            if (!Application.isEditor)
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
    }
}
