//using BeyondPixels.ECS.Components.Characters.Player;
//using Unity.Entities;
//using Unity.Mathematics;
//using UnityEngine;
//using UnityEngine.EventSystems;

//namespace BeyondPixels.ECS.Systems.Characters.Player
//{
//    public class TargettingSystem : ComponentSystem
//    {
//        private struct Data
//        {
//            public readonly int Length;
//            public ComponentDataArray<InputComponent> InputComponents;
//            public ExcludeComponent<TargetComponent> TargetComponents;
//            public EntityArray EntityArray;
//        }
//        [Inject]
//        private Data _data;
//        private struct AddedData
//        {
//            public readonly int Length;
//            public ComponentDataArray<InputComponent> InputComponents;
//            public ComponentDataArray<TargetComponent> TargetComponents;
//            public EntityArray EntityArray;
//        }
//        [Inject]
//        private AddedData _addedData;

//        protected override void OnUpdate()
//        {
//            for (int i = 0; i < _data.Length; i++)
//            {
//                var inputComponent = _data.InputComponents[i];

//                if (inputComponent.MouseButtonClicked == 1
//                    && !EventSystem.current.IsPointerOverGameObject())
//                {
//                    var ray = Camera.main.ScreenPointToRay(inputComponent.MousePosition);
//                    var layerMask = LayerMask.GetMask("Clickable");
//                    var raycastHit = Physics2D.GetRayIntersection(ray, 100f, layerMask);

//                    if (raycastHit.transform != null)
//                    {
//                        if (raycastHit.transform.CompareTag("Enemy"))
//                            PostUpdateCommands.AddComponent(_data.EntityArray[i],
//                                new TargetComponent
//                                {
//                                    Target = raycastHit.transform.GetComponent<GameObjectEntity>().Entity
//                                });
//                    }
//                }
//            }

//            for (int i = 0; i < _addedData.Length; i++)
//            {
//                var inputComponent = _addedData.InputComponents[i];

//                if (inputComponent.MouseButtonClicked == 1
//                    && !EventSystem.current.IsPointerOverGameObject())
//                {
//                    var ray = Camera.main.ScreenPointToRay(inputComponent.MousePosition);
//                    var layerMask = LayerMask.GetMask("Clickable");
//                    var raycastHit = Physics2D.GetRayIntersection(ray, 100f, layerMask);

//                    if (raycastHit.transform != null)
//                    {
//                        if (raycastHit.transform.CompareTag("Enemy"))
//                            PostUpdateCommands.SetComponent(_addedData.EntityArray[i],
//                                new TargetComponent
//                                {
//                                    Target = raycastHit.transform.GetComponent<GameObjectEntity>().Entity
//                                });
//                    }
//                    else
//                        PostUpdateCommands.RemoveComponent<TargetComponent>(_addedData.EntityArray[i]);
//                }
//                else if (!EntityManager.Exists(_addedData.TargetComponents[i].Target))
//                    PostUpdateCommands.RemoveComponent<TargetComponent>(_addedData.EntityArray[i]);
//            }
//        }
//    }
//}
