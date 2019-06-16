﻿using BeyondPixels.ECS.Components.Scenes;
using BeyondPixels.UI.ECS.Components;
using Unity.Entities;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeyondPixels.UI.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class GameOverUISystem : ComponentSystem
    {
        private ComponentGroup _gameOverGroup;
        private ComponentGroup _restartGroup;
        private ComponentGroup _quitGroup;

        protected override void OnCreateManager()
        {
            this._gameOverGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(GameOverComponent)
                }
            });
            this._restartGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(LoadLastButtonPressedComponent)
                }
            });
            this._quitGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(QuitButtonPressedComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._gameOverGroup).ForEach((Entity entity) =>
            {
                var gameOverMenu = UIManager.Instance.GameOverMenu;
                if (gameOverMenu.GetComponent<CanvasGroup>().alpha == 0)
                {
                    UIManager.Instance.CloseAllMenus();
                    var canvas = UIManager.Instance.transform.GetChild(0);
                    for (int i = 0; i < canvas.childCount; i++)
                    {
                        var child = canvas.GetChild(i);
                        if (child.name != gameOverMenu.name)
                            child.gameObject.SetActive(false);
                    }

                    gameOverMenu.GetComponent<CanvasGroup>().alpha = 1;
                    gameOverMenu.GetComponent<CanvasGroup>().blocksRaycasts = true;
                }
            });

            this.Entities.With(this._restartGroup).ForEach((Entity eventEntity) =>
            {
                var sceneLoadEntity = this.PostUpdateCommands.CreateEntity();
                this.PostUpdateCommands.AddComponent(sceneLoadEntity, new SceneLoadComponent
                {
                    SceneIndex = SceneManager.GetActiveScene().buildIndex
                });

                this.PostUpdateCommands.DestroyEntity(eventEntity);
            });

            this.Entities.With(this._quitGroup).ForEach((Entity eventEntity) =>
            {
                Application.Quit();

                this.PostUpdateCommands.DestroyEntity(eventEntity);
            });
        }
    }
}
