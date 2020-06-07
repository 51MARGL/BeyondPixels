using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    public struct HitComponent : IComponentData
    {
        public Entity Victim;
    }
}
