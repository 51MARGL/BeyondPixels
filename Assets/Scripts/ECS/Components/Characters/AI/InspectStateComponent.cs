using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Characters.AI
{
    public struct InspectStateComponent : IComponentData
    {
        public float StartedAt;
        public Vector2 InspectDirection;
    }
}
