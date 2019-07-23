using Unity.Entities;

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
                if (this.Direction == Direction.Up || this.Direction == Direction.Down)
                    return this.StartX;
                if (this.Direction == Direction.Left)
                    return this.StartX + this.Length - 1;
                return this.StartX - this.Length + 1;
            }
        }


        public int EndY
        {
            get
            {
                if (this.Direction == Direction.Left || this.Direction == Direction.Right)
                    return this.StartY;
                if (this.Direction == Direction.Up)
                    return this.StartY + this.Length - 1;
                return this.StartY - this.Length + 1;
            }
        }
    }

    public enum Direction
    {
        Up, Left, Down, Right
    }
}
