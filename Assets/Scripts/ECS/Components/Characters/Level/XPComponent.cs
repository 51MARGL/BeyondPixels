using System;

using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Level
{
    [Serializable]
    public struct XPComponent : IComponentData
    {
        public int CurrentXP;
    }
}
