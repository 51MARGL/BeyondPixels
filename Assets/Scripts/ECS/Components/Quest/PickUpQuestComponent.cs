using BeyondPixels.ECS.Components.Items;
using Unity.Entities;

namespace BeyondPixels.ECS.Components.Quest
{
    public struct PickUpQuestComponent : IComponentData
    {
        public ItemType ItemType;
    }
}
