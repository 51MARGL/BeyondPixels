using UnityEngine;

namespace BeyondPixels.ECS.Components.Objects
{
    [RequireComponent(typeof(SpriteMask), typeof(SpriteRenderer))]
    public class MaskOutlineComponent : MonoBehaviour
    {
        private SpriteRenderer _currentRenderer;
        private SpriteMask _spriteMask;

        public void Start()
        {
            this._currentRenderer = this.GetComponent<SpriteRenderer>();
            this._spriteMask = this.GetComponent<SpriteMask>();
        }

        public void Update()
        {
            this._spriteMask.sprite = this._currentRenderer.sprite;
        }
    }
}
