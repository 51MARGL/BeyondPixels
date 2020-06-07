using System;

using Unity.Entities;

namespace BeyondPixels.ECS.Components.Quest
{
    [Serializable]
    public struct QuestComponent : IComponentData
    {
        public int CurrentProgress;
        public int ProgressTarget;
    }
}
