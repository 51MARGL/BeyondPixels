using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.UI.ECS.Components;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.UI
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<UIManager>();

                return _instance;
            }
        }

        private GameObject _player;
        private SpellActionButton[] _spellActions;

        public void Initialize(GameObject player)
        {
            this._player = player;
            this._spellActions = player.GetComponent<PlayerUIComponent>().SpellButtonsGroup.ActionButtons;

            var playerEntity = _player.GetComponent<GameObjectEntity>().Entity;
            var spellBook = _player.GetComponent<SpellBookComponent>();
            foreach (var button in this._spellActions)
            {
                button.SpellIcon.sprite = spellBook.Spells[button.SpellIndex].Icon;
                button.SpellCaster = playerEntity;
            }
        }
    }
}
