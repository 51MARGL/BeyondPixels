using BeyondPixels.UI.ECS.Components;
using UnityEngine;

namespace BeyondPixels.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        public GameUIComponent GameUIComponent;
        public PlayerInfoMenuUIComponent PlayerInfoMenuUIComponent;
        public LootBagMenuUIComponent LootBagMenuUIComponent;
        public ToolTipUIComponent ToolTip;
        public GameObject GameOverMenu;

        public void Start()
        {
            UIManager.Instance = this;
        }

        public void CloseAllMenus()
        {
            this.PlayerInfoMenuUIComponent.GetComponent<CanvasGroup>().alpha = 0;
            this.PlayerInfoMenuUIComponent.GetComponent<CanvasGroup>().blocksRaycasts = false;

            this.LootBagMenuUIComponent.GetComponent<CanvasGroup>().alpha = 0;
            this.LootBagMenuUIComponent.GetComponent<CanvasGroup>().blocksRaycasts = false;

            this.GameOverMenu.GetComponent<CanvasGroup>().alpha = 0;
            this.GameOverMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;

            this.ToolTip.GetComponent<CanvasGroup>().alpha = 0;
        }

        public void ShowTooltip(Vector3 position, string header, string content, string buttonsDescription, bool below = false)
        {
            this.ToolTip.transform.position = position;
            this.ToolTip.Header.text = header;
            this.ToolTip.Content.text = content;
            this.ToolTip.ButtonsDescription.text = buttonsDescription;
            this.ToolTip.GetComponent<CanvasGroup>().alpha = 1;

            if (below)
                this.ToolTip.GetComponent<RectTransform>().pivot = new Vector2(-0.025f, 1);
            else
                this.ToolTip.GetComponent<RectTransform>().pivot = new Vector2(-0.025f, 0);

        }

        public void HideTooltip()
        {
            this.ToolTip.GetComponent<CanvasGroup>().alpha = 0;
        }
    }
}
