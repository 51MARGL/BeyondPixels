using Unity.Entities;

namespace BeyondPixels.UI.ECS.Components
{
    public struct AddStatButtonPressedComponent : IComponentData
    {
        public StatTarget StatTarget;
    }

    public enum StatTarget
    {
        HealthStat = 1,
        AttackStat = 2,
        DefenceStat = 3,
        MagicStat = 4
    }
}
