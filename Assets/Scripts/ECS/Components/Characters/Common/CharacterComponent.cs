using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    public struct CharacterComponent : IComponentData
    {
        public CharacterType CharacterType;
    }

    public enum CharacterType
    {
        Player = 1,
        Enemy = 2,
        Ally = 3
    }
}
