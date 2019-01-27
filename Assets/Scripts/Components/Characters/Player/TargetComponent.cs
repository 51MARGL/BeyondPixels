using System;
using Unity.Entities;

namespace BeyondPixels.Components.Characters.Player
{
    public struct TargetComponent : IComponentData
    {
        public Entity Target;
    }
}
