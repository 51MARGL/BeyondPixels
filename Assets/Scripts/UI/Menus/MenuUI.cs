using UnityEngine;

namespace BeyondPixels.UI.Menus
{
    public abstract class MenuUI : MonoBehaviour
    {
        public bool IsVisible => this.gameObject.activeSelf;

        public virtual void Show()
        {
            this.gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            this.gameObject.SetActive(false);
            UIManager.Instance.HideTooltip();
        }
    }
}
