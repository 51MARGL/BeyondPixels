using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.Components.ProceduralGeneration.Dungeon.Naive
{
    public struct RoomComponent : IComponentData
    {
        public int X;                  
        public int Y;
        public int2 Size;            
        public Direction EnteringCorridor;
    }
}
