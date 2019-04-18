using System;
using Unity.Entities;

namespace BeyondPixels.ECS.Components.Items
{
    [Serializable]
    public struct ItemComponent : IComponentData
    {
        public int StoreIndex;
        public int IconIndex;
        public int Level;
    }
}
