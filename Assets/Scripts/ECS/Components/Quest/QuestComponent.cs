using Unity.Entities;

namespace BeyondPixels.ECS.Components.Quest
{
    public struct QuestComponent : IComponentData
    {
        public int CurrentProgress;
        public int ProgressTarget;
    }
}
