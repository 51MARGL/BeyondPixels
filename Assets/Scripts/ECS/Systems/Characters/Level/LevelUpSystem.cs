using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;

using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Characters.Level
{
    public class LevelUpSystem : ComponentSystem
    {
        private EntityQuery _characterGroup;

        protected override void OnCreate()
        {
            this._characterGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
{
                    typeof(LevelComponent), typeof(LevelUpComponent),
                    typeof(PositionComponent), typeof(CharacterComponent)
}
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._characterGroup).ForEach((Entity entity, ref LevelComponent levelComponent, ref PositionComponent positionComponent, ref CharacterComponent characterComponent) =>
           {
               levelComponent.CurrentLevel++;
               levelComponent.NextLevelXP *= 2;
               levelComponent.SkillPoints++;

               this.PostUpdateCommands.RemoveComponent<LevelUpComponent>(entity);
           });
        }
    }
}
