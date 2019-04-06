using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Cutscenes;
using BeyondPixels.ECS.Components.Objects;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace BeyondPixels.ECS.Systems.Cutscenes
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class PlayerDungeonExitCutsceneSystem : ComponentSystem
    {
        private struct PlayerExitCutscenePlaying : IComponentData { }

        private ComponentGroup _triggerCutsceneGroup;
        private ComponentGroup _playerDoneCutSceneGroup;
        private bool cutsceneDone;
        protected override void OnCreateManager()
        {
            _triggerCutsceneGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(PlayerExitCutsceneComponent)
                },
                None = new ComponentType[]
                {
                    typeof(DestroyComponent)
                }
            });
            _playerDoneCutSceneGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(InCutsceneComponent), typeof(PlayerExitCutscenePlaying),
                    typeof(PlayerComponent), typeof(Rigidbody2D)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(_triggerCutsceneGroup).ForEach((Entity triggerEntity, PlayerExitCutsceneComponent playerExitCutsceneComponent) =>
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                var playerEntity = player.GetComponent<GameObjectEntity>().Entity;
                var rigidbody = player.GetComponent<Rigidbody2D>();
                var director = TimelinesManagerComponent.Instance.Timelines.PlayerDungeonExit;
                var levelExit = playerExitCutsceneComponent.ExitCaveDoor;

                var playerPosition = new float2(player.transform.position.x, player.transform.position.y);
                var desiredPosition = new float2(levelExit.transform.position.x, levelExit.transform.position.y);
                desiredPosition.y -= 1f;

                if (!EntityManager.HasComponent<InCutsceneComponent>(playerEntity))
                    PostUpdateCommands.AddComponent(playerEntity, new InCutsceneComponent());

                var movementComponent = EntityManager.GetComponentData<MovementComponent>(playerEntity);
                if (math.abs(playerPosition.x - desiredPosition.x) > 0.05f
                    || math.abs(playerPosition.y - desiredPosition.y) > 0.05f)
                {
                    movementComponent.Direction = desiredPosition - playerPosition;
                    PostUpdateCommands.SetComponent(playerEntity, movementComponent);
                    return;
                }
                movementComponent.Direction = float2.zero;
                PostUpdateCommands.SetComponent(playerEntity, movementComponent);

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
                        director.SetGenericBinding(playableAssetOutput.sourceObject, levelExit);
                    }
                }
                cutsceneDone = false;
                rigidbody.isKinematic = true;
                PostUpdateCommands.AddComponent(playerEntity, new PlayerExitCutscenePlaying());
                director.Play();
                PostUpdateCommands.AddComponent(triggerEntity, new DestroyComponent());
            });

            Entities.With(_playerDoneCutSceneGroup).ForEach((Entity playerEntity, Transform transform, Rigidbody2D rigidbody) =>
            {
                if (cutsceneDone)
                {
                    PostUpdateCommands.RemoveComponent<InCutsceneComponent>(playerEntity);
                    PostUpdateCommands.RemoveComponent<PlayerExitCutscenePlaying>(playerEntity);
                    SceneManager.LoadScene("DungeonScene", LoadSceneMode.Single);
                }
            });
        }
    }
}
