using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton
{
    public struct TileComponent : IComponentData
    {
        public int2 Position;
        public TileType CurrentGenState;
        public TileType NextGenState;
    }

    public enum TileType
    {
        Wall = 0,
        Floor = 1
    }
}
