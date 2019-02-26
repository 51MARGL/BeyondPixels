using Unity.Entities;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.BSP
{
    public struct CorridorComponent : IComponentData
    {
        public TileComponent Start;
        public TileComponent End;
    }
}
