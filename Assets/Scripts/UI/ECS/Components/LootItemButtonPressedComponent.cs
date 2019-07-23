using Unity.Entities;

namespace BeyondPixels.UI.ECS.Components
{
    public struct LootItemButtonPressedComponent : IComponentData
    {
        public Entity ItemEntity;
    }
}
