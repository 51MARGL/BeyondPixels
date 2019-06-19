using BeyondPixels.ECS.Components.Game;
using BeyondPixels.ECS.Components.Scenes;
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
                if (!gameOverMenu.IsVisible)
                {
                    UIManager.Instance.CloseAllMenus();
                    var canvas = UIManager.Instance.Canvas.transform;
                    for (int i = 0; i < canvas.childCount; i++)
                        canvas.GetChild(i).gameObject.SetActive(false);

                    gameOverMenu.Show();
                }

                this.Entities.With(this._restartGroup).ForEach((Entity eventEntity) =>
                {
                    var loadEntity = this.PostUpdateCommands.CreateEntity();
                    this.PostUpdateCommands.AddComponent(loadEntity, new LoadLastGameComponent());

                    this.PostUpdateCommands.DestroyEntity(eventEntity);
                });

                this.Entities.With(this._quitGroup).ForEach((Entity eventEntity) =>
                {
                    this.PostUpdateCommands.DestroyEntity(eventEntity);

                    Application.Quit();
                });
            });
        }
    }
}
