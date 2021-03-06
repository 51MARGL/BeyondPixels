﻿using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using BeyondPixels.SceneBootstraps;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Spawning
{
    public class AllySpawningSystem : ComponentSystem
    {
        private EntityQuery _spawnGroup;

        protected override void OnCreate()
        {
            this._spawnGroup = this.GetEntityQuery(new EntityQueryDesc
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
            var random = new Unity.Mathematics.Random((uint)System.Guid.NewGuid().GetHashCode());
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
        }
    }
}
