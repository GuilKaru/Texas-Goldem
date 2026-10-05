using System;
using System.Collections.Generic;
using UnityEngine;

namespace TexasHoldem
{
    public class SeatController : MonoBehaviour
    {
        // ── Inspector refs ────────────────────────────────────────────────────

        [Header("Seat Identity")]
        public int seatIndex;

        [Header("References")]
        [SerializeField] private VisualController visualController;

        [Header("Avatar Registry")]
        [SerializeField] private AvatarRegistry avatarRegistry;

        // ── Per-seat C# Actions ───────────────────────────────────────────────
        // UIPlayer (compact table node) and RosterRow both subscribe to these.

        public Action<Sprite, string>   OnInitialized;              // portrait, displayName
        public Action<int>              OnStackChanged;              // new stack
        public Action<int>              OnBetChanged;               // new bet (this street)
        public Action<string>           OnStatusChanged;            // "active"|"folded"|"all_in"|"eliminated"
        public Action<string, int>      OnActionPerformed;          // actionName, amount
        public Action<bool, bool, bool> OnBadgeChanged;             // isDealer, isSB, isBB
        public Action                   OnWinner;
        public Action<int>              OnWonChips;                 // pot amount won
        public Action<int>              OnLostChips;                // pot amount lost
        public Action                   OnNewHand;
        public Action<float>            OnWinChanceChanged;         // 0–1
        public Action                   OnClearActionLabel;         // staged cleanup
        public Action                   OnClearCurrentActionLabel;
        public Action                   OnEliminated;               // seat just hit 0 chips

        // ── Runtime data ──────────────────────────────────────────────────────

        public string DisplayName  { get; private set; } = "Agent";
        public string AvatarSlug   { get; private set; } = "";
        public string LogName      { get; private set; } = "Agent";
        public Sprite AvatarSprite { get; private set; }
        public int    Stack        { get; private set; }
        public int    CurrentBet   { get; private set; }
        public bool   IsDealer     { get; private set; }
        public bool   IsSmallBlind { get; private set; }
        public bool   IsBigBlind   { get; private set; }
        public bool   IsFolded     { get; private set; }
        public bool   IsAllIn      { get; private set; }
        public bool   IsEliminated { get; private set; }
        public float  WinChance    { get; private set; } = -1f;

        // Stored hole cards (used by HighlightWinningCards — cards rendered in RosterRow)
        private int _holeCard0 = -1;
        private int _holeCard1 = -1;

        // Change trackers
        private int    _prevStack      = -1;
        private int    _prevBet        = -1;
        private bool   _prevDealer     = false;
        private bool   _prevSB         = false;
        private bool   _prevBB         = false;
        private string _prevStatus     = null;
        private bool   _wasEliminated  = false;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            PokerTableEvents.OnNewHand             += HandleNewHand;
            PokerTableEvents.OnActionSeatChanged   += HandleActionSeatChanged;
            PokerTableEvents.OnHandResult          += HandleHandResult;
            PokerTableEvents.OnWinningHandRevealed += HandleWinningHandRevealed;
            PokerTableEvents.OnClearActionLabels   += HandleClearActionLabel;
            PokerTableEvents.OnEmotionChanged      += HandleEmotionChanged;
        }

        private void OnDestroy()
        {
            PokerTableEvents.OnNewHand             -= HandleNewHand;
            PokerTableEvents.OnActionSeatChanged   -= HandleActionSeatChanged;
            PokerTableEvents.OnHandResult          -= HandleHandResult;
            PokerTableEvents.OnWinningHandRevealed -= HandleWinningHandRevealed;
            PokerTableEvents.OnClearActionLabels   -= HandleClearActionLabel;
            PokerTableEvents.OnEmotionChanged      -= HandleEmotionChanged;
        }

        // ── Initialize ────────────────────────────────────────────────────────

        public void Initialize(int seat, string avatarSlug = null, string displayName = null)
        {
            seatIndex  = seat;
            AvatarSlug = avatarSlug ?? "";

            DisplayName = !string.IsNullOrWhiteSpace(displayName)
                ? displayName
                : $"Agent {seat}";

            Sprite portrait = avatarRegistry != null
                ? avatarRegistry.GetSprite(AvatarSlug)
                : null;

            string slugDisplayName = avatarRegistry != null
                ? avatarRegistry.GetDisplayName(AvatarSlug)
                : null;

            LogName = !string.IsNullOrWhiteSpace(slugDisplayName)
                ? slugDisplayName
                : DisplayName;

            AvatarSprite = portrait;
            OnInitialized?.Invoke(portrait, DisplayName);

            Debug.Log($"[SeatController] Seat {seatIndex} initialized: displayName={DisplayName}, logName={LogName}, avatar={AvatarSlug}");
        }

        public void SetDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return;
            DisplayName = displayName;
            OnInitialized?.Invoke(AvatarSprite, DisplayName);
            Debug.Log($"[SeatController] Seat {seatIndex} display name updated: {DisplayName}");
        }

        public void SetAvatarRegistry(AvatarRegistry registry)
        {
            avatarRegistry = registry;
        }

        // ── SyncFromDisplay — called by ActionManager ─────────────────────────

        public void SyncFromDisplay(SeatDisplayState state)
        {
            // Stack
            if (state.stack != _prevStack)
            {
                Stack      = state.stack;
                _prevStack = Stack;
                OnStackChanged?.Invoke(Stack);
            }

            // Bet
            if (state.current_bet != _prevBet)
            {
                CurrentBet = state.current_bet;
                _prevBet   = CurrentBet;
                OnBetChanged?.Invoke(CurrentBet);
            }

            // Status
            string status = GetStatus(state);
            if (status != _prevStatus)
            {
                _prevStatus  = status;
                IsFolded     = state.is_folded;
                IsAllIn      = state.is_all_in;
                IsEliminated = state.is_eliminated;
                OnStatusChanged?.Invoke(status);

                // Fire OnEliminated exactly once when this seat first hits eliminated state
                if (state.is_eliminated && !_wasEliminated)
                {
                    _wasEliminated = true;
                    OnEliminated?.Invoke();
                    Debug.Log($"[SeatController] Seat {seatIndex} eliminated.");
                }
            }

            // Badges
            if (state.is_dealer      != _prevDealer ||
                state.is_small_blind != _prevSB     ||
                state.is_big_blind   != _prevBB)
            {
                IsDealer     = state.is_dealer;
                IsSmallBlind = state.is_small_blind;
                IsBigBlind   = state.is_big_blind;
                _prevDealer  = IsDealer;
                _prevSB      = IsSmallBlind;
                _prevBB      = IsBigBlind;
                OnBadgeChanged?.Invoke(IsDealer, IsSmallBlind, IsBigBlind);
            }

            // Hole cards — stored locally so HighlightWinningCards can reference them.
            // Rendering is handled by RosterRow, not here.
            if (state.hole_card_0 >= 0 && state.hole_card_1 >= 0)
            {
                _holeCard0 = state.hole_card_0;
                _holeCard1 = state.hole_card_1;
            }
        }

        // ── NotifyAction — called by ActionManager ────────────────────────────

        public void NotifyAction(string actionName, int amount)
        {
            OnActionPerformed?.Invoke(actionName, amount);

            if (actionName == "Fold")
                visualController?.PlayFold();
        }

        public void ClearCurrentActionLabel()
        {
            OnClearCurrentActionLabel?.Invoke();
        }

        // ── SetWinChance — called by ActionManager after equity calculation ────

        public void SetWinChance(float chance01)
        {
            chance01 = Mathf.Clamp01(chance01);
            if (Mathf.Approximately(WinChance, chance01)) return;
            WinChance = chance01;
            OnWinChanceChanged?.Invoke(WinChance);
        }

        // ── HighlightWinningCards — called via HandleWinningHandRevealed ───────
        // winningCards: the 5 u8 ints forming the best hand.
        // isWinner:     true if this seat won or drew.
        // RosterRow handles the visual highlight — this just fires the correct action.

        public void HighlightWinningCards(List<int> winningCards, bool isWinner)
        {
            // Compact node visual (dim/highlight ring on avatar)
            if (!isWinner)
            {
                visualController?.DimAsNonWinner();
                return;
            }

            visualController?.PlayWinner();
        }

        // ── IsActiveInHand ────────────────────────────────────────────────────

        public bool IsActiveInHand() => !IsFolded && !IsAllIn && !IsEliminated;

        // ── Private helpers ───────────────────────────────────────────────────

        private string GetStatus(SeatDisplayState state)
        {
            if (state.is_eliminated) return "eliminated";
            if (state.is_folded)     return "folded";
            if (state.is_all_in)     return "all_in";
            return "active";
        }

        // ── PokerTableEvents handlers ─────────────────────────────────────────

        private void HandleNewHand(int handNumber)
        {
            _prevStack  = -1;
            _prevBet    = -1;
            _prevStatus = null;
            _prevDealer = false;
            _prevSB     = false;
            _prevBB     = false;
            _holeCard0  = -1;
            _holeCard1  = -1;

            // Do NOT reset _wasEliminated — once eliminated, always eliminated for this match

            IsFolded = false;
            IsAllIn  = false;

            Debug.Log($"[SeatController][Seat {seatIndex}] HandleNewHand hand={handNumber} RESET");

            visualController?.ResetVisuals();
            OnNewHand?.Invoke();
        }

        private void HandleActionSeatChanged(int? activeSeat)
        {
            bool isMyTurn = activeSeat.HasValue && activeSeat.Value == seatIndex;
            visualController?.SetActiveTurn(isMyTurn);
        }

        private void HandleHandResult(List<int> winners, int pot)
        {
            bool iWon = winners != null && winners.Contains(seatIndex);

            if (iWon)
            {
                OnWinner?.Invoke();
                // Split pot: divide evenly among all winners
                int chipsWon = winners.Count > 1 ? pot / winners.Count : pot;
                OnWonChips?.Invoke(chipsWon);
                visualController?.PlayWinner();
            }
            else
            {
                // loserAmount not tracked individually — stack diff visible via SyncFromDisplay
                OnLostChips?.Invoke(0);
            }
        }

        private void HandleWinningHandRevealed(List<int> winnerSeats,
                                               List<List<int>> bestFivePerSeat,
                                               string handName)
        {
            if (winnerSeats == null || bestFivePerSeat == null) return;

            int winnerIndex = winnerSeats.IndexOf(seatIndex);
            bool iWon = winnerIndex >= 0;

            if (iWon)
            {
                // Get this seat's best five if available
                List<int> myBestFive = winnerIndex < bestFivePerSeat.Count
                    ? bestFivePerSeat[winnerIndex]
                    : null;

                HighlightWinningCards(myBestFive, true);
            }
            else
            {
                HighlightWinningCards(null, false);
            }
        }

        private void HandleClearActionLabel()
        {
            OnClearActionLabel?.Invoke();
        }

        private void HandleEmotionChanged(int seat, int emotionId)
        {
            if (seat != seatIndex) return;
            visualController?.SetEmotionById(emotionId);
        }
    }
}