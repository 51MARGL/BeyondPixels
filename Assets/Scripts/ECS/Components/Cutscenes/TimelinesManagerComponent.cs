using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Cutscenes
{
    public class TimelinesManagerComponent : MonoBehaviour
    {
        public static TimelinesManagerComponent Instance { get; private set; }
        public TimelinesComponent Timelines { get; private set; }

        public void Start()
        {
            TimelinesManagerComponent.Instance = this;
            this.Timelines = GetComponent<TimelinesComponent>();
        }
    }
}
