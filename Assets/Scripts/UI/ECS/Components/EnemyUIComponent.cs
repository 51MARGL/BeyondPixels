using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeyondPixels.UI.ECS.Components
{
    public class EnemyUIComponent : MonoBehaviour
    {
        public Canvas Canvas;
        public Image HealthImage;
        public TextMeshProUGUI HealthText;
        public GameObject TargettingCircle;
    }
}
