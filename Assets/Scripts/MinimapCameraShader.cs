using UnityEngine;

namespace BeyondPixels
{
    public class MinimapCameraShader : MonoBehaviour
    {
        public Shader unlitShader;

        private void Start()
        {
            unlitShader = Shader.Find("Unlit/Texture");
            GetComponent<Camera>().SetReplacementShader(unlitShader, "");
        }
    }
}
