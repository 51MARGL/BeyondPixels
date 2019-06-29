using BeyondPixels.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;

using Unity.Collections;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon
{
    public class CleanUpSystem : ComponentSystem
    {
        private EntityQuery _boardGroup;
        private EntityQuery _tileGroup;
        private EntityQuery _tilemapGroup;

        protected override void OnCreate()
        {
            this._boardGroup = this.GetEntityQuery(
                ComponentType.ReadOnly(typeof(FinalBoardComponent)),
                ComponentType.ReadOnly(typeof(TilemapReadyComponent)),
                ComponentType.ReadOnly(typeof(EnemiesSpawnedComponent)),
                ComponentType.ReadOnly(typeof(LightsSpawnedComponent)),
                ComponentType.ReadOnly(typeof(PlayerSpawnedComponent)),
                ComponentType.ReadOnly(typeof(ExitSpawnedComponent)),
                ComponentType.ReadOnly(typeof(ChestSpawnedComponent)),
                ComponentType.ReadOnly(typeof(CageSpawnedComponent))
            );
            this._tileGroup = this.GetEntityQuery(
               ComponentType.ReadOnly(typeof(FinalTileComponent)));
            this._tilemapGroup = this.GetEntityQuery(
               ComponentType.ReadOnly(typeof(DungeonTileMapComponent)));
        }

        protected override void OnUpdate()
        {
            if (this._boardGroup.CalculateLength() == 0)
                return;

            this.DeleteAllEntities(this._boardGroup.CreateArchetypeChunkArray(Allocator.TempJob));
            this.DeleteAllEntities(this._tileGroup.CreateArchetypeChunkArray(Allocator.TempJob));

            this.Entities.With(this._tilemapGroup).ForEach((Entity entity) =>
            {
                this.PostUpdateCommands.AddComponent(entity, new Disabled());
            });
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
