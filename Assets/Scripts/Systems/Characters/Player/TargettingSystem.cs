using BeyondPixels.Components.Characters.Player;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeyondPixels.Systems.Characters.Player
{
    public class TargettingSystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public ComponentDataArray<InputComponent> InputComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private Data _data;
        [Inject]
        private ComponentDataFromEntity<TargetComponent> _targetComponents;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                var inputComponent = _data.InputComponents[i];

                if (inputComponent.MouseButtonClicked == 1
                    && !EventSystem.current.IsPointerOverGameObject())
                {
                    var ray = Camera.main.ScreenPointToRay(inputComponent.MousePosition);
                    var layerMask = LayerMask.GetMask("Clickable");
                    var raycastHit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);
                    
                    if (raycastHit.transform != null)
                    {
                        if (_targetComponents.Exists(_data.EntityArray[i]))
                            PostUpdateCommands.RemoveComponent<TargetComponent>(_data.EntityArray[i]);

                        if (raycastHit.transform.CompareTag("Enemy"))
                            PostUpdateCommands.AddComponent(_data.EntityArray[i],
                                new TargetComponent
                                {
                                    Target = raycastHit.transform.GetComponent<GameObjectEntity>().Entity
                                });
                    }
                    else if(_targetComponents.Exists(_data.EntityArray[i]))
                        PostUpdateCommands.RemoveComponent<TargetComponent>(_data.EntityArray[i]);
                }
            }
        }
    }
}
