﻿using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton;
using Unity.Collections;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon.CellularAutomaton
{
    public class CleanUpSystem : ComponentSystem
    {        
        private ComponentGroup _boardGroup;
        private ComponentGroup _roomGroup;
        private ComponentGroup _tileGroup;

        protected override void OnCreateManager()
        {
            _boardGroup = GetComponentGroup(
                ComponentType.ReadOnly(typeof(BoardComponent)),
                ComponentType.ReadOnly(typeof(BoardReadyComponent)),
                ComponentType.ReadOnly(typeof(TilemapReadyComponent))
            );
            _roomGroup = GetComponentGroup(
                ComponentType.ReadOnly(typeof(RoomComponent)));            
            _tileGroup = GetComponentGroup(
               ComponentType.ReadOnly(typeof(TileComponent)));
        }

        protected override void OnUpdate()
        {
            if (_boardGroup.CalculateLength() == 0)
                return;

            DeleteAllEntities(this._boardGroup.CreateArchetypeChunkArray(Allocator.TempJob));
            DeleteAllEntities(this._roomGroup.CreateArchetypeChunkArray(Allocator.TempJob));
            DeleteAllEntities(this._tileGroup.CreateArchetypeChunkArray(Allocator.TempJob));
        }

        private void DeleteAllEntities(NativeArray<ArchetypeChunk> chunks)
        {
            var entityType = GetArchetypeChunkEntityType();

            for (int chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
            {
                var chunk = chunks[chunkIndex];
                var entities = chunk.GetNativeArray(entityType);

                for (int i = 0; i < chunk.Count; i++)
                    PostUpdateCommands.DestroyEntity(entities[i]);
            }

            chunks.Dispose();
        }
    }
}