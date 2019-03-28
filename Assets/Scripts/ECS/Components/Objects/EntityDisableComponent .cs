using Unity.Entities;

namespace BeyondPixels.ECS.Components.Objects
{
    public struct EntityDisableComponent : IComponentData
    {
        public Entity Target;
    }
}
