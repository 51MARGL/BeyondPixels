using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.Spells
{
    public struct InstantiateSpellComponent : IComponentData
    {
        public Entity Caster;
        public Entity Target;
    }
}
