using Unity.Entities;

namespace BeyondPixels.Components.Spells
{
    public struct SpellComponent : IComponentData
    {
        public Entity Caster;
        public float CoolDown;
    }
}
