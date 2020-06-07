using BeyondPixels.ECS.Components.Scenes;

using Unity.Entities;

namespace BeyondPixels.UI.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class StoryMenuUISystem : ComponentSystem
    {
        private EntityQuery _storyGroup;

        protected override void OnCreate()
        {
            this._storyGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(StoryTellingComponent), typeof(PrintStoryComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._storyGroup).ForEach((Entity entity, StoryTellingComponent storyTellingComponent) =>
            {
                var menu = UIManager.Instance.StoryMenu;
                menu.Show();
                menu.TellStory(storyTellingComponent);

                this.PostUpdateCommands.RemoveComponent<PrintStoryComponent>(entity);
            });
        }
    }
}
