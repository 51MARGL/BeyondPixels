using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.BSP;

using Unity.Collections;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon.BSP
{
    public class CleanUpSystem : ComponentSystem
    {
        private ComponentGroup _boardGroup;
        private ComponentGroup _roomGroup;
        private ComponentGroup _corridorGroup;
        private ComponentGroup _tileGroup;

        protected override void OnCreateManager()
        {
            this._boardGroup = this.GetComponentGroup(
                ComponentType.ReadOnly(typeof(BoardComponent)),
                ComponentType.ReadOnly(typeof(BoardReadyComponent))
            );
            this._roomGroup = this.GetComponentGroup(
                ComponentType.ReadOnly(typeof(RoomComponent)));
            this._corridorGroup = this.GetComponentGroup(
               ComponentType.ReadOnly(typeof(CorridorComponent)));
            this._tileGroup = this.GetComponentGroup(
               ComponentType.ReadOnly(typeof(TileComponent)));
        }

        protected override void OnUpdate()
        {
            if (this._boardGroup.CalculateLength() == 0)
                return;

            this.DeleteAllEntities(this._boardGroup.CreateArchetypeChunkArray(Allocator.TempJob));
            this.DeleteAllEntities(this._roomGroup.CreateArchetypeChunkArray(Allocator.TempJob));
            this.DeleteAllEntities(this._corridorGroup.CreateArchetypeChunkArray(Allocator.TempJob));
            this.DeleteAllEntities(this._tileGroup.CreateArchetypeChunkArray(Allocator.TempJob));
        }

        private void DeleteAllEntities(NativeArray<ArchetypeChunk> chunks)
        {
            var entityType = this.GetArchetypeChunkEntityType();

            for (var chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
            {
                var chunk = chunks[chunkIndex];
                var entities = chunk.GetNativeArray(entityType);

                for (var i = 0; i < chunk.Count; i++)
                    this.PostUpdateCommands.DestroyEntity(entities[i]);
            }

            chunks.Dispose();
        }
    }
}
