using SimplePoker.Data;
using SimplePoker.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimplePoker.Logic
{
    /// <summary>
    /// Base class representing a player in the poker game, 
    /// handling actions, bets, and interactions with the game environment.
    /// </summary>
    public class PlayerBase : MonoBehaviour
    {
        public Action<PokerAction> OnActionChoosed;
        public Action<Card, List<Card>> OnCardAddedToHand;
        public Action<PokerHand> OnHandUpdated;
        public Action<int, int> OnChipsAmountChanged;
        public Action OnWinnerChoosen;
        public Action<int> OnBetMaked;
        public Action<PokerChip> OnPokerChipSeted;
        public Action OnPlayerActived;

        public string Name;

        [field: SerializeField] public int PositionIndex { get; private set; }
        [field: SerializeField] public bool IsPlayer { get; private set; }
        [field: SerializeField] public List<Card> Cards { get; private set; }
        [field: SerializeField] public PokerHand PokerHand { get; private set; }
        [field: SerializeField] public PokerChip PokerChip { get; private set; }
        [field: SerializeField] public int Chips { get; private set; }
        [field: SerializeField] public int CurrentBet { get; private set; }

        [field: SerializeField] public bool IsCheck { get; private set; }
        [field: SerializeField] public bool IsCall { get; private set; }
        [field: SerializeField] public bool IsRaise { get; private set; }
        [field: SerializeField] public bool IsAllIn { get; private set; }
        [field: SerializeField] public bool IsFold { get; private set; }

        public bool ContinueAllInNextTurn { get; set; }
        public bool ContinueFoldNextTurn { get; private set; }

        public PokerAction PokerAction { get; private set; }

        protected bool IsMyTurn;

        protected PokerGameAssetData asset;
        protected GameManager gameManager;

        /// <summary>
        /// Initializes the player's setup and subscribes to relevant game events.
        /// </summary>
        public virtual void Awake()
        {
            Cards = new List<Card>();
            PokerEventManager.OnPlayerTurnChanged += PokerEventManager_HandlePlayerTurn;
            PokerEventManager.OnTurnChanged += PokerEventManager_HandleTurnChanged;
            PokerEventManager.OnNewRoundStarted += PokerEventManager_HandleNewRoundStarted;
        }

        /// <summary>
        /// Disables the player's subscriptions to game events.
        /// </summary>
        private void OnDisable()
        {
            PokerEventManager.OnPlayerTurnChanged -= PokerEventManager_HandlePlayerTurn;
            PokerEventManager.OnTurnChanged -= PokerEventManager_HandleTurnChanged;
            PokerEventManager.OnNewRoundStarted -= PokerEventManager_HandleNewRoundStarted;

        }

        /// <summary>
        /// Initializes the player's data and references at the start of the game.
        /// </summary>
        private void Start()
        {
            gameManager = GameManager.Instance;
            asset = PokerGameAsset.Instance.PokerGameAssetData;
        }

        /// <summary>
        /// Handles the event when a new round starts, resetting player actions and flags.
        /// </summary>
        private void PokerEventManager_HandleNewRoundStarted()
        {
            ResetActions();
            ContinueAllInNextTurn = false;
            ContinueFoldNextTurn = false;
        }

        /// <summary>
        /// Handles the event when it's the player's turn, updating the current bet and executing the turn action.
        /// </summary>
        private void PokerEventManager_HandlePlayerTurn(PlayerBase player, GameManager.TurnPoker turnPoker)
        {
            if (player == this)
            {
                CurrentBet = gameManager.PokerPotBet.GetPlayerBetByTurn(this, gameManager.Turn);
                DoTurn(turnPoker);
            }
        }

        /// <summary>
        /// Handles the event when the turn changes, resetting the current bet and player actions.
        /// </summary>
        private void PokerEventManager_HandleTurnChanged()
        {
            CurrentBet = 0;
            ResetActions();
        }

        /// <summary>
        /// Clears the player's hand, removing the cards.
        /// </summary>
        public void ClearHand()
        {
            Cards.Clear();
        }

        /// <summary>
        /// Adds a card to the player's hand and invokes corresponding events.
        /// </summary>
        public void AddCardToHand(Card newCard)
        {
            Cards.Add(newCard);
            OnCardAddedToHand?.Invoke(newCard, Cards);
        }

        /// <summary>
        /// Updates the player's poker hand based on the cards on the table and invokes the corresponding event.
        /// </summary>
        /// <param name="cardsInTable">The cards currently on the table.</param>
        public void UpdatePokerHand(List<Card> cardsInTable)
        {
            List<Card> totalCards = Cards;
            totalCards = totalCards.Concat(cardsInTable).ToList();
            PokerHand = new CheckHand().GetBestHand(totalCards);

            OnHandUpdated?.Invoke(PokerHand);
        }

        /// <summary>
        /// Sets the player's chip count.
        /// </summary>
        public void SetChips(int chips)
        {
            OnChipsAmountChanged?.Invoke(Chips, chips);
            Chips = chips;
        }

        /// <summary>
        /// Adds chips to the player's chip count.
        /// </summary>
        public void AddChips(int chips)
        {
            int newValueChips = Chips + chips;
            OnChipsAmountChanged?.Invoke(Chips, newValueChips);
            Chips += chips;
        }

        /// <summary>
        /// Sets the player as the dealer.
        /// </summary>
        public void SetDealer()
        {
            PokerChip = PokerChip.DEALER;
            OnPokerChipSeted?.Invoke(PokerChip);
        }

        /// <summary>
        /// Sets the player as the small blind and makes a initial bet.
        /// </summary>
        public void SetSmallBlind(int chips)
        {
            PokerChip = PokerChip.SMALL_BLIND;
            MakeBet(chips);
            OnPokerChipSeted?.Invoke(PokerChip);
        }

        /// <summary>
        /// Sets the player as the big blind and makes a initial bet.
        /// </summary>
        public void SetBigBlind(int chips)
        {
            PokerChip = PokerChip.BIG_BLIND;
            MakeBet(chips);
            OnPokerChipSeted?.Invoke(PokerChip);
        }

        /// <summary>
        /// Processes the player's bet, updating their chip count and triggering additional game events.
        /// </summary>
        /// <param name="chips">The number of chips to bet.</param>
        private void MakeBet(int chips)
        {
            CurrentBet = gameManager.PokerPotBet.GetPlayerBetByTurn(this, gameManager.Turn);
            int futureChips = CurrentBet + Chips - chips;
            if (futureChips <= 0 && !IsAllIn)
            {
                ResetActions();

                PokerAction = PokerAction.ALL_IN;
                IsAllIn = true;
                ContinueAllInNextTurn = true;
                OnActionChoosed?.Invoke(PokerAction);

                int totalChips = CurrentBet + Chips;

                gameManager.AddChipsToPot(this, totalChips);
                CurrentBet = gameManager.PokerPotBet.GetPlayerBetByTurn(this, gameManager.Turn);
                gameManager.PlayerAllIn(this);
                OnBetMaked?.Invoke(totalChips);
                SetChips(0);
            }
            else if (futureChips > 0)
            {
                gameManager.AddChipsToPot(this, chips);
                SetChips(futureChips);
                OnBetMaked?.Invoke(chips);
            }
        }

        /// <summary>
        /// Resets the player's setup by clearing their hand and resetting the current bet to zero.
        /// </summary>
        public virtual void ResetSetup()
        {
            Cards.Clear();
            CurrentBet = 0;
        }

        /// <summary>
        /// Resets all player actions to their default state.
        /// </summary>
        public void ResetActions()
        {
            PokerAction = PokerAction.NOTHING;
            IsFold = false;
            IsCall = false;
            IsAllIn = false;
            IsCheck = false;
            IsRaise = false;
        }

        /// <summary>
        /// Player executes the fold action.
        /// </summary>
        public void DoFold()
        {
            ResetActions();
            IsFold = true;
            ContinueFoldNextTurn = true;
            PokerAction = PokerAction.FOLD;
            gameManager.PlayerFold(this);
            OnActionChoosed?.Invoke(PokerAction);
        }

        /// <summary>
        /// Player executes the check or call action.
        /// </summary>
        public void DoCheckOrCall()
        {
            ResetActions();
            CurrentBet = gameManager.PokerPotBet.GetPlayerBetByTurn(this, gameManager.Turn);
            if (!gameManager.HasBetInTable() && CurrentBet >= gameManager.PokerPotBet.GetHighBetByTurn(gameManager.Turn))
            {
                IsCheck = true;
                PokerAction = PokerAction.CHECK;
                OnActionChoosed?.Invoke(PokerAction);
            }
            else
            {
                IsCall = true;
                PokerAction = PokerAction.CALL;
                OnActionChoosed?.Invoke(PokerAction);
                MakeBet(gameManager.PokerPotBet.GetHighBetByTurn(gameManager.Turn));
            }
        }

        /// <summary>
        /// Player executes the check action.
        /// </summary>
        public void DoCheck()
        {
            IsCheck = true;
            PokerAction = PokerAction.CHECK;
            OnActionChoosed?.Invoke(PokerAction);
        }

        /// <summary>
        /// Player execute the call action.
        /// </summary>
        public void DoCall()
        {
            IsCall = true;
            PokerAction = PokerAction.CALL;
            OnActionChoosed?.Invoke(PokerAction);
        }

        /// <summary>
        /// Executes the raise action for the player with a default raise amount.
        /// </summary>
        public void DoRaise()
        {
            ResetActions();
            MakeBet(gameManager.BetBase + 50);

            IsRaise = true;
            PokerAction = PokerAction.RAISE;
            gameManager.SetPlayerEndTurn(this);
            OnActionChoosed?.Invoke(PokerAction);
        }

        /// <summary>
        /// Executes the raise action for the player with a specified raise amount.
        /// </summary>
        public void DoRaise(int raiseChips)
        {
            ResetActions();
            MakeBet(raiseChips);

            IsRaise = true;
            PokerAction = PokerAction.RAISE;
            gameManager.SetPlayerEndTurn(this);
            OnActionChoosed?.Invoke(PokerAction);
        }

        /// <summary>
        /// Player executes the all-in action.
        /// </summary>
        public void DoAllIn()
        {
            ResetActions();
            CurrentBet = gameManager.PokerPotBet.GetPlayerBetByTurn(this, gameManager.Turn);

            int totalChips = CurrentBet + Chips;
            MakeBet(CurrentBet + Chips);
            PokerAction = PokerAction.ALL_IN;
            IsAllIn = true;
            ContinueAllInNextTurn = true;
            gameManager.SetPlayerEndTurn(this);
            gameManager.PlayerAllIn(this);
            OnActionChoosed?.Invoke(PokerAction);
        }

        /// <summary>
        /// Checks if the player has performed any action.
        /// </summary>
        /// <returns>True if the player has performed an action, otherwise false.</returns>
        public bool DidAnAction()
        {
            return IsFold || IsCall || IsCheck || IsRaise || IsAllIn;
        }

        /// <summary>
        /// Initializes the player's position index in the game.
        /// </summary>
        /// <param name="positionIndex">The position index of the player to enable on the table.</param>
        public virtual void Setup(int positionIndex)
        {
            PositionIndex = positionIndex;
        }

        /// <summary>
        /// Initiates the player's turn in the game, 
        /// handling scenarios where the player is required to perform specific actions.
        /// </summary>
        /// <param name="turn">The current turn in the game.</param>
        public virtual void DoTurn(GameManager.TurnPoker turn)
        {
            IsMyTurn = true;
            if (ContinueAllInNextTurn)
            {
                IsAllIn = true;
                IsMyTurn = false;
                StartCoroutine(CallNextPlayerCoroutine(0.01f));
            }
            else if (ContinueFoldNextTurn)
            {
                IsFold = true;
                IsMyTurn = false;
                StartCoroutine(CallNextPlayerCoroutine(0.01f));
            }
        }

        /// <summary>
        /// Coroutine to delay the next player's turn.
        /// </summary>
        /// <param name="duration">The duration to wait before advancing to the next player.</param>
        protected IEnumerator CallNextPlayerCoroutine(float duration)
        {
            IsMyTurn = false;
            yield return new WaitForSeconds(duration);
            gameManager.NextPlayer();
        }
    }
}