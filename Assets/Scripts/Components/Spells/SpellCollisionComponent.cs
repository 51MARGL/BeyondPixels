using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.Components.Characters.Spells
{
    public struct SpellCollisionComponent : IComponentData
    {
        public Vector2 ImpactPoint;
        public float ImpactTime;
    }
}
