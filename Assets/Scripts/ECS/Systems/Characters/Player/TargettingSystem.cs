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
        private ComponentGroup _playerGroup;

        protected override void OnCreateManager()
        {
            _playerGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(InputComponent), typeof(PlayerComponent), typeof(PositionComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(_playerGroup).ForEach((Entity entity, ref InputComponent inputComponent, ref PositionComponent positionComponent) =>
            {
                if (inputComponent.MouseButtonClicked == 1
                     && !EventSystem.current.IsPointerOverGameObject())
                {
                    var ray = Camera.main.ScreenPointToRay(inputComponent.MousePosition);
                    var layerMask = LayerMask.GetMask("Clickable");
                    var raycastHit = Physics2D.GetRayIntersection(ray, 100f, layerMask);

                    if (raycastHit.transform != null)
                    {
                        if (raycastHit.transform.CompareTag("Enemy"))
                        {
                            if (!EntityManager.HasComponent<TargetComponent>(entity))
                                PostUpdateCommands.AddComponent(entity,
                                    new TargetComponent
                                    {
                                        Target = raycastHit.transform.GetComponent<GameObjectEntity>().Entity
                                    });
                            else
                                PostUpdateCommands.SetComponent(entity,
                                    new TargetComponent
                                    {
                                        Target = raycastHit.transform.GetComponent<GameObjectEntity>().Entity
                                    });
                        }
                    }
                    else
                        PostUpdateCommands.RemoveComponent<TargetComponent>(entity);

                }
                else if (inputComponent.SelectTargetButtonPressed == 1)
                {
                    var layerMask = LayerMask.GetMask("Clickable");

                    var width = (Camera.main.ViewportToWorldPoint(new Vector3(1.0F, 0.0F, 10)) 
                                - Camera.main.ViewportToWorldPoint(new Vector3(0.0F, 0.0F, -Camera.main.transform.position.z))).x;
                    var heigth = (Camera.main.ViewportToWorldPoint(new Vector3(0.0F, 1.0F, 10)) 
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
                        if (EntityManager.HasComponent<TargetComponent>(entity))
                        {
                            var targetComponent = EntityManager.GetComponentData<TargetComponent>(entity);
                            var currentTargetPosition = EntityManager.GetComponentData<PositionComponent>(targetComponent.Target);

                            foreach (var collider in colliders)
                            {
                                if (collider.GetComponent<GameObjectEntity>().Entity != targetComponent.Target)
                                {
                                    PostUpdateCommands.SetComponent(entity,
                                    new TargetComponent
                                    {
                                        Target = collider.GetComponent<GameObjectEntity>().Entity
                                    });
                                    break;
                                }
                            }
                        }
                        else
                        {
                            PostUpdateCommands.AddComponent(entity,
                                    new TargetComponent
                                    {
                                        Target = colliders[0].GetComponent<GameObjectEntity>().Entity
                                    });
                        }
                    }
                }

                if (EntityManager.HasComponent<TargetComponent>(entity))
                {
                    var targetComponent = EntityManager.GetComponentData<TargetComponent>(entity);

                    if (!EntityManager.Exists(targetComponent.Target)
                        || !GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main),
                                                    EntityManager.GetComponentObject<SpriteRenderer>(targetComponent.Target).bounds))
                        PostUpdateCommands.RemoveComponent<TargetComponent>(entity);
                }
            });
        }
    }
}
