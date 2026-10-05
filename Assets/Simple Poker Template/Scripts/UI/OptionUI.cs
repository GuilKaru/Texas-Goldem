using SimplePoker.SceneController;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePoker.UI
{
    public class OptionUI : BaseUI
    {
        [Header("UI Buttons")]
        [Space(10)]
        [SerializeField] private Button screenBackButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button menuButton;

        protected override void Awake()
        {
            base.Awake();

            screenBackButton.onClick.AddListener(OnButtonLargeBackClicked);
            backButton.onClick.AddListener(OnButtonBackClicked);
            menuButton.onClick.AddListener(OnButtonMenuClicked);
        }

        private void OnDestroy()
        {
            screenBackButton.onClick.RemoveListener(OnButtonLargeBackClicked);
            backButton.onClick.RemoveListener(OnButtonBackClicked);
            menuButton.onClick.AddListener(OnButtonMenuClicked);
        }


        private void OnButtonLargeBackClicked()
        {
            BackButtonCheck();
        }

        private void OnButtonBackClicked()
        {
            BackButtonCheck();
        }

        private void BackButtonCheck()
        {
            if (UIStateManager.UIManager.GameplayUI != null)
                UIStateManager?.SetMenuState(UIState.GAMEPLAY);
            else
                UIStateManager?.SetMenuState(UIState.MENU);
        }

        private void OnButtonMenuClicked()
        {
            TransitionSceneManager.Instance?.LoadScene("01_Menu");
        }
    }
}