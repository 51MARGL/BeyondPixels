using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.Components.Spells
{
    public struct SpellCollisionComponent : IComponentData
    {
        public Entity SpellEntity;
        public Vector2 ImpactPoint;
        public float ImpactTime;
    }
}
