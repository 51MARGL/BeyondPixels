using BeyondPixels.ECS.Components.Characters.Player;
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
    public class PlayerTutorialEnterCutsceneSystem : ComponentSystem
    {
        private struct PlayerEnterCutscenePlaying : IComponentData { }

        private EntityQuery _startGroup;
        private EntityQuery _playerDoneCutSceneGroup;
        private bool cutsceneDone;

        protected override void OnCreate()
        {
            this._startGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PlayerEnterTutorialComponent)
                }
            });
            this._playerDoneCutSceneGroup = this.GetEntityQuery(new EntityQueryDesc
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
            this.Entities.With(this._startGroup).ForEach((Entity startEntity) =>
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                var playerEntity = player.GetComponent<GameObjectEntity>().Entity;
                var rigidbody = player.GetComponent<Rigidbody2D>();
                var director = TimelinesManagerComponent.Instance.Timelines.PlayerTutorialEnter;
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

                this.cutsceneDone = false;
                rigidbody.isKinematic = true;
                if (!this.EntityManager.HasComponent<InCutsceneComponent>(playerEntity))
                    this.PostUpdateCommands.AddComponent(playerEntity, new InCutsceneComponent());
                this.PostUpdateCommands.AddComponent(playerEntity, new PlayerEnterCutscenePlaying());
                director.Play();

                this.PostUpdateCommands.DestroyEntity(startEntity);
            });

            this.Entities.With(this._playerDoneCutSceneGroup).ForEach((Entity playerEntity, Transform transform, Rigidbody2D rigidbody) =>
            {
                if (this.cutsceneDone)
                {
                    this.PostUpdateCommands.RemoveComponent<InCutsceneComponent>(playerEntity);
                    this.PostUpdateCommands.RemoveComponent<PlayerEnterCutscenePlaying>(playerEntity);

                    var trigger = GameObject.Find("EntryMessageTrigger");
                    trigger.GetComponent<BoxCollider2D>().enabled = true;
                }
            });
        }
    }
}
