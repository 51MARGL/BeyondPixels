using System;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.Components.Characters.Common
{
    public struct SpellCastingComponent : IComponentData
    {
        public int SpellIndex;
        public float StartedAt;
    }
}
