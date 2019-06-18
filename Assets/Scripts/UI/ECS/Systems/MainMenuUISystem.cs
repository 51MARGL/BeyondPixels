﻿using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.ECS.Components.Scenes;
using BeyondPixels.UI.ECS.Components;
using Unity.Entities;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeyondPixels.UI.ECS.Systems
{
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class MainMenuUISystem : ComponentSystem
    {
        private ComponentGroup _cutsceneGroup;
        private ComponentGroup _resumeGroup;
        private ComponentGroup _newGameGroup;
        private ComponentGroup _restartGroup;
        private ComponentGroup _optionsGroup;
        private ComponentGroup _quitGroup;

        protected override void OnCreateManager()
        {
            this._cutsceneGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(InCutsceneComponent)
                }
            });
            this._resumeGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(ResumeButtonPressedComponent)
                }
            });
            this._newGameGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(NewGameButtonPressedComponent)
                }
            });
            this._restartGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(LoadLastButtonPressedComponent)
                }
            });
            this._optionsGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(OptionsButtonPressedComponent)
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
            if (_cutsceneGroup.CalculateLength() > 0)
            {
                UIManager.Instance.CloseAllMenus();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var mainMenu = UIManager.Instance.MainMenu;
                if (!mainMenu.IsVisible)
                {
                    UIManager.Instance.CloseAllMenus();
                    mainMenu.Show();
                }
                else
                {
                    mainMenu.Hide();
                }
            }
            if (UIManager.Instance.MainMenu.IsVisible)
            {
                var mainMenu = UIManager.Instance.MainMenu;
                this.Entities.With(this._resumeGroup).ForEach((Entity eventEntity) =>
                {
                    mainMenu.Hide();

                    this.PostUpdateCommands.DestroyEntity(eventEntity);
                });

                this.Entities.With(this._newGameGroup).ForEach((Entity eventEntity) =>
                {
                    var sceneLoadEntity = this.PostUpdateCommands.CreateEntity();
                    this.PostUpdateCommands.AddComponent(sceneLoadEntity, new StartNewGameComponent());

                    mainMenu.Hide();
                    this.PostUpdateCommands.DestroyEntity(eventEntity);
                });

                this.Entities.With(this._restartGroup).ForEach((Entity eventEntity) =>
                {
                    var sceneLoadEntity = this.PostUpdateCommands.CreateEntity();
                    this.PostUpdateCommands.AddComponent(sceneLoadEntity, new SceneLoadComponent
                    {
                        SceneIndex = SceneManager.GetActiveScene().buildIndex
                    });

                    mainMenu.Hide();
                    this.PostUpdateCommands.DestroyEntity(eventEntity);
                });

                this.Entities.With(this._optionsGroup).ForEach((Entity eventEntity) =>
                {

                    mainMenu.Hide();
                    this.PostUpdateCommands.DestroyEntity(eventEntity);
                });

                this.Entities.With(this._quitGroup).ForEach((Entity eventEntity) =>
                {
                    mainMenu.Hide();
                    this.PostUpdateCommands.DestroyEntity(eventEntity);

                    Application.Quit();
                });
            }
        }
    }
}
