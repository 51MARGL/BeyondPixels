using UnityEngine;

namespace BeyondPixels.Utilities
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
