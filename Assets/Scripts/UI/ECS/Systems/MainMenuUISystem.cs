using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Game;
using BeyondPixels.UI.ECS.Components;
using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.UI.ECS.Systems
{
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class MainMenuUISystem : ComponentSystem
    {
        private EntityQuery _cutsceneGroup;
        private EntityQuery _resumeGroup;
        private EntityQuery _newGameGroup;
        private EntityQuery _restartGroup;
        private EntityQuery _optionsGroup;
        private EntityQuery _quitGroup;

        protected override void OnCreateManager()
        {
            this._cutsceneGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(InCutsceneComponent)
                }
            });
            this._resumeGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(ResumeButtonPressedComponent)
                }
            });
            this._newGameGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(NewGameButtonPressedComponent)
                }
            });
            this._restartGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(LoadLastButtonPressedComponent)
                }
            });
            this._optionsGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(OptionsButtonPressedComponent)
                }
            });
            this._quitGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(QuitButtonPressedComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            if (this._cutsceneGroup.CalculateLength() > 0)
            {
                UIManager.Instance.CloseAllMenus();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape) && UIManager.Instance.MainMenu.InGameMenu)
            {
                if (UIManager.Instance.CurrentYesNoDialog != null)
                    return;

                if (UIManager.Instance.MainMenu.IgnoreEsc)
                {
                    UIManager.Instance.MainMenu.IgnoreEsc = false;
                    return;
                }

                var mainMenu = UIManager.Instance.MainMenu;
                if (!mainMenu.IsVisible)
                {
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
                    var startEntity = this.PostUpdateCommands.CreateEntity();
                    this.PostUpdateCommands.AddComponent(startEntity, new StartNewGameComponent());

                    mainMenu.Hide();
                    this.PostUpdateCommands.DestroyEntity(eventEntity);
                });

                this.Entities.With(this._restartGroup).ForEach((Entity eventEntity) =>
                {
                    var sceneLoadEntity = this.PostUpdateCommands.CreateEntity();
                    this.PostUpdateCommands.AddComponent(sceneLoadEntity, new LoadLastGameComponent());

                    mainMenu.Hide();
                    this.PostUpdateCommands.DestroyEntity(eventEntity);
                });

                this.Entities.With(this._optionsGroup).ForEach((Entity eventEntity) =>
                {
                    UIManager.Instance.OptionsMenu.Show();
                    this.PostUpdateCommands.DestroyEntity(eventEntity);
                });

                this.Entities.With(this._quitGroup).ForEach((Entity eventEntity) =>
                {
                    var quitEntity = this.PostUpdateCommands.CreateEntity();
                    this.PostUpdateCommands.AddComponent(quitEntity, new QuitGameComponent());

                    this.PostUpdateCommands.DestroyEntity(eventEntity);
                });
            }
        }
    }
}
