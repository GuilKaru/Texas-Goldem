using SimplePoker.UnityAds;
using SimplePoker.Helper;
using SimplePoker.SaveLoad;
using SimplePoker.SceneController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SimplePoker.Data;
using SimplePoker.ScriptableObjects;
using SimplePoker.Attribute;

namespace SimplePoker.UI
{

    /// <summary>
    /// Represents a button for selecting a city level.
    /// </summary>
    public class LevelCityButton : MonoBehaviour
    {
        [field: SerializeField, ReadOnly] public LevelData LevelData { get; private set; }

        [SerializeField, ReadOnly] private Button levelCityButton;

        [SerializeField] private Image cityImage;
        [SerializeField] private TextMeshProUGUI cityNameText;
        [SerializeField] private TextMeshProUGUI amountPlayersText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private TextMeshProUGUI ticketText;

        private void Awake()
        {
            levelCityButton = GetComponent<Button>();
            levelCityButton.onClick.AddListener(OnButtonLevelCityClicked);
        }

        private void OnDestroy()
        {
            levelCityButton.onClick.RemoveListener(OnButtonLevelCityClicked);
        }

        private void OnButtonLevelCityClicked()
        {
            int chips = DatabaseManager.LoadPlayerChips();
            if(chips >= LevelData.Ticket)
            {
                /*AdsInitializer adsInitializer = AdsInitializer.Instance;
                adsInitializer?.InterstitialAd.ShowAd();*/

                PokerLevelData.Instance?.SetPokerMatchData(LevelData);
                DatabaseManager.SubPlayerChipsAndSave(LevelData.Ticket);
                TransitionSceneManager.Instance?.LoadScene("02_Gameplay");
            }
            else
            {
                UIStateManager.Instance?.SetMenuState(UIState.ADS);
            }
        }

        /// <summary>
        /// Sets the data of the level for the city button.
        /// </summary>
        /// <param name="levelData">The level data to set.</param>
        public void SetLevelData(LevelData levelData)
        {
            LevelData = levelData;

            cityImage.sprite = LevelData.CityPhoto;
            cityNameText.SetText(LevelData.CityName);
            amountPlayersText.SetText($"{LevelData.AmountOfPlayers} Players");

            string rewardValueAbbreviate = Helpers.AbbreviateMoneyUSD(LevelData.Reward);
            rewardText.SetText($"Reward    {rewardValueAbbreviate}");

            string ticketValueAbbreviate = Helpers.AbbreviateMoneyUSD(LevelData.Ticket);
            ticketText.SetText($"Ticket    {ticketValueAbbreviate}");
        }
    }
}