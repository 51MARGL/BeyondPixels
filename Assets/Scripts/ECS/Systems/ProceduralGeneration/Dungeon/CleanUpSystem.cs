﻿using BeyondPixels.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using Unity.Collections;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon
{
    public class CleanUpSystem : ComponentSystem
    {
        private ComponentGroup _boardGroup;
        private ComponentGroup _tileGroup;
        private ComponentGroup _tilemapGroup;

        protected override void OnCreateManager()
        {
            _boardGroup = GetComponentGroup(
                ComponentType.ReadOnly(typeof(FinalBoardComponent)),
                ComponentType.ReadOnly(typeof(TilemapReadyComponent)),
                ComponentType.ReadOnly(typeof(EnemiesSpawnedComponent)),
                ComponentType.ReadOnly(typeof(LightsSpawnedComponent)),
                ComponentType.ReadOnly(typeof(PlayerSpawnedComponent))
            );
            _tileGroup = GetComponentGroup(
               ComponentType.ReadOnly(typeof(FinalTileComponent)));
            _tilemapGroup = GetComponentGroup(
               ComponentType.ReadOnly(typeof(DungeonTileMapComponent)));
        }

        protected override void OnUpdate()
        {
            if (_boardGroup.CalculateLength() == 0)
                return;

            DeleteAllEntities(this._boardGroup.CreateArchetypeChunkArray(Allocator.TempJob));
            DeleteAllEntities(this._tileGroup.CreateArchetypeChunkArray(Allocator.TempJob));

            Entities.With(_tilemapGroup).ForEach((Entity entity) =>
            {
                PostUpdateCommands.AddComponent(entity, new Disabled());
            });
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
