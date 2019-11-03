using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    public struct WeaponComponent : IComponentData
    {
        public int DamageValue;
        public float MeleeAttackRange;
        public float CoolDown;
        public float SpellAttackRange;
    }
}
