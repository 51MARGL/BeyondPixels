﻿using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.Naive
{
    public struct BoardComponent : IComponentData
    {
        public int2 Size;
        public int RoomCount;
        public int MaxRoomSize;
        public int MinCorridorLength;
        public int MaxCorridorLength;
        public uint RandomSeed;
    }
}
