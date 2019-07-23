using System;

using Unity.Entities;

namespace BeyondPixels.ECS.Components.Items
{
    [Serializable]
    public struct DefenceStatModifierComponent : IComponentData
    {
        public int Value;
    }
}
