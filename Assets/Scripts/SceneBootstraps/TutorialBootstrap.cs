using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Game;
using BeyondPixels.ECS.Components.Quest;
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

            this.StartTutorial();
        }

        public void FixedUpdate()
        {
            this.FixedGroup.Update();
        }

        private void StartTutorial()
        {
            var exit = GameObject.Find("LevelExit");
            var entity = exit.GetComponent<GameObjectEntity>().Entity;
            var entityManager = World.Active.EntityManager;
            entityManager.AddComponentData(entity, new LevelExitComponent());
            entityManager.AddComponentData(entity, new PositionComponent
            {
                CurrentPosition = new Unity.Mathematics.float2(exit.transform.position.x, exit.transform.position.y),
                InitialPosition = new Unity.Mathematics.float2(exit.transform.position.x, exit.transform.position.y)
            });

            var cutsceneEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(cutsceneEntity, new PlayerEnterTutorialComponent());

            this.CreateQuests();
        }

        private void CreateQuests()
        {
            var entityManager = World.Active.EntityManager;
            this.CreateInvestigateQuest(entityManager);
            this.CreateLevelUpQuest(entityManager);
            this.CreateSpendQuest(entityManager);
        }

        private void CreateInvestigateQuest(EntityManager entityManager)
        {
            var questObj = GameObject.Instantiate(PrefabManager.Instance.QuestPrefab);
            var questEntity = questObj.GetComponent<GameObjectEntity>().Entity;

            var progressTarget = 1;
            entityManager.AddComponentData(questEntity, new QuestComponent
            {
                CurrentProgress = 0,
                ProgressTarget = progressTarget
            });
            entityManager.AddComponentData(questEntity, new InvestigateQuestComponent());
            entityManager.AddComponentData(questEntity, new LevelComponent
            {
                CurrentLevel = 1
            });
            entityManager.AddComponentData(questEntity, new XPRewardComponent
            {
                XPAmount = 5
            });
            questObj.GetComponent<QuestTextComponent>().QuestText = "Investigate the dungeons";
        }

        private void CreateLevelUpQuest(EntityManager entityManager)
        {
            var questObj = GameObject.Instantiate(PrefabManager.Instance.QuestPrefab);
            var questEntity = questObj.GetComponent<GameObjectEntity>().Entity;

            var progressTarget = 1;
            entityManager.AddComponentData(questEntity, new QuestComponent
            {
                CurrentProgress = 0,
                ProgressTarget = progressTarget
            });
            entityManager.AddComponentData(questEntity, new LevelUpQuestComponent());
            entityManager.AddComponentData(questEntity, new LevelComponent
            {
                CurrentLevel = 1
            });
            entityManager.AddComponentData(questEntity, new XPRewardComponent
            {
                XPAmount = 5
            });
            questObj.GetComponent<QuestTextComponent>().QuestText = "Level up 1 time";
        }

        private void CreateSpendQuest(EntityManager entityManager)
        {
            var questObj = GameObject.Instantiate(PrefabManager.Instance.QuestPrefab);
            var questEntity = questObj.GetComponent<GameObjectEntity>().Entity;

            var progressTarget = 1;
            entityManager.AddComponentData(questEntity, new QuestComponent
            {
                CurrentProgress = 0,
                ProgressTarget = progressTarget
            });
            entityManager.AddComponentData(questEntity, new SpendSkillPointQuestComponent());
            entityManager.AddComponentData(questEntity, new LevelComponent
            {
                CurrentLevel = 1
            });
            entityManager.AddComponentData(questEntity, new XPRewardComponent
            {
                XPAmount = 5
            });
            questObj.GetComponent<QuestTextComponent>().QuestText = "Spend 1 skill point";
        }
    }
}
