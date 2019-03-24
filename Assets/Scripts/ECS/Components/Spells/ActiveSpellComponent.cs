using Unity.Entities;

namespace BeyondPixels.ECS.Components.Spells
{
    public struct ActiveSpellComponent : IComponentData
    {
        public Entity Owner;
        public int SpellIndex;
        public int ActionIndex;
    }
}
