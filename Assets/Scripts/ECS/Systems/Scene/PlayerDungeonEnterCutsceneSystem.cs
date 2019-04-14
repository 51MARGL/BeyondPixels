﻿using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.ECS.Components.Scenes;
using BeyondPixels.SceneBootstraps;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Playables;

namespace BeyondPixels.ECS.Systems.Scenes
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class PlayerDungeonEnterCutsceneSystem : ComponentSystem
    {
        private struct PlayerEnterCutsceneTriggeredComponent : IComponentData { }
        private struct PlayerEnterCutscenePlaying : IComponentData { }

        private ComponentGroup _boardCameraGroup;
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
            _playerDoneCutSceneGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(InCutsceneComponent), typeof(PlayerEnterCutscenePlaying),
                    typeof(PlayerComponent), typeof(Rigidbody2D)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(_boardCameraGroup).ForEach((Entity boardEntity, ref FinalBoardComponent finalBoardComponent) =>
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                var playerEntity = player.GetComponent<GameObjectEntity>().Entity;
                var rigidbody = player.GetComponent<Rigidbody2D>();
                var director = TimelinesManagerComponent.Instance.Timelines.PlayerDungeonEnter;
                var levelEnter = GameObject.Instantiate(PrefabManager.Instance.DungeonLevelEnter, player.transform.position, Quaternion.identity);
                if (!director.enabled)
                    director.enabled = true;

                void onStop(PlayableDirector aDirector)
                {
                    cutsceneDone = true;
                    rigidbody.isKinematic = false;
                    director.stopped -= onStop;
                    director.enabled = false;
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
                    else if (playableAssetOutput.streamName == "CaveFall")
                    {
                        director.SetGenericBinding(playableAssetOutput.sourceObject, levelEnter);
                    }
                }
                cutsceneDone = false;
                rigidbody.isKinematic = true;
                if (!EntityManager.HasComponent<InCutsceneComponent>(playerEntity))
                    PostUpdateCommands.AddComponent(playerEntity, new InCutsceneComponent());
                PostUpdateCommands.AddComponent(playerEntity, new PlayerEnterCutscenePlaying());
                director.Play();
                PostUpdateCommands.AddComponent(boardEntity, new PlayerEnterCutsceneTriggeredComponent());
            });

            Entities.With(_playerDoneCutSceneGroup).ForEach((Entity playerEntity, Transform transform, Rigidbody2D rigidbody) =>
            {
                if (cutsceneDone)
                {
                    PostUpdateCommands.RemoveComponent<InCutsceneComponent>(playerEntity);
                    PostUpdateCommands.RemoveComponent<PlayerEnterCutscenePlaying>(playerEntity);
                }
            });
        }
    }
}