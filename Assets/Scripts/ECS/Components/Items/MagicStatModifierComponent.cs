using System;

using Unity.Entities;

namespace BeyondPixels.ECS.Components.Items
{
    [Serializable]
    public struct MagickStatModifierComponent : IComponentData
    {
        public int Value;
    }
}
