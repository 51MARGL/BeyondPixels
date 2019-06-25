using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Game;
using BeyondPixels.ECS.Components.Scenes;
using BeyondPixels.UI;
using BeyondPixels.Utilities;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.SceneBootstraps
{
    public class TutorialBootstrap : MonoBehaviour
    {
        private FixedUpdateSystemGroup FixedGroup;

        private void Start()
        {
            this.FixedGroup = World.Active.GetOrCreateSystem<FixedUpdateSystemGroup>();

            var settings = SettingsManager.Instance;
            UIManager.Instance.MainMenu.InGameMenu = true;

            var exit = GameObject.Find("LevelExit");
            var entity = exit.GetComponent<GameObjectEntity>().Entity;
            var entityManager = World.Active.EntityManager;
            entityManager.AddComponentData(entity, new LevelExitComponent());
            entityManager.AddComponentData(entity, new PositionComponent
            {
                CurrentPosition = new Unity.Mathematics.float2(exit.transform.position.x, exit.transform.position.y),
                InitialPosition = new Unity.Mathematics.float2(exit.transform.position.x, exit.transform.position.y)
            });
        }

        public void FixedUpdate()
        {
            this.FixedGroup.Update();
        }
    }
}
