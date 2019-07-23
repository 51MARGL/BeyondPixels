using Unity.Entities;

namespace BeyondPixels.ECS.Components.Items
{
    public struct PickUpComponent : IComponentData
    {
        public Entity ItemEntity;
        public Entity Owner;
    }
}
