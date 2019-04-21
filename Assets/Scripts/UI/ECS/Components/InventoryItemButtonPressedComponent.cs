using Unity.Entities;

namespace BeyondPixels.UI.ECS.Components
{
    public struct InventoryItemButtonPressedComponent : IComponentData
    {
        public Entity ItemEntity;
        public int MouseButton;
    }
}
