using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.Naive
{
    public struct CorridorComponent : IComponentData
    {
        public int StartX;      
        public int StartY;      
        public int Length; 
        public Direction Direction;

        // Get the end position of the corridor based on it's start position and which direction it's heading.
        public int EndPositionX
        {
            get
            {
                if (Direction == Direction.North || Direction == Direction.South)
                    return StartX;
                if (Direction == Direction.East)
                    return StartX + Length - 1;
                return StartX - Length + 1;
            }
        }


        public int EndPositionY
        {
            get
            {
                if (Direction == Direction.East || Direction == Direction.West)
                    return StartY;
                if (Direction == Direction.North)
                    return StartY + Length - 1;
                return StartY - Length + 1;
            }
        }
    }

    public enum Direction
    {
        North, East, South, West
    }
}
