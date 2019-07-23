using Unity.Entities;

namespace BeyondPixels.ECS.Components.Spells
{
    public struct InstantiateSpellComponent : IComponentData
    {
        public Entity Caster;
        public Entity Target;
    }
}
