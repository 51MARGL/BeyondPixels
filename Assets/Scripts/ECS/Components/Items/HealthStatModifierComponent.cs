using System;

using Unity.Entities;

namespace BeyondPixels.ECS.Components.Items
{
    [Serializable]
    public struct HealthStatModifierComponent : IComponentData
    {
        public int Value;
    }
}
