using Unity.Entities;

namespace BeyondPixels.ECS.Components.Spells
{
    public struct TargetRequiredComponent : IComponentData
    {
        public Entity Target;
    }
}