using System;

using Unity.Entities;

namespace BeyondPixels.ECS.Components.Items
{
    [Serializable]
    public struct AttackStatModifierComponent : IComponentData
    {
        public int Value;
    }
}
