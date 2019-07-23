using System;

using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Stats
{
    [Serializable]
    public struct MagicStatComponent : IComponentData
    {
        public int CurrentValue;
        public int BaseValue;
        public int PerPointValue;
        public int PointsSpent;
    }
}
