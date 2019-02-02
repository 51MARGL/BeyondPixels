using Unity.Entities;

namespace BeyondPixels.Components.Spells
{
    public struct TargetRequiredComponent : IComponentData
    {
        public Entity Target;
    }
}