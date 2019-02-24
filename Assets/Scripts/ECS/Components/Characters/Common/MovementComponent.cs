using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    public struct MovementComponent : IComponentData
    {
        public float2 Direction;
        public float Speed;
    }
}
