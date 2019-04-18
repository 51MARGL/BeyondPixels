using Unity.Entities;

namespace BeyondPixels.ECS.Components.Items
{
    public struct PickedUpComponent : IComponentData
    {
        public Entity Owner;
    }
}
