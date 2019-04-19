using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    public struct DamageComponent : IComponentData
    {
        public int DamageOnImpact;
        public int DamagePerSecond;
        public DamageType DamageType;
    }

    public struct FinalDamageComponent : IComponentData
    {
        public float DamageAmount;
        public DamageType DamageType;
    }

    public enum DamageType
    {
        Weapon = 0,
        Magic = 1
    }
}
