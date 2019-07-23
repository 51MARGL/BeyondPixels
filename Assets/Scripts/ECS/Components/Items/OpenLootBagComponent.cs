using Unity.Entities;

namespace BeyondPixels.ECS.Components.Items
{
    public struct OpenLootBagComponent : IComponentData
    {
        public int IsOpened;
    }
}
