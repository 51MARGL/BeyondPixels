using UnityEngine;
using UnityEngine.UI;

namespace BeyondPixels.UI.ECS.Components
{
    public class EnemyUIComponent : MonoBehaviour
    {
        public Canvas Canvas;
        public Image HealthImage;
        public Text HealthText;
        public GameObject TargettingCircle;
    }
}
