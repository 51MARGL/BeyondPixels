using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using Cinemachine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Cutscenes
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class DungeonCameraInitializeSystem : ComponentSystem
    {
        private struct BoardCameraInitializedComponent : IComponentData { }

        private ComponentGroup _boardCameraGroup;

        protected override void OnCreateManager()
        {
            _boardCameraGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent)
                },
                None = new ComponentType[]
                {
                    typeof(TilemapReadyComponent), typeof(BoardCameraInitializedComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(_boardCameraGroup).ForEach((Entity entity, ref FinalBoardComponent finalBoardComponent) =>
            {
                var mapCamTargetGroup = new GameObject("TileMapCamTargetGroup").AddComponent<CinemachineTargetGroup>();
                var lb = new GameObject("camTarget1");
                var rb = new GameObject("camTarget2");
                var lt = new GameObject("camTarget3");
                var rt = new GameObject("camTarget4");
                var groupCamera = GameObject.Find("TileMapVCamera").GetComponent<CinemachineVirtualCamera>();
                lb.transform.position = float3.zero - 1;
                rb.transform.position = new float3(finalBoardComponent.Size.x, -1, 0);
                lt.transform.position = new float3(-1, finalBoardComponent.Size.y, 0);
                rt.transform.position = new float3(finalBoardComponent.Size.x, finalBoardComponent.Size.y, 0);

                mapCamTargetGroup.m_Targets = new CinemachineTargetGroup.Target[4];
                mapCamTargetGroup.m_Targets[0].target = lb.transform;
                mapCamTargetGroup.m_Targets[1].target = rb.transform;
                mapCamTargetGroup.m_Targets[2].target = lt.transform;
                mapCamTargetGroup.m_Targets[3].target = rt.transform;

                groupCamera.Follow = mapCamTargetGroup.transform;
                groupCamera.LookAt = mapCamTargetGroup.transform;

                PostUpdateCommands.AddComponent(entity, new BoardCameraInitializedComponent());
            });
        }
    }
}
