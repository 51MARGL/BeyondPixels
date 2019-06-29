using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.ECS.Components.Scenes;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace BeyondPixels.ECS.Systems.Scenes
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class PlayerDungeonExitCutsceneSystem : ComponentSystem
    {
        private struct PlayerExitCutscenePlaying : IComponentData { }

        private EntityQuery _triggerCutsceneGroup;
        private EntityQuery _playerDoneCutSceneGroup;
        private bool cutsceneDone;
        protected override void OnCreate()
        {
            this._triggerCutsceneGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PlayerExitCutsceneComponent), typeof(PositionComponent), typeof(Transform)
                },
                None = new ComponentType[]
                {
                    typeof(DestroyComponent)
                }
            });
            this._playerDoneCutSceneGroup = this.GetEntityQuery(new EntityQueryDesc
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
            this.Entities.With(this._triggerCutsceneGroup).ForEach((Entity exitEntity, Transform levelExitTransform, ref PositionComponent exitPositionComponent) =>
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                var playerEntity = player.GetComponent<GameObjectEntity>().Entity;
                var rigidbody = player.GetComponent<Rigidbody2D>();
                var director = TimelinesManagerComponent.Instance.Timelines.PlayerDungeonExit;
                var levelExit = levelExitTransform.GetChild(0).gameObject;

                var playerPosition = new float2(player.transform.position.x, player.transform.position.y);
                var desiredPosition = exitPositionComponent.CurrentPosition;
                desiredPosition.y -= 1f;

                if (!this.EntityManager.HasComponent<InCutsceneComponent>(playerEntity))
                    this.PostUpdateCommands.AddComponent(playerEntity, new InCutsceneComponent());

                var movementComponent = this.EntityManager.GetComponentData<MovementComponent>(playerEntity);
                if (math.abs(playerPosition.x - desiredPosition.x) > 0.05f
                    || math.abs(playerPosition.y - desiredPosition.y) > 0.05f)
                {
                    movementComponent.Direction = desiredPosition - playerPosition;
                    this.PostUpdateCommands.SetComponent(playerEntity, movementComponent);
                    return;
                }
                movementComponent.Direction = float2.zero;
                this.PostUpdateCommands.SetComponent(playerEntity, movementComponent);

                if (!director.enabled)
                    director.enabled = true;

                void onStop(PlayableDirector aDirector)
                {
                    this.cutsceneDone = true;
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
                this.cutsceneDone = false;
                rigidbody.isKinematic = true;
                this.PostUpdateCommands.AddComponent(playerEntity, new PlayerExitCutscenePlaying());
                director.Play();
                this.PostUpdateCommands.RemoveComponent<PlayerExitCutsceneComponent>(exitEntity);
            });

            this.Entities.With(this._playerDoneCutSceneGroup).ForEach((Entity playerEntity, Transform transform, Rigidbody2D rigidbody) =>
            {
                if (this.cutsceneDone)
                {
                    this.PostUpdateCommands.RemoveComponent<InCutsceneComponent>(playerEntity);
                    this.PostUpdateCommands.RemoveComponent<PlayerExitCutscenePlaying>(playerEntity);

                    var saveGameEntity = this.PostUpdateCommands.CreateEntity();
                    this.PostUpdateCommands.AddComponent(saveGameEntity, new SaveGameComponent());

                    var sceneLoadEntity = this.PostUpdateCommands.CreateEntity();
                    this.PostUpdateCommands.AddComponent(sceneLoadEntity, new SceneLoadComponent
                    {
                        SceneIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/DungeonScene.unity")
                    });
                }
            });
        }
    }
}
