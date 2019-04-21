using System;

using Unity.Entities;

namespace BeyondPixels.ECS.Components.Items
{
    [Serializable]
    public struct MagicStatModifierComponent : IComponentData
    {
        public int Value;
    }
}
