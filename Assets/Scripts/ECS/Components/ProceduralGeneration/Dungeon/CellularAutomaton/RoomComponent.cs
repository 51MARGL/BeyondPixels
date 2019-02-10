﻿using Unity.Collections;
using Unity.Entities;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton
{
    public struct RoomComponent : IComponentData
    {
        public int IsAccessibleFromMainRoom;
        public int IsMainRoom;
        public int StartTileIndex;
        public int TileCount;
    }
}
