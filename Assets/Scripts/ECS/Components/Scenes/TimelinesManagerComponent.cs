using UnityEngine;

namespace BeyondPixels.ECS.Components.Scenes
{
    public class TimelinesManagerComponent : MonoBehaviour
    {
        public static TimelinesManagerComponent Instance { get; private set; }
        public TimelinesComponent Timelines { get; private set; }

        public void Start()
        {
            TimelinesManagerComponent.Instance = this;
            this.Timelines = this.GetComponent<TimelinesComponent>();
        }
    }
}
