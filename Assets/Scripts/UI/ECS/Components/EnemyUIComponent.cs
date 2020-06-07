using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static BeyondPixels.UI.ECS.Components.GameUIComponent;

namespace BeyondPixels.UI.ECS.Components
{
    public class EnemyUIComponent : MonoBehaviour
    {
        public Canvas Canvas;
        public Image HealthImage;
        public TextMeshProUGUI HealthText;
        public GameObject TargettingCircle;
        public SpellCastBarGroupWrapper SpellCastBarGroup;
    }
}
