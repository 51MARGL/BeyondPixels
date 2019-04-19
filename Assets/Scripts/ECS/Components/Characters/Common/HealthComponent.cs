using System;

using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    [Serializable]
    public struct HealthComponent : IComponentData
    {
        public float MaxValue;
        public float CurrentValue;
    }
}
