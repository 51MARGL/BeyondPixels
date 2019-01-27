using System;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.Components.Characters.Player
{
    public struct AttackComponent : IComponentData
    {
        public int CurrentComboIndex;        
    } 
}
