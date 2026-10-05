/*using SimplePoker.SaveLoad;
using SimplePoker.UnityAds;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePoker.UI
{
    public class AdsUI : BaseUI
    {
        [Header("UI Buttons")]
        [Space(10)]
        [SerializeField] private Button backButton;
        [SerializeField] private Button rewardAdsButton;

        [Header("UI Texts")]
        [Space(10)]
        [SerializeField] private TextMeshProUGUI rewardValueText;
        [SerializeField] private TextMeshProUGUI errorAdText;

        private AdsInitializer adsInitializer;

        protected override void Awake()
        {
            base.Awake();

            backButton.onClick.AddListener(OnButtonBackClicked);
            rewardAdsButton.onClick.AddListener(OnButtonAdsClicked);

        }

        private void Start()
        {
            adsInitializer = AdsInitializer.Instance;
            AdsInitializer.OnAdsInitialized += Handle_OnAdsInitialized;
        }

        private void OnDestroy()
        {
            backButton.onClick.RemoveListener(OnButtonBackClicked);
            rewardAdsButton.onClick.RemoveListener(OnButtonAdsClicked);

            AdsInitializer.OnAdsInitialized -= Handle_OnAdsInitialized;
        }

        private void OnButtonBackClicked()
        {
            UIStateManager?.SetMenuState(UIState.MENU);
        }


        private void OnButtonAdsClicked()
        {

            adsInitializer?.RewardedAd.ShowAd((isShowed) =>
            {
                if (isShowed)
                {
                    //Pay reward
                    RewardedManager rewardedManager = new RewardedManager();
                    int reward = rewardedManager.GetRewardValue();
                    DatabaseManager.AddPlayerChipsAndSave(reward);
                    rewardedManager.ChangeToNextRewardValue();
                    UIStateManager?.SetMenuState(UIState.MENU);
                }
                else
                {
                    errorAdText.gameObject.SetActive(true);
                    errorAdText.SetText("Error to show ad");
                }
            });
        }

        private void Handle_OnAdsInitialized()
        {
            adsInitializer = AdsInitializer.Instance;
            rewardAdsButton.interactable = adsInitializer.IsInitialized;
        }

        public override void Enable()
        {
            base.Enable();

            RewardedManager rewardedManager = new RewardedManager();
            string rewardStringFormat = rewardedManager.GetRewardStringFormat();
            rewardValueText.SetText(rewardStringFormat);
            errorAdText.gameObject.SetActive(false);

            adsInitializer = AdsInitializer.Instance;
            if (adsInitializer != null)
            {
                rewardAdsButton.interactable = adsInitializer.IsInitialized;
                adsInitializer.RewardedAd.LoadAd((isLoaded) =>
                {
                    rewardAdsButton.interactable = isLoaded;
                    if (!isLoaded)
                    {
                        errorAdText.gameObject.SetActive(true);
                        errorAdText.SetText("Ad unavailable, try again");
                    }
                });
            }
            else
            {
                rewardAdsButton.interactable = false;
                errorAdText.gameObject.SetActive(true);
                errorAdText.SetText("Ad unavailable, try again");
            }
        }
    }
}*/