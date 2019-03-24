using Unity.Entities;

namespace BeyondPixels.ECS.Components.Spells
{
    public struct CoolDownComponent : IComponentData
    {
        public float CoolDownTime;
    }
}
