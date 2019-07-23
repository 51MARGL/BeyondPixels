using UnityEngine;
using UnityEngine.Playables;

namespace BeyondPixels.ECS.Components.Scenes
{
    public class TimelinesComponent : MonoBehaviour
    {
        public PlayableDirector PlayerDungeonEnter;
        public PlayableDirector PlayerDungeonExit;
        public PlayableDirector PlayerTutorialEnter;
    }
}
