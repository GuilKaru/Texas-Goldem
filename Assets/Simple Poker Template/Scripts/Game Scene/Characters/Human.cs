using SimplePoker.Data;
using SimplePoker.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePoker.Logic
{
    /// <summary>
    /// Class representing a human player in the poker game, 
    /// managing UI elements and interactions specific to user.
    /// </summary>
    public class Human : PlayerBase
    {
        [field: SerializeField] public PlayerData PlayerData { get; private set; }

        [Header("Human UI")]
        [SerializeField] private Button foldButton;
        [SerializeField] private Button checkOrCallButton;
        [SerializeField] private Button raiseButton;

        [SerializeField] private Slider raiseSlider;
        [SerializeField] private TextMeshProUGUI raiseChipsSliderText;

        [SerializeField] private GameObject screenCanvas;

        /// <summary>
        /// Initializes the player's data and UI elements upon awakening, 
        /// including event listeners and button functionalities.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            PlayerData = PokerLevelData.Instance?.PlayerData;

            raiseSlider.onValueChanged.AddListener(delegate { UpdateInfoRaiseSlider(); });

            foldButton.onClick.AddListener(delegate { DoFold(); });
            foldButton.onClick.AddListener(delegate { StartCoroutine(CallNextPlayerCoroutine(asset.Time_CallNextPlayer)); });
            foldButton.onClick.AddListener(delegate { DisableInterface(); });

            checkOrCallButton.onClick.AddListener(delegate { DoCheckOrCall(); });
            checkOrCallButton.onClick.AddListener(delegate { StartCoroutine(CallNextPlayerCoroutine(asset.Time_CallNextPlayer)); });
            checkOrCallButton.onClick.AddListener(delegate { DisableInterface(); });

            raiseButton.onClick.AddListener(delegate { DoRaise((int)raiseSlider.value + CurrentBet); });
            raiseButton.onClick.AddListener(delegate { StartCoroutine(CallNextPlayerCoroutine(asset.Time_CallNextPlayer)); });
            raiseButton.onClick.AddListener(delegate { DisableInterface(); });

            screenCanvas.SetActive(false);
        }

        /// <summary>
        /// Updates the information displayed on the raise slider.
        /// </summary>
        private void UpdateInfoRaiseSlider()
        {
            // Round the slider value to the nearest multiple of the forced bet increase value
            int newValue = Mathf.RoundToInt(raiseSlider.value / gameManager.IncreaseForcedBet) * gameManager.IncreaseForcedBet;

            // Ensure that the new value does not exceed the maximum value of the slider
            if ((int)raiseSlider.maxValue - newValue < gameManager.IncreaseForcedBet)
                newValue = (int)raiseSlider.maxValue;

            // Set the slider value to the new value if it has changed
            if (raiseSlider.value != newValue)
                raiseSlider.value = newValue;

            // Calculate the visual value displayed on the slider, considering the current bet amount
            int visualValue = (int)raiseSlider.value - CurrentBet;
            if (CurrentBet != gameManager.BetBase && CurrentBet != 0)
            {
                raiseChipsSliderText.SetText($"{visualValue.ToString("C0")}");
            }
            else
            {
                raiseChipsSliderText.SetText($"{raiseSlider.value.ToString("C0")}");
            }

            if (RaiseIsAllIn())
                raiseChipsSliderText.SetText($"ALL IN");
        }

        /// <summary>
        /// Sets the player's data and invokes the corresponding event when the player is activated.
        /// </summary>
        /// <param name="playerData">The data of the player.</param>
        public void SetData(PlayerData playerData)
        {
            PlayerData = playerData;
            OnPlayerActived?.Invoke();
        }

        /// <summary>
        /// Disables the interface associated with the human player.
        /// </summary>
        public void DisableInterface()
        {
            screenCanvas.SetActive(false);
        }

        /// <summary>
        /// Determines if raise value would result in going all-in.
        /// </summary>
        /// <returns>True if raising would result in going all-in, otherwise false.</returns>
        private bool RaiseIsAllIn()
        {
            return raiseSlider.minValue == raiseSlider.maxValue || raiseSlider.value >= Chips;
        }


        /// <summary>
        /// Initiates the player's turn in the game, handling UI elements based on game conditions.
        /// </summary>
        /// <param name="turn">The current turn in the game.</param>
        public override void DoTurn(GameManager.TurnPoker turn)
        {
            base.DoTurn(turn);
            if (!gameManager.HasBetInTable() && CurrentBet >= gameManager.PokerPotBet.GetHighBetByTurn(gameManager.Turn))
            {
                checkOrCallButton.GetComponentInChildren<TextMeshProUGUI>().SetText("CHECK");
            }
            else
            {
                checkOrCallButton.GetComponentInChildren<TextMeshProUGUI>().SetText("CALL");
            }

            // Deactivate the screen canvas if the player is scheduled to fold or go all-in next turn
            if (ContinueAllInNextTurn || ContinueFoldNextTurn)
                screenCanvas.SetActive(false);
            else
            {
                // Set the minimum value of the raise slider
                if (Chips >= gameManager.BetBase)
                {
                    raiseSlider.minValue = gameManager.BetBase + gameManager.IncreaseForcedBet;
                    if (raiseSlider.minValue >= Chips)
                        raiseSlider.minValue = Chips;
                }
                else
                    raiseSlider.minValue = Chips;

                // Set the maximum value of the raise slider 
                raiseSlider.maxValue = Chips;

                // Set the initial value of the raise slider
                raiseSlider.value = raiseSlider.minValue;
                screenCanvas.SetActive(true);
            }
        }
    }
}