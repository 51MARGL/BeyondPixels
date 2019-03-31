using UnityEngine;

namespace BeyondPixels.Utilities
{
    public class AnimatableSortingOrder : MonoBehaviour
    {
        private SpriteRenderer sr;                
        public float sortingLayer = 0.0f;        

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            int layer = (int)sortingLayer;        
            sr.sortingOrder = layer;            
        }
    }
}
