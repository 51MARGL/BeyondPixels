using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    public struct HitComponent : IComponentData
    {
        public Entity Victim;
    }
}
