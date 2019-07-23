using BeyondPixels.ECS.Components.Scenes;

using Unity.Entities;

namespace BeyondPixels.ColliderEvents
{
    public class OnExitLevelTriggerToEntity : OnUseTriggerToEntity
    {
        public override void Use()
        {
            base.Use();
            var entityManager = World.Active.EntityManager;
            var entity = this.GetComponent<GameObjectEntity>().Entity;
            entityManager.AddComponentData(entity, new PlayerExitCutsceneComponent());

            this.Canvas.enabled = false;
            this.IsInside = false;
        }
    }
}
