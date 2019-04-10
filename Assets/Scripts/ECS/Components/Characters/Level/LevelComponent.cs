using System;
using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Level
{
    [Serializable]
    public struct LevelComponent : IComponentData
    {
        public int CurrentLevel;
        public int NextLevelXP;
    }
}
