using System;

using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Level
{
    [Serializable]
    public struct XPRewardComponent : IComponentData
    {
        public int XPAmount;
    }
}
