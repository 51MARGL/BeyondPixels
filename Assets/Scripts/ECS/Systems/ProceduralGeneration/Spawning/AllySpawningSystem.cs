using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.SceneBootstraps;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.AI;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Spawning
{
    public class AllySpawningSystem : ComponentSystem
    {
        private ComponentGroup _spawnGroup;

        protected override void OnCreateManager()
        {
            this._spawnGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(SpawnAllyComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            var entities = this._spawnGroup.ToEntityArray(Allocator.TempJob);
            var spawnComponents = this._spawnGroup.ToComponentDataArray<SpawnAllyComponent>(Allocator.TempJob);
            var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());
            var playerEntity = GameObject.FindGameObjectWithTag("Player").GetComponent<GameObjectEntity>().Entity;
            var playerLvlComponent = this.EntityManager.GetComponentData<LevelComponent>(playerEntity);

            for (var i = 0; i < entities.Length; i++)
            {
                this.InstantiateAlly(spawnComponents[i].Position, playerLvlComponent, ref random);
                this.PostUpdateCommands.DestroyEntity(entities[i]);
            }
            entities.Dispose();
            spawnComponents.Dispose();
        }

        private void InstantiateAlly(float2 position, LevelComponent playerLvlComponent, ref Unity.Mathematics.Random random)
        {
            var ally = GameObject.Instantiate(PrefabManager.Instance.Ally,
                new Vector3(position.x, position.y - 0.5f, 0), Quaternion.identity);
            var allyEntity = ally.GetComponent<GameObjectEntity>().Entity;
            var allyInitializeComponent = ally.GetComponent<AllyInitializeComponent>();
            var navMeshAgent = ally.GetComponent<NavMeshAgent>();
            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;

            #region statsInit
            var lvlComponent = new LevelComponent
            {
                CurrentLevel = math.max(0, random.NextInt(playerLvlComponent.CurrentLevel,
                                           playerLvlComponent.CurrentLevel + 3)),
                NextLevelXP = 100,
                SkillPoints = 0
            };

            this.PostUpdateCommands.AddComponent(allyEntity, lvlComponent);
            this.PostUpdateCommands.AddComponent(allyEntity, new AttackStatComponent
            {
                BaseValue = 5,
                CurrentValue = 6 * (lvlComponent.CurrentLevel - 1) / 2
            });
            #endregion

            this.PostUpdateCommands.AddComponent(allyEntity, new CharacterComponent
            {
                CharacterType = CharacterType.Ally
            });
            this.PostUpdateCommands.AddComponent(allyEntity, new MovementComponent
            {
                Direction = float2.zero,
                Speed = allyInitializeComponent.MovementSpeed
            });

            this.PostUpdateCommands.AddComponent(allyEntity, new WeaponComponent
            {
                DamageValue = allyInitializeComponent.WeaponDamage,
                AttackRange = allyInitializeComponent.AttackRange,
                CoolDown = allyInitializeComponent.AttackCoolDown
            });
            this.PostUpdateCommands.AddComponent(allyEntity, new IdleStateComponent
            {
                StartedAt = Time.time
            });
            this.PostUpdateCommands.AddComponent(allyEntity, new PositionComponent
            {
                InitialPosition = new float2(ally.transform.position.x, ally.transform.position.y)
            });
            GameObject.Destroy(allyInitializeComponent);
        }
    }
}
