using BeyondPixels.Components.Characters.Spells;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BeyondPixels.Systems.Spells
{
    public class DestroySystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;           
            public ComponentDataArray<SpellComponent> SpellComponents;
            public ComponentDataArray<DestroyComponent> DestroyComponents;
            public ComponentArray<Transform> TransformComponents;      
        }
        [Inject]
        private Data _data;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _data.Length; i++)            
                GameObject.Destroy(_data.TransformComponents[i].gameObject);            
        }      
    }
}
