using Unity.Entities;

namespace BeyondPixels.Components.Characters.Spells
{
    public struct TargetRequiredComponent : IComponentData
    {
        public Entity Target;
    }
}