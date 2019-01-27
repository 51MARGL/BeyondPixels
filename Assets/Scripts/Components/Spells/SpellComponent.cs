using Unity.Entities;

namespace BeyondPixels.Components.Characters.Spells
{
    public struct SpellComponent : IComponentData
    {
        public Entity Caster;
        public float CoolDown;
    }
}
