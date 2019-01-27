using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.Components.ProceduralGeneration.Dungeon.Naive
{
    public struct BoardComponent : IComponentData
    {
        public int2 Size;            
        public int RoomCount;
        public int RoomSize;
        public int MinCorridorLength;
        public int MaxCorridorLength;
    }
}
