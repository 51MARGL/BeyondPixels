using System;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    public struct SpellCastingComponent : IComponentData
    {
        public int SpellIndex;
        public Entity ActiveSpell;
        public float StartedAt;
    }
}
