using System.Linq;

using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.EventSystems;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class TargettingSystem : ComponentSystem
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
                    var layerMask = LayerMask.GetMask("Enemy");
                    var raycastHit = Physics2D.GetRayIntersection(ray, 100f, layerMask);

                    if (raycastHit.transform != null)
                    {
                        var targetPosition = new float2(raycastHit.transform.position.x, raycastHit.transform.position.y);
                        if (!this.InLineOfSigth(positionComponent.CurrentPosition, targetPosition))
                            return;

                        var targetEntity = (raycastHit.transform.GetComponent<GameObjectEntity>()
                                            ?? raycastHit.transform.GetComponentInParent<GameObjectEntity>()).Entity;

                        if (!this.EntityManager.HasComponent<TargetComponent>(entity))
                            this.PostUpdateCommands.AddComponent(entity,
                                new TargetComponent
                                {
                                    Target = targetEntity
                                });
                        else
                            this.PostUpdateCommands.SetComponent(entity,
                                new TargetComponent
                                {
                                    Target = targetEntity
                                });
                    }
                    else if (this.EntityManager.HasComponent<TargetComponent>(entity))
                        this.PostUpdateCommands.RemoveComponent<TargetComponent>(entity);

                }
                else if (inputComponent.SelectTargetButtonPressed == 1)
                {
                    var layerMask = LayerMask.GetMask("Enemy");

                    var width = (Camera.main.ViewportToWorldPoint(new Vector3(1.0F, 0.0F, -Camera.main.transform.position.z))
                                - Camera.main.ViewportToWorldPoint(new Vector3(0.0F, 0.0F, -Camera.main.transform.position.z))).x;
                    var heigth = (Camera.main.ViewportToWorldPoint(new Vector3(0.0F, 1.0F, -Camera.main.transform.position.z))
                                - Camera.main.ViewportToWorldPoint(new Vector3(0.0F, 0.0F, -Camera.main.transform.position.z))).y;

                    var hits = Physics2D.BoxCastAll(Camera.main.transform.position,
                                                    new Vector2(width, heigth), 0,
                                                    Camera.main.transform.forward, -Camera.main.transform.position.z, layerMask);
                    if (hits.Length > 0)
                    {
                        var playerPosition = positionComponent.CurrentPosition;
                        var colliders = hits.Select(h => h.collider)
                                        .OrderBy(c => math.distance(playerPosition,
                                                                    new float2(c.transform.position.x,
                                                                                c.transform.position.y))).ToArray();
                        if (this.EntityManager.HasComponent<TargetComponent>(entity))
                        {
                            var targetComponent = this.EntityManager.GetComponentData<TargetComponent>(entity);
                            var currentTargetPosition = this.EntityManager.GetComponentData<PositionComponent>(targetComponent.Target);

                            foreach (var collider in colliders)
                            {
                                var targetPosition = new float2(collider.transform.position.x, collider.transform.position.y);
                                if (!this.InLineOfSigth(positionComponent.CurrentPosition, targetPosition))
                                    continue;

                                var targetEntity = (collider.GetComponent<GameObjectEntity>()
                                            ?? collider.GetComponentInParent<GameObjectEntity>()).Entity;

                                if (targetEntity != targetComponent.Target)
                                {
                                    this.PostUpdateCommands.SetComponent(entity,
                                        new TargetComponent
                                        {
                                            Target = targetEntity
                                        });

                                    return;
                                }
                            }
                        }
                        else
                        {
                            foreach (var collider in colliders)
                            {
                                var targetPosition = new float2(collider.transform.position.x, collider.transform.position.y);
                                if (!this.InLineOfSigth(positionComponent.CurrentPosition, targetPosition))
                                    continue;

                                var targetEntity = (collider.GetComponent<GameObjectEntity>()
                                               ?? collider.GetComponentInParent<GameObjectEntity>()).Entity;

                                this.PostUpdateCommands.AddComponent(entity,
                                        new TargetComponent
                                        {
                                            Target = targetEntity
                                        });
                                return;
                            }
                        }
                    }
                }
                if (this.EntityManager.HasComponent<TargetComponent>(entity))
                {
                    var targetComponent = this.EntityManager.GetComponentData<TargetComponent>(entity);

                    if (!this.EntityManager.Exists(targetComponent.Target)
                        || !GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main),
                                                    this.EntityManager.GetComponentObject<SpriteRenderer>(targetComponent.Target).bounds))
                    {
                        this.PostUpdateCommands.RemoveComponent<TargetComponent>(entity);
                        return;
                    }

                    var targetPosition = this.EntityManager.GetComponentData<PositionComponent>(targetComponent.Target);

                    if (!this.InLineOfSigth(positionComponent.CurrentPosition, targetPosition.CurrentPosition))
                        this.PostUpdateCommands.RemoveComponent<TargetComponent>(entity);
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
