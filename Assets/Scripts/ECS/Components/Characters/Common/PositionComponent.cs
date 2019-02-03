using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    public struct PositionComponent : IComponentData
    {
        public Vector2 CurrentPosition;
        public Vector2 InitialPosition;
    }
}
