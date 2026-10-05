using SimplePoker.Helper;
using SimplePoker.SaveLoad;
using SimplePoker.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePoker.UI
{
    public class MenuUI : BaseUI
    {
        [Header("Level Settings")]
        [Space(10)]
        [SerializeField] private LevelCityButton levelCityButtonPrefab;
        [SerializeField] private LevelData[] levels;
        [SerializeField] private Transform levelContainer;

        [Header("UI Images")]
        [Space(10)]
        [SerializeField] private TextMeshProUGUI chipsText;

        [Header("UI Images")]
        [Space(10)]
        [SerializeField] private Image portraitImage;

        [Header("UI Buttons")]
        [Space(10)]
        [SerializeField] private Button profileButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button coinsButton;


        protected override void Awake()
        {
            base.Awake();

            levels = Resources.LoadAll<LevelData>("Scriptable Objects/LevelData/");

            InstantiateAndSetupLevels();

            profileButton.onClick.AddListener(OnButtonProfileClicked);
            optionsButton.onClick.AddListener(OnButtonOptionsClicked);
            coinsButton.onClick.AddListener(OnButtonCoinsClicked);

            ProfileUI.OnPortraitChanged += Handle_OnPortraitChanged;
        }

        private void OnDestroy()
        {
            profileButton.onClick.RemoveListener(OnButtonProfileClicked);
            optionsButton.onClick.RemoveListener(OnButtonOptionsClicked);
            coinsButton.onClick.RemoveListener(OnButtonCoinsClicked);

            ProfileUI.OnPortraitChanged -= Handle_OnPortraitChanged;
        }

        private void InstantiateAndSetupLevels()
        {
            for (int i = 0; i < levels.Length; i++)
            {
                LevelCityButton levelCityButton = Instantiate(levelCityButtonPrefab, levelContainer);
                levelCityButton.SetLevelData(levels[i]);
            }
        }

        private void OnButtonProfileClicked()
        {
            UIStateManager?.SetMenuState(UIState.PROFILE);
        }

        private void OnButtonOptionsClicked()
        {
            UIStateManager?.SetMenuState(UIState.OPTIONS);
        }

        private void OnButtonCoinsClicked()
        {
            UIStateManager?.SetMenuState(UIState.ADS);
        }

        private void Handle_OnPortraitChanged(Sprite portrait)
        {
            UpdatePortraitImage(portrait);
        }

        public override void Enable()
        {
            base.Enable();
            Sprite portrait = UIStateManager?.UIManager.ProfileUI.GetPortrait();
            UpdatePortraitImage(portrait);
            UpdateChips();
        }

        private void UpdatePortraitImage(Sprite portrait)
        {
            portraitImage.sprite = portrait;
        }

        private void UpdateChips()
        {
            int chips = DatabaseManager.LoadPlayerChips();
            chipsText.SetText(Helpers.MoneyUSDFormat(chips));
        }
    }
}