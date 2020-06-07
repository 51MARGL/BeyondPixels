using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.ECS.Components.Scenes;

using System.Collections;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeyondPixels.ECS.Systems.Scenes
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SceneLoadSystem : ComponentSystem
    {
        private EntityQuery _sceneSwitchGroup;
        private EntityQuery _saveGameGroup;
        protected override void OnCreate()
        {
            this._sceneSwitchGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(SceneLoadComponent)
                }
            });
            this._saveGameGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(SaveGameComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            if (this._saveGameGroup.CalculateEntityCount() > 0)
            {
                return;
            }

            this.Entities.With(this._sceneSwitchGroup).ForEach((Entity entity, ref SceneLoadComponent sceneLoadComponent) =>
            {
                var index = sceneLoadComponent.SceneIndex;
                SceneFadeManager.Instance.OnFadeOutEvent += () =>
                {
                    SceneFadeManager.Instance.StartCoroutine(this.LoadSceneAsync(index));
                };
                SceneFadeManager.Instance.Animator.SetTrigger("FadeOut");
                Time.timeScale = 1f;
            });
            if (this._sceneSwitchGroup.CalculateEntityCount() > 0)
            {
                var entityArray = this.EntityManager.GetAllEntities(Allocator.TempJob);
                for (var i = 0; i < entityArray.Length; i++)
                {
                    this.PostUpdateCommands.DestroyEntity(entityArray[i]);
                }

                entityArray.Dispose();
            }
        }

        protected IEnumerator LoadSceneAsync(int index)
        {
            var asyncLoad = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }

            yield return new WaitForSeconds(1f);
            asyncLoad.allowSceneActivation = true;
        }
    }
}
