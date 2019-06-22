using BeyondPixels.ECS.Components.Game;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Game
{
    public class QuitGameSystem : ComponentSystem
    {
        private ComponentGroup _quitGroup;

        protected override void OnCreateManager()
        {
            this._quitGroup = this.GetComponentGroup(new EntityArchetypeQuery
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
