using System;
using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Stats
{
    [Serializable]
    public struct HealthStatComponent : IComponentData
    {
        public int CurrentValue;
        public int BaseValue;
        public int PerLevelValue;
    }
}
