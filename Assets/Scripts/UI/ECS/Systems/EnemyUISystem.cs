﻿using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.UI.ECS.Components;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.UI.ECS.Systems.UI
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class EnemyUISystem : ComponentSystem
    {
        private EntityQuery _playerGroup;
        private EntityQuery _enemyGroup;
        protected override void OnCreate()
        {
            this._playerGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(TargetComponent), typeof(PlayerComponent)
                }
            });
            this._enemyGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(EnemyUIComponent), typeof(SpriteRenderer), typeof(HealthComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.deltaTime;
            this.Entities.With(this._enemyGroup).ForEach(
                (Entity entity,
                 ref HealthComponent healthComponent,
                 SpriteRenderer spriteRenderer,
                 EnemyUIComponent enemyUIComponent) =>
            {
                if (Camera.main == null)
                    return;

                // if object is vissible by main camera
                if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main),
                                                    spriteRenderer.bounds))
                {
                    if (!enemyUIComponent.Canvas.enabled)
                        enemyUIComponent.Canvas.enabled = true;
                }
                else
                {
                    if (enemyUIComponent.Canvas.enabled)
                        enemyUIComponent.Canvas.enabled = false;
                }

                if (enemyUIComponent.Canvas.enabled)
                {
                    //Canvas scale
                    var currentLocalScale = enemyUIComponent.Canvas.transform.localScale;
                    if (enemyUIComponent.transform.localScale.x < 0 && enemyUIComponent.Canvas.transform.localScale.x > 0)
                        enemyUIComponent.Canvas.transform.localScale = new Vector3(-math.abs(currentLocalScale.x), currentLocalScale.y, currentLocalScale.z);
                    else if (enemyUIComponent.transform.localScale.x > 0 && enemyUIComponent.Canvas.transform.localScale.x < 0)
                        enemyUIComponent.Canvas.transform.localScale = new Vector3(math.abs(currentLocalScale.x), currentLocalScale.y, currentLocalScale.z);

                    // Heatlh 
                    var currentHealth = healthComponent.CurrentValue;
                    var maxHealth = healthComponent.MaxValue;
                    var currentFill = currentHealth / maxHealth;

                    enemyUIComponent.HealthImage.fillAmount
                        = math.lerp(enemyUIComponent.HealthImage.fillAmount, currentFill, deltaTime * 10f);
                    var displayedValue = currentHealth < 0 ? 0 : currentHealth;
                    enemyUIComponent.HealthText.text = displayedValue.ToString("F1") + "/" + maxHealth.ToString("F1");

                    //Targetting image
                    enemyUIComponent.TargettingCircle.SetActive(false);
                    this.Entities.With(this._playerGroup).ForEach((Entity playerEntity, ref TargetComponent targetComponent) =>
                    {
                        if (targetComponent.Target == entity)
                            enemyUIComponent.TargettingCircle.SetActive(true);
                        else
                            enemyUIComponent.TargettingCircle.SetActive(false);
                    });
                }
            });
        }
    }
}
