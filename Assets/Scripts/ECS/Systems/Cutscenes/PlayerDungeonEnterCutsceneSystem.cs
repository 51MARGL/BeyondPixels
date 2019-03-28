using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Cutscenes;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Playables;

namespace BeyondPixels.ECS.Systems.Cutscenes
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class PlayerDungeonEnterCutsceneSystem : ComponentSystem
    {
        private struct PlayerEnterCutsceneTriggeredComponent : IComponentData { }

        private ComponentGroup _boardCameraGroup;
        private ComponentGroup _playerGroup;
        private ComponentGroup _playerDoneCutSceneGroup;
        private bool cutsceneDone;
        protected override void OnCreateManager()
        {
            _boardCameraGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent), typeof(PlayerSpawnedComponent), typeof(EnemiesSpawnedComponent)
                },
                None = new ComponentType[]
                {
                    typeof(PlayerEnterCutsceneTriggeredComponent)
                }
            });
            _playerGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(Transform), typeof(PlayerComponent), typeof(Rigidbody2D)
                },
                None = new ComponentType[]
                {
                    typeof(InCutsceneComponent)
                }
            });
            _playerDoneCutSceneGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(InCutsceneComponent), typeof(PlayerComponent), typeof(Rigidbody2D)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(_boardCameraGroup).ForEach((Entity boardEntity, ref FinalBoardComponent finalBoardComponent) =>
            {
                Entities.With(_playerGroup).ForEach((Entity playerEntity, Transform transform, Rigidbody2D rigidbody) =>
                {
                    var player = transform.gameObject;
                    var director = TimelinesManagerComponent.Instance.Timelines.PlayerDungeonEnter;
                    void onStop(PlayableDirector aDirector)
                    {
                        cutsceneDone = true;
                        rigidbody.isKinematic = false;
                        director.stopped -= onStop;
                    }
                    director.stopped += onStop;

                    foreach (var playableAssetOutput in director.playableAsset.outputs)
                    {
                        if (playableAssetOutput.streamName == "PlayerAnim")
                        {
                            director.SetGenericBinding(playableAssetOutput.sourceObject, player);
                        }
                        else if (playableAssetOutput.streamName == "PlayerMove")
                        {
                            director.SetGenericBinding(playableAssetOutput.sourceObject, player);
                        }
                    }
                    cutsceneDone = false;
                    rigidbody.isKinematic = true;
                    PostUpdateCommands.AddComponent(playerEntity, new InCutsceneComponent());
                    director.Play();
                });
                PostUpdateCommands.AddComponent(boardEntity, new PlayerEnterCutsceneTriggeredComponent());
            });

            Entities.With(_playerDoneCutSceneGroup).ForEach((Entity playerEntity, Transform transform, Rigidbody2D rigidbody) =>
            {
                if (cutsceneDone)
                    PostUpdateCommands.RemoveComponent<InCutsceneComponent>(playerEntity);
            });
        }
    }
}
