using UnityEngine;

namespace BeyondPixels.SceneBootstraps
{
    public class AppCloseFix : MonoBehaviour
    {
        private void OnApplicationQuit()
        {
            StopAllCoroutines();
            if (!Application.isEditor)
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
    }
}
