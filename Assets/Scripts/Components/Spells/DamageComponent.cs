using BeyondPixels.Components.Characters.Common;
using Unity.Entities;

namespace BeyondPixels.Components.Characters.Spells
{
    public struct DamageComponent : IComponentData
    {
        public int DamageOnImpact;
        public int DamagePerSecond;
        public DamageType DamageType;
    }
}
