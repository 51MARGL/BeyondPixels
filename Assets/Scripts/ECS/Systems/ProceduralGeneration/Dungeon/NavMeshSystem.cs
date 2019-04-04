using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.NavMesh;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon
{
    public class NavMeshSystem : ComponentSystem
    {
        private ComponentGroup _navMeshGroup;
        private ComponentGroup _boardGroup;
        private ComponentGroup _tilesGroup;

        protected override void OnCreateManager()
        {
            _boardGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent)
                }
            });
            _navMeshGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(NavMeshComponent), typeof(Transform)
                }
            });
            _tilesGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalTileComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(_boardGroup).ForEach((Entity entity, ref FinalBoardComponent finalBoardComponent) =>
            {
                GenerateNavMesh(finalBoardComponent.Size, entity);
            });
        }

        private void GenerateNavMesh(int2 boardSize, Entity boardEntity)
        {
            Entities.With(_navMeshGroup).ForEach((Entity entity, NavMeshComponent navMeshComponent, Transform transform) =>
            {
                navMeshComponent.NavMeshGround.transform.localPosition = new Vector3(boardSize.x / 2f, 0, boardSize.y / 2f);
                navMeshComponent.NavMeshGround.transform.localScale = new Vector3(boardSize.x - 1, 1, boardSize.y - 1);
                Entities.With(_tilesGroup).ForEach((ref FinalTileComponent finalTileComponent) =>
                {
                    if (finalTileComponent.TileType == TileType.Floor)
                        return;

                    var wall = Object.Instantiate(navMeshComponent.NavMeshWall, Vector3.zero,
                        Quaternion.identity, transform);
                    wall.transform.localPosition = new Vector3(finalTileComponent.Position.x, 0.5f, finalTileComponent.Position.y);
                });

                navMeshComponent.NavMeshSurface.BuildNavMesh();

                var count = transform.childCount;
                for (int i = 1; i < count; i++)
                    Object.Destroy(transform.GetChild(i).gameObject);

                PostUpdateCommands.AddComponent(entity, new Disabled());
            });
        }
    }
}
