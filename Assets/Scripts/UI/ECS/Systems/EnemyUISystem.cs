using BeyondPixels.ECS.Components.Characters.Common;
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
        private ComponentGroup _playerGroup;
        private ComponentGroup _enemyGroup;
        protected override void OnCreateManager()
        {
            _playerGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(TargetComponent), typeof(PlayerComponent)
                }
            });
            _enemyGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(EnemyUIComponent), typeof(SpriteRenderer), typeof(HealthComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.deltaTime;
            Entities.With(_enemyGroup).ForEach(
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
                    var currentFill = (float)currentHealth / maxHealth;

                    enemyUIComponent.HealthImage.fillAmount
                        = math.lerp(enemyUIComponent.HealthImage.fillAmount, currentFill, deltaTime * 10f);
                    var displayedValue = currentHealth < 0 ? 0 : currentHealth;
                    enemyUIComponent.HealthText.text = displayedValue + "/" + maxHealth;

                    //Targetting image
                    enemyUIComponent.TargettingCircle.SetActive(false);
                    Entities.With(_playerGroup).ForEach((Entity playerEntity, ref TargetComponent targetComponent) =>
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
