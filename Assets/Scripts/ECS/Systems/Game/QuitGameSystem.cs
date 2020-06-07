using BeyondPixels.ECS.Components.Game;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Game
{
    public class QuitGameSystem : ComponentSystem
    {
        private EntityQuery _quitGroup;

        protected override void OnCreate()
        {
            this._quitGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(QuitGameComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._quitGroup).ForEach((Entity entity) =>
            {
                var sceneLoadEntity = this.PostUpdateCommands.CreateEntity();
                this.PostUpdateCommands.DestroyEntity(entity);

                Time.timeScale = 1f;
                Application.Quit();
            });
        }
    }
}
