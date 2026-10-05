using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePoker.UI
{
    public class GameplayUI : BaseUI
    {
        [Header("UI Buttons")]
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button timeScaleButton;

        [Header("UI Texts")]
        [SerializeField] private TextMeshProUGUI timeScaleText;

        private float[] timeScaleValues = { 1f, 1.5f, 2f };
        private int timeScaleIndex;

        protected override void Awake()
        {
            base.Awake();

            optionsButton.onClick.AddListener(OnButtonOptionsClicked);
            timeScaleButton.onClick.AddListener(OnButtonTimeScaleClicked);
            timeScaleIndex = 0;
        }

        private void OnDestroy()
        {
            optionsButton.onClick.RemoveListener(OnButtonOptionsClicked);
            timeScaleButton.onClick.RemoveListener(OnButtonTimeScaleClicked);
        }

        public override void Enable()
        {
            base.Enable();
            UpdateTimeScale();
        }

        private void OnButtonOptionsClicked()
        {
            UIStateManager?.SetMenuState(UIState.OPTIONS);
        }

        private void OnButtonTimeScaleClicked()
        {
            timeScaleIndex = (timeScaleIndex + 1) % timeScaleValues.Length;
            UpdateTimeScale();
        }

        private void UpdateTimeScale()
        {
            Time.timeScale = timeScaleValues[timeScaleIndex];
            timeScaleText.SetText($"{Time.timeScale}x");
        }
    }
}