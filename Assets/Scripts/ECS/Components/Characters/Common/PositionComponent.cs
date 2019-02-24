using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    public struct PositionComponent : IComponentData
    {
        public float2 CurrentPosition;
        public float2 InitialPosition;
    }
}
