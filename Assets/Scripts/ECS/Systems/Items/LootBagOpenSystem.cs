
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Items;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.EventSystems;

namespace BeyondPixels.ECS.Systems.Items
{
    public class LootBagOpenSystem : ComponentSystem
    {
        private EntityQuery _playerGroup;

        protected override void OnCreate()
        {
            this._playerGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(InputComponent), typeof(PlayerComponent), typeof(PositionComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._playerGroup).ForEach((Entity entity, ref InputComponent inputComponent, ref PositionComponent positionComponent) =>
            {
                if (Camera.main == null)
                    return;

                if (inputComponent.MouseButtonClicked == 1
                     && !EventSystem.current.IsPointerOverGameObject())
                {
                    var ray = Camera.main.ScreenPointToRay(inputComponent.MousePosition);
                    var layerMask = LayerMask.GetMask("World");
                    var hits = Physics2D.GetRayIntersectionAll(ray, 100f, layerMask);

                    foreach (var hit in hits)
                    {
                        if (hit.transform != null && hit.transform.tag == "LootBag")
                        {
                            var targetPosition = new float2(hit.transform.position.x, hit.transform.position.y);
                            if (!this.InLineOfSigth(positionComponent.CurrentPosition, targetPosition))
                                return;

                            var targetEntity = hit.transform.GetComponent<GameObjectEntity>().Entity;

                            if (!this.EntityManager.HasComponent<OpenLootBagComponent>(targetEntity))
                                this.PostUpdateCommands.AddComponent(targetEntity, new OpenLootBagComponent());
                            return;
                        }
                    }
                }
            });
        }

        private bool InLineOfSigth(float2 position, float2 targetPosition)
        {
            var wallLayer = LayerMask.GetMask("World");
            var distance = math.distance(position, targetPosition);
            var hits = Physics2D.RaycastAll(position,
                                            targetPosition - position,
                                            distance, wallLayer);

            foreach (var hit in hits)
                if (hit.transform.tag == "Wall")
                    return false;

            return true;
        }
    }
}
