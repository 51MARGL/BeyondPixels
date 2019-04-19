using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    public struct SpellCastingComponent : IComponentData
    {
        public int SpellIndex;
        public Entity ActiveSpell;
        public float StartedAt;
    }
}
