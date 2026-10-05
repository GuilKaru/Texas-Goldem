using SimplePoker.Data;
using SimplePoker.Helper;
using SimplePoker.SceneController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePoker.UI
{
    public class WinnerUI : BaseUI
    {
        [SerializeField] private Button menuButton;
        [SerializeField] private TextMeshProUGUI moneyRewardText;

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
            string moneyReward = Helpers.AbbreviateMoneyUSD(PokerLevelData.Instance.LevelData.Reward);
            moneyRewardText.SetText(moneyReward);
        }
    }
}