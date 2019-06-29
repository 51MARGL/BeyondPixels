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
        private EntityQuery _gameOverGroup;
        private EntityQuery _restartGroup;
        private EntityQuery _quitGroup;

        protected override void OnCreate()
        {
            this._gameOverGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(GameOverComponent)
                }
            });
            this._restartGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(LoadLastButtonPressedComponent)
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
                    var quitEntity = this.PostUpdateCommands.CreateEntity();
                    this.PostUpdateCommands.AddComponent(quitEntity, new QuitGameComponent());

                    this.PostUpdateCommands.DestroyEntity(eventEntity);
                });
            });
        }
    }
}
