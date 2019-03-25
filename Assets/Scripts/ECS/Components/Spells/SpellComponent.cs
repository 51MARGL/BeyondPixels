using Unity.Entities;

namespace BeyondPixels.ECS.Components.Spells
{
    public struct SpellComponent : IComponentData
    {
        public Entity Caster;
    }
}
