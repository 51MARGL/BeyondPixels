using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;

using Cinemachine;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Scenes
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class DungeonCameraInitializeSystem : ComponentSystem
    {
        private struct BoardCameraInitializedComponent : IComponentData { }

        private EntityQuery _boardCameraGroup;

        protected override void OnCreate()
        {
            this._boardCameraGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
{
                    typeof(FinalBoardComponent)
},
                None = new ComponentType[]
{
                    typeof(BoardCameraInitializedComponent)
}
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._boardCameraGroup).ForEach((Entity entity, ref FinalBoardComponent finalBoardComponent) =>
            {
                var mapCamTargetGroup = new GameObject("TileMapCamTargetGroup").AddComponent<CinemachineTargetGroup>();
                var lb = new GameObject("camTarget1");
                var rb = new GameObject("camTarget2");
                var lt = new GameObject("camTarget3");
                var rt = new GameObject("camTarget4");
                var groupCamera = GameObject.Find("TileMapVCamera").GetComponent<CinemachineVirtualCamera>();
                lb.transform.position = float3.zero - 5;
                rb.transform.position = new float3(finalBoardComponent.Size.x + 5, -5, 0);
                lt.transform.position = new float3(-1, finalBoardComponent.Size.y + 5, 0);
                rt.transform.position = new float3(finalBoardComponent.Size.x + 5, finalBoardComponent.Size.y + 5, 0);

                mapCamTargetGroup.m_Targets = new CinemachineTargetGroup.Target[4];
                mapCamTargetGroup.m_Targets[0].target = lb.transform;
                mapCamTargetGroup.m_Targets[1].target = rb.transform;
                mapCamTargetGroup.m_Targets[2].target = lt.transform;
                mapCamTargetGroup.m_Targets[3].target = rt.transform;

                groupCamera.Follow = mapCamTargetGroup.transform;
                groupCamera.LookAt = mapCamTargetGroup.transform;

                this.PostUpdateCommands.AddComponent(entity, new BoardCameraInitializedComponent());
            });
        }
    }
}
