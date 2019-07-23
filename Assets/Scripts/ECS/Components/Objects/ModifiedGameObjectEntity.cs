using Unity.Entities;

namespace BeyondPixels.ECS.Components.Objects
{
    public class ModifiedGameObjectEntity : GameObjectEntity
    {
        public void Awake()
        {
            base.OnEnable();
        }

        public void OnDestroy()
        {
            base.OnDisable();
        }

        protected override void OnDisable()
        {

        }

        protected override void OnEnable()
        {

        }
    }
}
