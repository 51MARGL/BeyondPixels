using BeyondPixels.UI.ECS.Components;
using UnityEngine;

namespace BeyondPixels.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        public Canvas Canvas;
        public GameUIComponent GameUIComponent;
        public PlayerInfoMenuUIComponent PlayerInfoMenuUIComponent;
        public LootBagMenuUIComponent LootBagMenuUIComponent;
        public ToolTipUIComponent ToolTip;
        public GameOverMenuUIComponent GameOverMenu;
        public MainMenuUIComponent MainMenu;
        public OptionsMenuUIComponent OptionsMenu;
        public StoryMenuUIComponent StoryMenu;
        public QuestMenuUIComponent QuestMenu;

        [SerializeField]
        protected YesNoDialogUIComponent YesNoDialog;
        public YesNoDialogUIComponent CurrentYesNoDialog { get; private set; }

        public void Awake()
        {
            UIManager.Instance = this;
        }

        public void CloseAllMenus()
        {
            this.PlayerInfoMenuUIComponent.Hide();
            this.LootBagMenuUIComponent.Hide();
            this.QuestMenu.Hide();
            this.OptionsMenu.Hide();
            this.MainMenu.Hide();

            this.HideTooltip();
        }

        public void ShowTooltip(Vector3 position, string header, string content, string buttonsDescription, bool below = false)
        {
            this.ToolTip.gameObject.SetActive(true);
            this.ToolTip.transform.position = position;
            this.ToolTip.Header.text = header;
            this.ToolTip.Content.text = content;
            this.ToolTip.ButtonsDescription.text = buttonsDescription;

            if (below)
                this.ToolTip.GetComponent<RectTransform>().pivot = new Vector2(-0.025f, 1);
            else
                this.ToolTip.GetComponent<RectTransform>().pivot = new Vector2(-0.025f, 0);

        }

        public void HideTooltip()
        {
            this.ToolTip.gameObject.SetActive(false);
        }

        public YesNoDialogUIComponent CreateYesNoDialog()
        {
            var dialogObj = GameObject.Instantiate(this.YesNoDialog.gameObject, this.Canvas.transform);
            this.CurrentYesNoDialog = dialogObj.GetComponent<YesNoDialogUIComponent>();
            return this.CurrentYesNoDialog;
        }
    }
}
