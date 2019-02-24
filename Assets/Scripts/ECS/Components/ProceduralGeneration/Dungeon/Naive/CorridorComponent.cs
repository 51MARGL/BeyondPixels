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
        public int EndX
        {
            get
            {
                if (Direction == Direction.Up || Direction == Direction.Down)
                    return StartX;
                if (Direction == Direction.Left)
                    return StartX + Length - 1;
                return StartX - Length + 1;
            }
        }


        public int EndY
        {
            get
            {
                if (Direction == Direction.Left || Direction == Direction.Right)
                    return StartY;
                if (Direction == Direction.Up)
                    return StartY + Length - 1;
                return StartY - Length + 1;
            }
        }
    }

    public enum Direction
    {
        Up, Left, Down, Right
    }
}
