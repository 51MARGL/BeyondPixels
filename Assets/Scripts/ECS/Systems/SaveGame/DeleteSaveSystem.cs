using BeyondPixels.ECS.Components.SaveGame;

using Unity.Entities;

namespace BeyondPixels.ECS.Systems.SaveGame
{
    public class DeleteSaveSystem : ComponentSystem
    {
        private EntityQuery _deleteGroup;

        protected override void OnCreateManager()
        {
            this._deleteGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(DeleteSaveComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._deleteGroup).ForEach((Entity entity) =>
            {
                SaveGameManager.DeleteSave();
                this.PostUpdateCommands.DestroyEntity(entity);
            });
        }
    }
}
