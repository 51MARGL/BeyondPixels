
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
        private ComponentGroup _playerGroup;

        protected override void OnCreateManager()
        {
            this._playerGroup = this.GetComponentGroup(new EntityArchetypeQuery
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
                    var raycastHit = Physics2D.GetRayIntersection(ray, 100f, layerMask);

                    if (raycastHit.transform != null && raycastHit.transform.tag == "LootBag")
                    {
                        var targetPosition = new float2(raycastHit.transform.position.x, raycastHit.transform.position.y);
                        if (!this.InLineOfSigth(positionComponent.CurrentPosition, targetPosition))
                            return;

                        var targetEntity = raycastHit.transform.GetComponent<GameObjectEntity>().Entity;

                        if (!this.EntityManager.HasComponent<OpenLootBagComponent>(targetEntity))
                            this.PostUpdateCommands.AddComponent(targetEntity, new OpenLootBagComponent());

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
