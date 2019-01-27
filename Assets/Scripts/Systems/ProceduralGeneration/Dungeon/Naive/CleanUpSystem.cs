using BeyondPixels.Components.ProceduralGeneration.Dungeon.Naive;
using Unity.Entities;

namespace BeyondPixels.Systems.ProceduralGeneration.Dungeon.Naive
{
    public class CleanUpSystem : ComponentSystem
    {        
        private struct Data
        {
            public readonly int Length;
            public ComponentDataArray<BoardComponent> BoardComponents;
            public ComponentDataArray<BoardReadyComponent> BoardReadyComponents;
            public ComponentDataArray<TilemapReadyComponent> TilemapReadyComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private Data _data;
        private ComponentGroup _roomGroup;
        private ComponentGroup _corridorGroup;
        private ComponentGroup _tileGroup;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                var entity = _data.EntityArray[i];
                EntityManager.DestroyEntity(_roomGroup);
                EntityManager.DestroyEntity(_corridorGroup);
                EntityManager.DestroyEntity(_tileGroup);

                PostUpdateCommands.DestroyEntity(entity);
            }
        }

        protected override void OnCreateManager(int capacity)
        {
            _roomGroup = GetComponentGroup(
                ComponentType.ReadOnly(typeof(RoomComponent)));
            _corridorGroup = GetComponentGroup(
               ComponentType.ReadOnly(typeof(CorridorComponent)));
            _tileGroup = GetComponentGroup(
               ComponentType.ReadOnly(typeof(TileComponent)));
        }
    }
}
