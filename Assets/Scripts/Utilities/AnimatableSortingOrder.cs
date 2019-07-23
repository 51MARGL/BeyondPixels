using UnityEngine;

namespace BeyondPixels.Utilities
{
    public class AnimatableSortingOrder : MonoBehaviour
    {
        private SpriteRenderer sr;
        public float sortingLayer = 0.0f;

        private void Start()
        {
            this.sr = this.GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            var layer = (int)this.sortingLayer;
            this.sr.sortingOrder = layer;
        }
    }
}
