using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.AI
{
    public struct FollowStateComponent : IComponentData
    {
        public Entity Target;
        public float LastTimeAttacked;
        public float LastTimeSpellChecked;
    }
}
