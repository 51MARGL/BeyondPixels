using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.Naive
{
    public struct TileComponent : IComponentData
    {
        public int2 Postition;
        public TileType TileType;
    }

    public enum TileType
    {
        Wall = 0,
        Floor = 1
    }
}
