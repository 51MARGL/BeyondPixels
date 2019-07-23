using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Player
{
    public struct TargetComponent : IComponentData
    {
        public Entity Target;
    }
}
