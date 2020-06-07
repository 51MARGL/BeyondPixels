using UnityEngine;

namespace BeyondPixels.ECS.Components.Objects
{
    public class OutlineComponent : MonoBehaviour
    {
        [Range(1, 3)]
        public float OutlineThickness;
        public Color OutlineColor;

        private SpriteRenderer _parentRenderer;
        private SpriteRenderer _currentRenderer;

        public void Start()
        {
            this._currentRenderer = this.GetComponent<SpriteRenderer>();
            this._parentRenderer = this.transform.parent.GetComponent<SpriteRenderer>();
        }

        public void Update()
        {
            this._currentRenderer.sprite = this._parentRenderer.sprite;
            this._currentRenderer.material.SetColor("_OutlineColor", this.OutlineColor);
            this._currentRenderer.material.SetFloat("_Outline", this.OutlineThickness);

        }
    }
}
