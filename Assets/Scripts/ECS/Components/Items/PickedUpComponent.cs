using System;
using Unity.Entities;

namespace BeyondPixels.ECS.Components.Items
{
    [Serializable]
    public struct PickedUpComponent : IComponentData
    {
        public Entity Owner;
    }
}
