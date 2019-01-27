using System;
using Unity.Entities;

namespace BeyondPixels.Components.Characters.Common
{
    public struct DamageComponent : IComponentData
    {
        public int DamageValue;
        public DamageType DamageType;
    }

    public enum DamageType
    {
        Weapon = 0,
        Fire = 1,
        Ice = 2,
        Magic = 3
    }
}
