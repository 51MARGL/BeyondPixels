using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.ECS.Components.Spells;
using Cinemachine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class InitializeSystem : ComponentSystem
    {
        private EntityQuery _group;

        protected override void OnCreate()
        {
            this._group = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(PlayerInitializeComponent), typeof(Transform), typeof(StatsInitializeComponent),
                }
            });
        }
        protected override void OnUpdate()
        {
            this.Entities.With(this._group).ForEach((Entity entity,
                PlayerInitializeComponent playerInitializeComponent,
                StatsInitializeComponent statsInitializeComponent,
                Transform transform) =>
            {
                this.PostUpdateCommands.AddComponent(entity, new PlayerComponent());
                this.PostUpdateCommands.AddComponent(entity, new InputComponent());

                this.PostUpdateCommands.AddComponent(entity, new CharacterComponent
                {
                    CharacterType = CharacterType.Player
                });
                this.PostUpdateCommands.AddComponent(entity, new HealthComponent
                {
                    MaxValue = playerInitializeComponent.BaseHealth,
                    CurrentValue = playerInitializeComponent.BaseHealth,
                    BaseValue = playerInitializeComponent.BaseHealth
                });
                this.PostUpdateCommands.AddComponent(entity, new MovementComponent
                {
                    Direction = float2.zero,
                    Speed = playerInitializeComponent.MovementSpeed
                });
                this.PostUpdateCommands.AddComponent(entity, new WeaponComponent
                {
                    DamageValue = playerInitializeComponent.WeaponDamage
                });
                this.PostUpdateCommands.AddComponent(entity, new PositionComponent
                {
                    InitialPosition = new float2(transform.position.x, transform.position.y)
                });
                GameObject.Destroy(playerInitializeComponent);
                this.PostUpdateCommands.RemoveComponent<PlayerInitializeComponent>(entity);

                #region statsInit
                this.PostUpdateCommands.AddComponent(entity, statsInitializeComponent.LevelComponent);
                this.PostUpdateCommands.AddComponent(entity, statsInitializeComponent.HealthStatComponent);

                this.PostUpdateCommands.AddComponent(entity, statsInitializeComponent.AttackStatComponent);
                this.PostUpdateCommands.AddComponent(entity, statsInitializeComponent.DefenceStatComponent);
                this.PostUpdateCommands.AddComponent(entity, statsInitializeComponent.MagicStatComponent);
                this.PostUpdateCommands.AddComponent(entity, new XPComponent());
                this.PostUpdateCommands.AddComponent(entity, new AdjustStatsComponent());
                GameObject.Destroy(statsInitializeComponent);
                #endregion

                #region spellInit
                for (var i = 0; i < 3; i++)
                {
                    var spellEntity = this.PostUpdateCommands.CreateEntity();
                    this.PostUpdateCommands.AddComponent(spellEntity, new ActiveSpellComponent
                    {
                        Owner = entity,
                        ActionIndex = i + 1,
                        SpellIndex = i
                    });
                }
                #endregion

                #region camera
                var camera = GameObject.Find("PlayerVCamera").GetComponent<CinemachineVirtualCamera>();
                camera.Follow = transform;
                camera.LookAt = transform;
                #endregion

                var loadGameEntity = this.PostUpdateCommands.CreateEntity();
                this.PostUpdateCommands.AddComponent(loadGameEntity, new LoadGameComponent());
            });
        }
    }
}
