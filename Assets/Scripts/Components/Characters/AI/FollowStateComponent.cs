using Unity.Entities;

namespace BeyondPixels.Components.Characters.AI
{
    public struct FollowStateComponent : IComponentData
    {
        public Entity Target;
        public float LastTimeAttacked;
    }
}
