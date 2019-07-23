using Unity.Entities;

namespace BeyondPixels.ECS.Components.Objects
{
    public struct EntityEnableComponent : IComponentData
    {
        public Entity Target;
    }
}
