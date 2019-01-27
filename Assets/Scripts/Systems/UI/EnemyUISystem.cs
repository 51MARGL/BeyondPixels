using BeyondPixels.Components.Characters.Common;
using BeyondPixels.Components.Characters.Player;
using BeyondPixels.Components.UI;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.Systems.UI
{
    public class EnemyUISystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public ComponentArray<EnemyUIComponent> EnemyUIComponents;
            public ComponentArray<SpriteRenderer> RendererComponents;
            public ComponentDataArray<HealthComponent> HealthComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private Data _data;

        private struct PlayerData
        {
            public readonly int Length;
            public ComponentArray<PlayerUIComponent> PlayerUIComponents;
            public ComponentDataArray<TargetComponent> TargetComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private PlayerData _playerData;

        protected override void OnUpdate()
        {
            var deltaTime = Time.deltaTime;
            for (int i = 0; i < _data.Length; i++)
            {
                var enemyUIComponent = _data.EnemyUIComponents[i];
                if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main),
                       _data.RendererComponents[i].bounds)) // if object is vissible by main camera
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
                        enemyUIComponent.Canvas.transform.localScale = new Vector3(-Mathf.Abs(currentLocalScale.x), currentLocalScale.y, currentLocalScale.z);
                    else if (enemyUIComponent.transform.localScale.x > 0 && enemyUIComponent.Canvas.transform.localScale.x < 0)
                        enemyUIComponent.Canvas.transform.localScale = new Vector3(Mathf.Abs(currentLocalScale.x), currentLocalScale.y, currentLocalScale.z);

                    // Heatlh 
                    var currentHealth = _data.HealthComponents[i].CurrentValue;
                    var maxHealth = _data.HealthComponents[i].MaxValue;
                    var currentFill = (float)currentHealth / maxHealth;
                    if (enemyUIComponent.HealthImage.fillAmount != currentFill)
                    {
                        enemyUIComponent.HealthImage.fillAmount
                            = Mathf.Lerp(enemyUIComponent.HealthImage.fillAmount, currentFill, deltaTime * 10f);
                        var displayedValue = currentHealth < 0 ? 0 : currentHealth;
                        enemyUIComponent.HealthText.text = displayedValue + " / " + maxHealth;
                    }

                    //Targetting image
                    if (_playerData.Length > 0)
                        for (int j = 0; j < _playerData.Length; j++)
                        {
                            if (_playerData.TargetComponents[j].Target == _data.EntityArray[i])
                                enemyUIComponent.TargettingImage.enabled = true;
                            else
                                enemyUIComponent.TargettingImage.enabled = false;
                        }
                    else
                        enemyUIComponent.TargettingImage.enabled = false;
                }
            }
        }
    }
}
