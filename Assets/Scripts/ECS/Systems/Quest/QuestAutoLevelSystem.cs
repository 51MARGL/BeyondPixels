using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Quest;
using BeyondPixels.ECS.Systems.Items;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Quest
{
    [UpdateBefore(typeof(PickUpSystem))]
    public class QuestAutoLevelSystem : ComponentSystem
    {
        private EntityQuery _questGroup;
        private EntityQuery _playerGroup;

        protected override void OnCreate()
        {
            this._questGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(QuestComponent), typeof(LevelComponent)
                },
                None = new ComponentType[]
                {
                    typeof(LevelAdjustedComponent)
                }
            });
            this._playerGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PlayerComponent), typeof(LevelComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            this.Entities.With(this._playerGroup).ForEach((Entity entity, ref LevelComponent levelComponent) =>
            {
                var lvl = levelComponent.CurrentLevel;

                this.Entities.With(this._questGroup).ForEach((Entity questEntity,
                    ref LevelComponent qlevelComponent) =>
                {
                    qlevelComponent.CurrentLevel = lvl;
                    this.PostUpdateCommands.AddComponent(questEntity, new LevelAdjustedComponent());
                });
            });
        }
    }
}