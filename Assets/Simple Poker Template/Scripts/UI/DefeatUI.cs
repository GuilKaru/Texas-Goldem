using SimplePoker.Data;
using SimplePoker.Helper;
using SimplePoker.SceneController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePoker.UI
{
    public class DefeatUI : BaseUI
    {
        [SerializeField] private Button menuButton;
        [SerializeField] private TextMeshProUGUI moneyLoseText;

        protected override void Awake()
        {
            base.Awake();
            menuButton.onClick.AddListener(OnButtonMenuClicked);
        }

        private void OnDestroy()
        {
            menuButton.onClick.AddListener(OnButtonMenuClicked);
        }

        private void OnButtonMenuClicked()
        {
            TransitionSceneManager.Instance?.LoadScene("01_Menu");
        }

        public override void Enable()
        {
            base.Enable();
            string moneyLose = Helpers.AbbreviateMoneyUSD(PokerLevelData.Instance.LevelData.Ticket);
            moneyLoseText.SetText(moneyLose);
        }
    }
}