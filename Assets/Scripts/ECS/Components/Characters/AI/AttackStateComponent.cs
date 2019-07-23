using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.AI
{
    public struct AttackStateComponent : IComponentData
    {
        public float StartedAt;
        public Entity Target;
    }
}
