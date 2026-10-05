using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TexasHoldem
{
    public class ActionManager : MonoBehaviour
    {
        public static ActionManager Instance { get; private set; }

        // ── Timing (tunable in Inspector) ─────────────────────────────────────

        [Header("Playback Timing (seconds)")]
        [SerializeField] private float delayBetweenActions         = 0.8f;
        [SerializeField] private float delayAfterCommunity         = 0.5f;
        [SerializeField] private float delayShowdownReveal         = 1.0f;
        [SerializeField] private float delayAfterWinner            = 0.5f;

        [SerializeField] private bool  enableThinkingStep          = true;
        [SerializeField] private float thinkingDuration            = 0.7f;
        [SerializeField] private float actionResultVisibleDuration = 0.8f;

        [Header("Blind Intro Timing (seconds)")]
        [Tooltip("How long to show the clean-slate state (full stacks, pot=0) before blinds post.")]
        [SerializeField] private float delayInitialState           = 2.0f;
        [Tooltip("Pause after each blind posts.")]
        [SerializeField] private float delayPerBlind               = 1.0f;

        [Header("Staged Cleanup Timing (seconds)")]
        [Tooltip("After Won/Lost shows: pause before clearing seat action boxes.")]
        [SerializeField] private float stagedDelayClearActions     = 2.0f;
        [Tooltip("After action boxes clear: pause before clearing board + pot.")]
        [SerializeField] private float stagedDelayClearBoard       = 1.5f;
        [Tooltip("After board clears: pause before hiding hole cards.")]
        [SerializeField] private float stagedDelayClearHoleCards   = 1.0f;
        [Tooltip("After hole cards hide: pause before clearing the action log.")]
        [SerializeField] private float stagedDelayClearLog         = 0.8f;
        [Tooltip("After log clears: brief pause before HandDealt processes.")]
        [SerializeField] private float stagedDelayBeforeNextHand   = 1.0f;

        [Header("Gap-Close Timing")]
        [Tooltip("Pause after gap-close animation before next hand begins.")]
        [SerializeField] private float delayAfterGapClose          = 0.5f;
        
        [SerializeField] private float delayCollectBetsToPot = 0.85f;

        // ── Private state ─────────────────────────────────────────────────────

        private Queue<string>     _eventQueue     = new Queue<string>();
        private bool              _isProcessing   = false;
        private MatchDisplayState _displayState   = null;
        private static bool       _didBootLoading = false;

        private List<int> _lastCommunityCards  = new List<int>();
        private int       _lastActionType      = -1;

        // Per-seat stack baselines — List<int> indexed by seat, sized at match start
        private List<int> _handStartStacks   = new List<int>();
        private List<int> _streetStartStacks = new List<int>();
        private int       _lastProcessedStreet = 0;

        // Currently eliminated seat indices (accumulated across hands)
        private List<int> _eliminatedSeats  = new List<int>();

        private const int EngineActionAdvanceStreet = 1;

        // ── Public ────────────────────────────────────────────────────────────

        public string CurrentMatchId => _displayState?.match_id;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Entry point — called by WebGLReceiver ─────────────────────────────

        public void EnqueueEvent(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return;
            _eventQueue.Enqueue(json);
            if (!_isProcessing)
            {
                _isProcessing = true;
                _ = ProcessQueueAsync();
            }
        }

        // ── Market data — called directly by WebGLReceiver ────────────────────

        public void ReceiveMarketData(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return;

            MarketDataPayload payload = Deserialize<MarketDataPayload>(json);
            if (payload?.seats == null) return;

            foreach (MarketSeatData entry in payload.seats)
            {
                RosterRow row = RosterPanel.Instance?.GetRow(entry.seat_index);
                row?.SetMarketData(entry.price, entry.slippage);
            }
        }

        public void StopMarketData()
        {
            // Intentionally does NOT clear rows — market data persists until the
            // match resets. Clearing here caused "--" mid-queue because
            // StopMarketData fires before the event queue finishes processing.
            Debug.Log("[ActionManager] StopMarketData — polling stopped, rows keep last values.");
        }

        // ── Queue processor ───────────────────────────────────────────────────

        private async Task ProcessQueueAsync()
        {
            while (_eventQueue.Count > 0)
            {
                string json = _eventQueue.Dequeue();
                await ProcessEventAsync(json);
            }
            _isProcessing = false;
        }

        private async Task ProcessEventAsync(string json)
        {
            string kind = PeekKind(json);
            if (string.IsNullOrEmpty(kind))
            {
                Debug.LogWarning("[ActionManager] Could not read 'kind' from event JSON.");
                return;
            }

            Debug.Log($"[ActionManager] Processing event: {kind}");

            switch (kind)
            {
                case "MatchCreated":      await HandleMatchCreated(json);      break;
                case "HandDealt":         await HandleHandDealt(json);         break;
                case "EngineActionTaken": await HandleEngineActionTaken(json); break;
                case "ActionTaken":       await HandleActionTaken(json);       break;
                case "BanterEvent":       await HandleBanterEvent(json);       break;
                case "HandCompleted":     await HandleHandCompleted(json);     break;
                case "MatchCompleted":    await HandleMatchCompleted(json);    break;
                default:
                    Debug.LogWarning($"[ActionManager] Unknown event kind: '{kind}'");
                    break;
            }
        }

        // ── MatchCreated ──────────────────────────────────────────────────────

        private async Task HandleMatchCreated(string json)
        {
            MatchCreatedEvent evt = Deserialize<MatchCreatedEvent>(json);
            if (evt == null) return;

            Debug.Log($"[ActionManager] Match created: {evt.match_id}");

            int seatCount = GameManager.Instance ? GameManager.Instance.SeatCount : 2;

            _displayState = new MatchDisplayState
            {
                match_id     = evt.match_id,
                match_winner = -1,
                seats        = new SeatDisplayState[seatCount]
            };

            for (int i = 0; i < seatCount; i++)
                _displayState.seats[i] = new SeatDisplayState { seat_index = i };

            InitStackBaselines(seatCount);

            if (!_didBootLoading)
            {
                _didBootLoading = true;
                if (MainMenuScreen.Instance)
                    await MainMenuScreen.Instance.PlayAsync();
                else
                    await WaitSeconds(1.6f);
            }
            else
            {
                await WaitSeconds(0.1f);
            }
        }

        // ── HandDealt ─────────────────────────────────────────────────────────

        private async Task HandleHandDealt(string json)
        {
            HandDealtEvent evt = Deserialize<HandDealtEvent>(json);
            if (evt == null) return;

            GameOverScreen.Instance?.OnNewHand(CurrentMatchId);

            int seatCount = GameManager.Instance ? GameManager.Instance.SeatCount : 2;

            if (_displayState == null)
            {
                _displayState = new MatchDisplayState
                {
                    match_winner = -1,
                    seats        = new SeatDisplayState[seatCount]
                };
                for (int i = 0; i < seatCount; i++)
                    _displayState.seats[i] = new SeatDisplayState { seat_index = i };
                InitStackBaselines(seatCount);
            }

            // ── Pre-blind stacks ──────────────────────────────────────────────
            // HandDealt stacks are post-blind. Add each seat's blind back.
            List<int> preBlindStacks = new List<int>(seatCount);
            for (int i = 0; i < seatCount; i++)
            {
                int postBlindStack = i < evt.stacks.Count ? evt.stacks[i] : 0;
                int blindPaid = 0;
                if (i == evt.small_blind_seat) blindPaid = evt.small_blind_amount;
                else if (i == evt.big_blind_seat) blindPaid = evt.big_blind_amount;
                preBlindStacks.Add(postBlindStack + blindPaid);
            }

            for (int i = 0; i < seatCount; i++)
            {
                if (i < _handStartStacks.Count)   _handStartStacks[i]   = preBlindStacks[i];
                if (i < _streetStartStacks.Count)  _streetStartStacks[i] = preBlindStacks[i];
            }

            _lastProcessedStreet = 0;
            _lastActionType      = -1;

            // ── Display state setup ───────────────────────────────────────────
            _displayState.hand_index         = evt.hand_index;
            _displayState.button             = evt.button;
            _displayState.small_blind_seat   = evt.small_blind_seat;
            _displayState.big_blind_seat     = evt.big_blind_seat;
            _displayState.small_blind_amount = evt.small_blind_amount;
            _displayState.big_blind_amount   = evt.big_blind_amount;
            _displayState.street             = 0;
            _displayState.community_cards.Clear();
            _lastCommunityCards.Clear();

            PokerTableEvents.OnDealerButtonChanged?.Invoke(evt.button);
            
            for (int i = 0; i < seatCount; i++)
            {
                SeatDisplayState s = _displayState.seats[i];
                s.is_dealer      = i == evt.button;
                s.is_small_blind = i == evt.small_blind_seat;
                s.is_big_blind   = i == evt.big_blind_seat;
                s.is_folded      = false;
                s.is_all_in      = false;
                // Hole cards stored now — NOT synced until Phase 3
                s.hole_card_0 = (evt.hole_cards != null && i < evt.hole_cards.Count && evt.hole_cards[i].Count > 0)
                    ? evt.hole_cards[i][0] : -1;
                s.hole_card_1 = (evt.hole_cards != null && i < evt.hole_cards.Count && evt.hole_cards[i].Count > 1)
                    ? evt.hole_cards[i][1] : -1;
            }

            // Fire OnNewHand — resets all seats, log, community card visuals
            PokerTableEvents.OnNewHand?.Invoke(evt.hand_index);
            await WaitSeconds(0.3f);

            // Clean Slate ─────────────────────────────────────────
            // Pre-blind stacks, pot = 0, no bets, no hole cards.

            _displayState.pot = 0;
            for (int i = 0; i < seatCount; i++)
            {
                _displayState.seats[i].stack       = preBlindStacks[i];
                _displayState.seats[i].current_bet = 0;
            }

            SyncAllSeatsExceptHoleCards();
            UIManager.Instance?.SyncFromDisplayState(_displayState);
            Debug.Log($"[ActionManager] Hand {evt.hand_index} — Phase 0: Clean slate.");

            // Positions and UI are now fully reset — safe to fade nodes back in.
            // This fires before delayInitialState so nodes appear as the clean
            // slate is shown, not after blinds have already posted.
            PokerTableEvents.OnFadeInTableNodes?.Invoke();

            await WaitSeconds(delayInitialState);

            // Small Blind posts ───────────────────────────────────

            int sbSeat = evt.small_blind_seat;
            _displayState.pot = evt.small_blind_amount;
            _displayState.seats[sbSeat].stack       = sbSeat < evt.stacks.Count ? evt.stacks[sbSeat] : 0;
            _displayState.seats[sbSeat].current_bet = evt.small_blind_amount;

            SyncAllSeatsExceptHoleCards();
            
            // Highlight the small blind as the current active seat.
            // This is an automatic payment, not a decision — never "thinking".
            PokerTableEvents.OnSeatThinkingChanged?.Invoke(null);
            PokerTableEvents.OnActionSeatChanged?.Invoke(sbSeat);

            UIManager.Instance?.SyncFromDisplayState(_displayState);
            UIManager.Instance?.LogBlindPayment(sbSeat, "small", evt.small_blind_amount);
            UIManager.Instance?.ShowActionLabel(sbSeat, "blind", evt.small_blind_amount);
            PokerTableEvents.OnChipsToPot?.Invoke(sbSeat, evt.small_blind_amount);
            Debug.Log($"[ActionManager] Hand {evt.hand_index} — Phase 1: SB seat {sbSeat} posts {evt.small_blind_amount}.");

            await WaitSeconds(delayPerBlind);
            UIManager.Instance?.ClearSeatActionLabel(sbSeat);
            
            // Clear the blind highlight before moving to BB.
            PokerTableEvents.OnActionSeatChanged?.Invoke(null);

            // Big Blind posts ─────────────────────────────────────

            int bbSeat = evt.big_blind_seat;
            _displayState.pot = evt.pot;
            _displayState.seats[bbSeat].stack       = bbSeat < evt.stacks.Count ? evt.stacks[bbSeat] : 0;
            _displayState.seats[bbSeat].current_bet = evt.big_blind_amount;

            SyncAllSeatsExceptHoleCards();
            
            // Highlight the big blind as the current active seat.
            // This is an automatic payment, not a decision — never "thinking".
            PokerTableEvents.OnSeatThinkingChanged?.Invoke(null);
            PokerTableEvents.OnActionSeatChanged?.Invoke(bbSeat);

            
            UIManager.Instance?.SyncFromDisplayState(_displayState);
            UIManager.Instance?.LogBlindPayment(bbSeat, "big", evt.big_blind_amount);
            UIManager.Instance?.ShowActionLabel(bbSeat, "blind", evt.big_blind_amount);
            PokerTableEvents.OnChipsToPot?.Invoke(bbSeat, evt.big_blind_amount);
            Debug.Log($"[ActionManager] Hand {evt.hand_index} — Phase 2: BB seat {bbSeat} posts {evt.big_blind_amount}.");

            await WaitSeconds(delayPerBlind);
            UIManager.Instance?.ClearSeatActionLabel(bbSeat);
            
            // Clear the blind highlight before dealing cards.
            PokerTableEvents.OnSeatThinkingChanged?.Invoke(null);
            PokerTableEvents.OnActionSeatChanged?.Invoke(null);

            // Cards deal ──────────────────────────────────────────
            // Full SyncAllSeats includes hole cards → triggers RosterRow card display.

            SyncAllSeats();
            RefreshSeatWinChances();
            Debug.Log($"[ActionManager] Hand {evt.hand_index} — Phase 3: Cards dealing.");

            await WaitForAllSeatsDealt(seatCount);
            Debug.Log($"[ActionManager] Hand {evt.hand_index} — Deal complete. Ready for actions.");
        }

        // ── EngineActionTaken ─────────────────────────────────────────────────

        private async Task HandleEngineActionTaken(string json)
        {
            EngineActionTakenEvent evt = Deserialize<EngineActionTakenEvent>(json);
            if (evt == null || _displayState == null) return;

            if (evt.action_type != EngineActionAdvanceStreet)
            {
                Debug.LogWarning($"[ActionManager] Ignoring EngineActionTaken action_type={evt.action_type}");
                return;
            }

            int seatCount = _displayState.seats.Length;

            _displayState.pot    = evt.pot;
            _displayState.street = evt.street;
            _displayState.community_cards = evt.community_cards != null
                ? new List<int>(evt.community_cards)
                : new List<int>();

            for (int i = 0; i < seatCount; i++)
            {
                if (i < evt.stacks.Count)
                    _displayState.seats[i].stack = evt.stacks[i];
                _displayState.seats[i].current_bet = 0;
                if (i < _streetStartStacks.Count)
                    _streetStartStacks[i] = _displayState.seats[i].stack;
            }

            _lastProcessedStreet = evt.street;
            _lastActionType      = -1;

            for (int i = 0; i < seatCount; i++)
                UIManager.Instance?.ClearSeatActionLabel(i);

            // Clear active seat — nobody thinks while community cards are shown
            PokerTableEvents.OnSeatThinkingChanged?.Invoke(null);
            PokerTableEvents.OnActionSeatChanged?.Invoke(null);

            await CollectBetsToPotAsync();

            PokerTableEvents.OnStreetChanged?.Invoke(evt.street);
            PokerTableEvents.OnCommunityCardsChanged?.Invoke(new List<int>(_displayState.community_cards));

            SyncAllSeats();
            UIManager.Instance?.SyncFromDisplayState(_displayState);
            _lastCommunityCards = new List<int>(_displayState.community_cards);

            RefreshSeatWinChances();
            await WaitSeconds(delayAfterCommunity);
        }

        // ── ActionTaken ───────────────────────────────────────────────────────

        private async Task HandleActionTaken(string json)
        {
            ActionTakenEvent evt = Deserialize<ActionTakenEvent>(json);
            if (evt == null || _displayState == null) return;

            int seatCount = _displayState.seats.Length;

            // Snapshot previous stacks for chip animation calculations
            List<int> prevStacks = new List<int>(seatCount);
            for (int i = 0; i < seatCount; i++)
                prevStacks.Add(_displayState.seats[i].stack);

            _displayState.pot = evt.pot;

            // Update all stacks from event
            for (int i = 0; i < seatCount && i < evt.stacks.Count; i++)
                _displayState.seats[i].stack = evt.stacks[i];

            // Street advanced within ActionTaken?
            bool streetAdvanced =
                evt.community_cards != null &&
                evt.community_cards.Count > _lastCommunityCards.Count;

            if (streetAdvanced)
            {
                for (int i = 0; i < seatCount; i++)
                {
                    _displayState.seats[i].current_bet = 0;
                    if (i < _streetStartStacks.Count)
                        _streetStartStacks[i] = prevStacks[i];
                }

                _lastProcessedStreet = evt.street;
                _lastActionType      = -1;

                for (int i = 0; i < seatCount; i++)
                    UIManager.Instance?.ClearSeatActionLabel(i);

                _displayState.community_cards = new List<int>(evt.community_cards);
                _displayState.street          = evt.street;

                // Clear active seat during street change — nobody is "thinking"
                // while the flop/turn/river cards are being shown.
                PokerTableEvents.OnSeatThinkingChanged?.Invoke(null);
                PokerTableEvents.OnActionSeatChanged?.Invoke(null);

                await CollectBetsToPotAsync();

                PokerTableEvents.OnStreetChanged?.Invoke(evt.street);
                PokerTableEvents.OnCommunityCardsChanged?.Invoke(new List<int>(evt.community_cards));

                SyncAllSeats();
                UIManager.Instance?.SyncFromDisplayState(_displayState);
                _lastCommunityCards = new List<int>(evt.community_cards);

                RefreshSeatWinChances();
                await WaitSeconds(delayAfterCommunity);
            }

            // ── Apply acting seat state ───────────────────────────────────────

            SeatDisplayState actingSeat = _displayState.seats[evt.seat];
            string actionName = ActionNames.FromInt(evt.action_type);

            switch (evt.action_type)
            {
                case 0: // Fold
                    actingSeat.is_folded = true;
                    break;

                case 1: // Check
                    break;

                case 2: // Call
                case 3: // Bet
                case 4: // Raise
                    int streetStart  = evt.seat < _streetStartStacks.Count ? _streetStartStacks[evt.seat] : 0;
                    int currentStack = evt.seat < evt.stacks.Count ? evt.stacks[evt.seat] : 0;
                    actingSeat.current_bet = Mathf.Max(0, streetStart - currentStack);
                    if (actingSeat.stack == 0)
                        actingSeat.is_all_in = true;
                    break;
            }

            // Fire action seat changed BEFORE notifying action (drives FeaturedSeatDisplay).
            // This is re-displaying an action that's already resolved — never "thinking".
            PokerTableEvents.OnSeatThinkingChanged?.Invoke(null);
            PokerTableEvents.OnActionSeatChanged?.Invoke(evt.seat);
            UIManager.Instance?.ShowActionLabel(evt.seat, actionName, evt.amount);

            // Chip animation
            int prevActingStack = evt.seat < prevStacks.Count ? prevStacks[evt.seat] : 0;
            int newActingStack  = evt.seat < evt.stacks.Count ? evt.stacks[evt.seat] : 0;
            int chipsPaid       = Mathf.Max(0, prevActingStack - newActingStack);
            if (chipsPaid > 0)
                PokerTableEvents.OnChipsToPot?.Invoke(evt.seat, chipsPaid);

            // Emotion updates — fire for any seat with emotion != -1
            if (evt.emotions != null)
            {
                for (int i = 0; i < evt.emotions.Count; i++)
                {
                    if (evt.emotions[i] >= 0)
                        PokerTableEvents.OnEmotionChanged?.Invoke(i, evt.emotions[i]);
                }
            }

            SyncAllSeats();
            UIManager.Instance?.SyncFromDisplayState(_displayState);

            await WaitSeconds(actionResultVisibleDuration);

            // ── Thinking suppression ──────────────────────────────────────────

            bool isFold   = evt.action_type == 0;
            bool bothAllIn = AllSeatsAllInOrFolded();

            // Matching call suppression: preflop only (street == 0)
            bool isMatchingCall = evt.action_type == 2
                               && (_lastActionType == 3 || _lastActionType == 4)
                               && evt.street == 0
                               && AllCommittedEqual();

            bool betsAreEqual = AllCommittedEqual();
            bool isCheckCheck = evt.action_type == 1
                             && (_lastActionType == 1
                             || (_lastActionType == 2 && betsAreEqual && evt.street == 0));

            string nextKind            = PeekNextQueuedKind();
            bool queueEnding           = nextKind == "HandCompleted" || nextKind == "MatchCompleted";
            bool queueEngineTransition = nextKind == "EngineActionTaken";

            // Does the very next queued event (whatever its kind) actually carry
            // new community cards? Catches the case where a street advance is
            // delivered as a plain "ActionTaken" rather than "EngineActionTaken".
            bool nextEventBringsNewCards = PeekNextQueuedEventBringsNewCommunityCards();

            // Suppress thinking only when the game state is genuinely still
            // transitioning at this point: all-in showdown, hand/match ending,
            // engine advance, or another reveal already queued right behind this
            // one. streetAdvanced is deliberately NOT included here — it only
            // describes whether THIS event's reveal already happened, which by
            // this point is fully done and displayed (that suppression was
            // already applied earlier, before the reveal). Keeping it here would
            // wrongly block the next seat's turn any time a street-advance event
            // bundles the new street's first action (e.g. a check) alongside it.
            bool suppressThinking = bothAllIn || queueEnding || queueEngineTransition
                                 || nextEventBringsNewCards;

            _lastActionType = evt.action_type;

            if (enableThinkingStep && !suppressThinking)
            {
                int nextSeat = NextActiveSeatAfter(evt.seat);
                if (nextSeat >= 0)
                {
                    PokerTableEvents.OnActionSeatChanged?.Invoke(nextSeat);
                    UIManager.Instance?.ShowActionLabel(nextSeat, "thinking", 0);

                    // This IS the genuine speculative case: Unity is guessing who
                    // acts next before the backend's real event for it has arrived.
                    PokerTableEvents.OnSeatThinkingChanged?.Invoke(nextSeat);
                }
            }

            await WaitSeconds(delayBetweenActions);
        }

        // ── BanterEvent ───────────────────────────────────────────────────────

        private async Task HandleBanterEvent(string json)
        {
            BanterEvent evt = Deserialize<BanterEvent>(json);
            if (evt == null) return;

            if (string.IsNullOrWhiteSpace(evt.banter_text))
            {
                Debug.LogWarning("[ActionManager] BanterEvent — empty banter_text, skipping.");
                return;
            }

            Debug.Log($"[ActionManager] BanterEvent — seat {evt.banter_seat}: \"{evt.banter_text}\"");

            PokerTableEvents.OnBanterReceived?.Invoke(evt.banter_seat, evt.banter_text);

            // BanterBar auto-hides after its own internal duration.
            // ActionManager waits a fixed time to let banter display before processing next event.
            await WaitSeconds(4.5f);
        }

        // ── HandCompleted ─────────────────────────────────────────────────────

        private async Task HandleHandCompleted(string json)
        {
            HandCompletedEvent evt = Deserialize<HandCompletedEvent>(json);
            if (evt == null || _displayState == null) return;

            int seatCount = _displayState.seats.Length;

            // ── Update stacks, reset bets ─────────────────────────────────────
            for (int i = 0; i < seatCount; i++)
            {
                if (i < evt.stacks_after.Count)
                    _displayState.seats[i].stack = evt.stacks_after[i];
                _displayState.seats[i].current_bet = 0;
            }

            _displayState.pot = evt.pot;

            // ── Community cards ───────────────────────────────────────────────
            if (evt.community_cards != null)
            {
                bool boardChanged =
                    _displayState.community_cards == null ||
                    _displayState.community_cards.Count != evt.community_cards.Count;

                if (!boardChanged)
                {
                    for (int i = 0; i < evt.community_cards.Count; i++)
                    {
                        if (_displayState.community_cards[i] != evt.community_cards[i])
                        {
                            boardChanged = true;
                            break;
                        }
                    }
                }

                _displayState.community_cards = new List<int>(evt.community_cards);

                if (boardChanged)
                    PokerTableEvents.OnCommunityCardsChanged?.Invoke(
                        new List<int>(evt.community_cards));

                UIManager.Instance?.SyncFromDisplayState(_displayState);
            }

            RefreshSeatWinChances();
            PokerTableEvents.OnSeatThinkingChanged?.Invoke(null);
            PokerTableEvents.OnActionSeatChanged?.Invoke(null);
            UIManager.Instance?.ShowWaitingForResults();

            // ── Showdown: reveal hole cards ───────────────────────────────────
            bool isShowdown = evt.ended_by == 1;
            string handName = "";

            if (isShowdown && evt.hole_cards != null)
            {
                // Sync all hole cards from the completed event
                for (int i = 0; i < seatCount && i < evt.hole_cards.Count; i++)
                {
                    var cards = evt.hole_cards[i];
                    _displayState.seats[i].hole_card_0 = cards.Count > 0 ? cards[0] : -1;
                    _displayState.seats[i].hole_card_1 = cards.Count > 1 ? cards[1] : -1;
                }

                // Push hole cards to UIPlayers (already face-up — spectator game)
                for (int i = 0; i < seatCount && i < evt.hole_cards.Count; i++)
                {
                    var cards = evt.hole_cards[i];
                    if (cards.Count < 2) continue;
                    UIPlayer uiPlayer = GameManager.Instance?.GetSeatController(i)?.GetComponent<UIPlayer>();
                    uiPlayer?.SetHoleCards(cards[0], cards[1], null, dealAnimation: false);
                }

                await WaitSeconds(delayShowdownReveal);

                // Build winner best-five lists for the winning hand highlight
                List<int>        winnerSeats    = evt.winners ?? new List<int>();
                List<List<int>>  bestFivePerSeat = new List<List<int>>();

                foreach (int winSeat in winnerSeats)
                {
                    if (winSeat >= seatCount || winSeat >= evt.hole_cards.Count)
                    {
                        bestFivePerSeat.Add(new List<int>());
                        continue;
                    }

                    var cards = evt.hole_cards[winSeat];
                    if (cards.Count < 2)
                    {
                        bestFivePerSeat.Add(new List<int>());
                        continue;
                    }

                    var (bestFive, detectedHandName) = PokerOddsCalculator.GetBestFiveWithName(
                        cards[0], cards[1], _displayState.community_cards);

                    bestFivePerSeat.Add(bestFive);

                    if (string.IsNullOrEmpty(handName))
                        handName = detectedHandName;
                }

                PokerTableEvents.OnWinningHandRevealed?.Invoke(winnerSeats, bestFivePerSeat, handName);
                await WaitSeconds(0.4f);
            }

            // ── Fire hand result ──────────────────────────────────────────────
            List<int> winners = evt.winners ?? new List<int>();
            
            await CollectBetsToPotAsync();

           PokerTableEvents.OnPotAwarded?.Invoke(
               new List<int>(winners),
               evt.pot);

            PokerTableEvents.OnHandResult?.Invoke(winners, evt.pot);
            UIManager.Instance?.ShowHandWinner(winners, evt.pot, isShowdown, handName);
            UIManager.Instance?.ShowWinnerTurnPrompt(winners);

            SyncAllSeats();
            await WaitSeconds(delayAfterWinner);

            // ── Handle eliminations ───────────────────────────────────────────
            List<int> newlyEliminated = evt.eliminated_seats ?? new List<int>();

            foreach (int elimSeat in newlyEliminated)
            {
                if (_eliminatedSeats.Contains(elimSeat)) continue;
                _eliminatedSeats.Insert(0, elimSeat); // most recent first

                // Mark in display state
                if (elimSeat < seatCount)
                    _displayState.seats[elimSeat].is_eliminated = true;

                // Notify seat controller (fires OnEliminated → RosterRow switches to eliminated panel)
                SeatController sc = GameManager.Instance?.GetSeatController(elimSeat);
                sc?.SyncFromDisplay(_displayState.seats[elimSeat]);

                PokerTableEvents.OnSeatEliminated?.Invoke(elimSeat);
                Debug.Log($"[ActionManager] Seat {elimSeat} eliminated.");
            }

            // ── Wait for next hand data before fading ─────────────────────────
            Debug.Log("[ActionManager] Holding — waiting for next HandDealt before fade-out...");
            await WaitUntilNextHandQueued();
            Debug.Log("[ActionManager] Next hand data received — starting staged fade-out.");

            // ── Staged fade-out ───────────────────────────────────────────────
            await WaitSeconds(stagedDelayClearActions);
            PokerTableEvents.OnClearActionLabels?.Invoke();

            await WaitSeconds(stagedDelayClearBoard);
            PokerTableEvents.OnClearBoardAndPot?.Invoke();

            await WaitSeconds(stagedDelayClearHoleCards);
            PokerTableEvents.OnClearHoleCards?.Invoke();

            // Fade out compact table nodes. They stay invisible while UI resets.
            // FadeInTableNodes fires after Phase-0 in next HandDealt once everything
            // is repositioned — eliminating the "ghost in the middle" problem.
            PokerTableEvents.OnFadeOutTableNodes?.Invoke();

            await WaitSeconds(stagedDelayClearLog);
            PokerTableEvents.OnClearActionLog?.Invoke();

            // ── Between-hand operations ───────────────────────────────────────
            if (newlyEliminated.Count > 0)
            {
                List<int> activeSeats = GetActiveSeats();

                // Deactivate compact nodes for eliminated seats + animate gap-close
                TableNodeLayoutManager.Instance?.AnimateGapClose(activeSeats, _eliminatedSeats);

                // Reorder roster rows
                RosterPanel.Instance?.RefreshOrder(activeSeats, _eliminatedSeats);

                await WaitSeconds(delayAfterGapClose);

                // Notify all listeners that the active seat list has changed
                PokerTableEvents.OnActiveSeatsChanged?.Invoke(activeSeats);
            }

            await WaitSeconds(stagedDelayBeforeNextHand);
        }

        // ── MatchCompleted ────────────────────────────────────────────────────

        private async Task HandleMatchCompleted(string json)
        {
            MatchCompletedEvent evt = Deserialize<MatchCompletedEvent>(json);
            if (evt == null) return;

            if (_displayState != null)
            {
                _displayState.match_over   = true;
                _displayState.match_winner = evt.winner;

                // Update final stacks
                int seatCount = _displayState.seats.Length;
                for (int i = 0; i < seatCount && i < evt.final_stacks.Count; i++)
                    _displayState.seats[i].stack = evt.final_stacks[i];
            }

            // Resolve winner name from SeatController
            SeatController winnerSeat = GameManager.Instance?.GetSeatController(evt.winner);
            string winnerName = winnerSeat
                ? (!string.IsNullOrWhiteSpace(winnerSeat.DisplayName)
                    ? winnerSeat.DisplayName : $"Agent {evt.winner}")
                : $"Agent {evt.winner}";

            Debug.Log($"[ActionManager] Match over. Winner: {winnerName} (seat {evt.winner}). Total hands: {evt.total_hands}");

            PokerTableEvents.OnSeatThinkingChanged?.Invoke(null);
            PokerTableEvents.OnActionSeatChanged?.Invoke(null);
            PokerTableEvents.OnMatchOver?.Invoke(winnerName);
            UIManager.Instance?.ShowMatchOver(winnerName);
            GameOverScreen.Instance?.NotifyMatchCompleted(evt.winner);

            await WaitSeconds(0.1f);
        }

        // ── Seat sync ─────────────────────────────────────────────────────────

       
        /// Syncs all SeatControllers from display state.
        /// Does NOT push hole cards to RosterRow — that is handled exclusively
        /// by WaitForAllSeatsDealt (animated deal) and HandleHandCompleted (showdown reveal).
        private void SyncAllSeats()
        {
            if (_displayState == null) return;
            for (int i = 0; i < _displayState.seats.Length; i++)
            {
                SeatController seat = GameManager.Instance?.GetSeatController(i);
                seat?.SyncFromDisplay(_displayState.seats[i]);
            }
        }
        
        /// Syncs stack, bet, status, badges — hole cards masked to -1.
        /// Used during blind intro Phases 0–2 to avoid triggering card display.
        private void SyncAllSeatsExceptHoleCards()
        {
            if (_displayState == null) return;
            for (int i = 0; i < _displayState.seats.Length; i++)
            {
                SeatController seat = GameManager.Instance?.GetSeatController(i);
                if (seat == null) continue;

                SeatDisplayState s = _displayState.seats[i];
                SeatDisplayState masked = new SeatDisplayState
                {
                    seat_index     = s.seat_index,
                    stack          = s.stack,
                    current_bet    = s.current_bet,
                    is_dealer      = s.is_dealer,
                    is_small_blind = s.is_small_blind,
                    is_big_blind   = s.is_big_blind,
                    is_folded      = s.is_folded,
                    is_all_in      = s.is_all_in,
                    is_eliminated  = s.is_eliminated,
                    hole_card_0    = -1,
                    hole_card_1    = -1,
                    emotion        = s.emotion
                };

                seat.SyncFromDisplay(masked);
            }
        }

        // ── Reset ─────────────────────────────────────────────────────────────

        public void ResetState()
        {
            _eventQueue.Clear();
            _isProcessing = false;
            _displayState = null;
            _lastCommunityCards.Clear();
            _eliminatedSeats.Clear();
            _lastActionType      = -1;
            _lastProcessedStreet = 0;

            _handStartStacks.Clear();
            _streetStartStacks.Clear();

            Debug.Log("[ActionManager] State reset.");
        }

        // ── Win chances ───────────────────────────────────────────────────────

        private void RefreshSeatWinChances()
        {
            if (_displayState == null) return;

            // Build list of active (non-folded, non-eliminated) seats with valid hole cards
            var activeSeatHands = new List<(int seatIndex, int card0, int card1)>();

            for (int i = 0; i < _displayState.seats.Length; i++)
            {
                SeatDisplayState s = _displayState.seats[i];
                if (s.is_folded || s.is_eliminated) continue;
                if (s.hole_card_0 < 0 || s.hole_card_1 < 0) continue;
                activeSeatHands.Add((i, s.hole_card_0, s.hole_card_1));
            }

            if (activeSeatHands.Count < 2) return;

            Dictionary<int, float> equity = PokerOddsCalculator.CalculateMultiWayEquity(
                activeSeatHands, _displayState.community_cards);

            foreach (var kvp in equity)
            {
                SeatController sc = GameManager.Instance?.GetSeatController(kvp.Key);
                sc?.SetWinChance(kvp.Value);
            }

            // Zero out folded seats
            for (int i = 0; i < _displayState.seats.Length; i++)
            {
                SeatDisplayState s = _displayState.seats[i];
                if (!s.is_folded && !s.is_eliminated) continue;
                SeatController sc = GameManager.Instance?.GetSeatController(i);
                sc?.SetWinChance(0f);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void InitStackBaselines(int seatCount)
        {
            _handStartStacks.Clear();
            _streetStartStacks.Clear();
            for (int i = 0; i < seatCount; i++)
            {
                _handStartStacks.Add(0);
                _streetStartStacks.Add(0);
            }
        }

        private List<int> GetActiveSeats()
        {
            if (_displayState == null) return new List<int>();
            List<int> active = new List<int>();
            for (int i = 0; i < _displayState.seats.Length; i++)
                if (!_displayState.seats[i].is_eliminated)
                    active.Add(i);
            return active;
        }

        private bool AllSeatsAllInOrFolded()
        {
            if (_displayState == null) return false;
            foreach (SeatDisplayState s in _displayState.seats)
                if (!s.is_folded && !s.is_all_in && !s.is_eliminated) return false;
            return true;
        }
        
        /// Returns true when all active (non-folded, non-eliminated) seats have
        /// committed the same amount this street. Used for check-check / call detection.
        private bool AllCommittedEqual()
        {
            if (_displayState == null) return false;
            int? reference = null;
            for (int i = 0; i < _displayState.seats.Length; i++)
            {
                SeatDisplayState s = _displayState.seats[i];
                if (s.is_folded || s.is_eliminated) continue;
                int committed = i < _streetStartStacks.Count
                    ? _streetStartStacks[i] - s.stack
                    : 0;
                if (reference == null) reference = committed;
                else if (committed != reference.Value) return false;
            }
            return true;
        }
        
        /// Returns the next active (non-folded, non-eliminated) seat index
        /// after the given seat, wrapping around. Returns -1 if none found.
        private int NextActiveSeatAfter(int currentSeat)
        {
            if (_displayState == null) return -1;
            int n = _displayState.seats.Length;
            for (int offset = 1; offset < n; offset++)
            {
                int candidate = (currentSeat + offset) % n;
                SeatDisplayState s = _displayState.seats[candidate];
                if (!s.is_folded && !s.is_eliminated)
                    return candidate;
            }
            return -1;
        }
        
        /// Deals cards to all active RosterRows with animation.
        /// Each UIPlayer.SetHoleCards fires its onComplete callback when both
        /// cards are fully dealt and flipped. This method waits for all N seats.
        private Task WaitForAllSeatsDealt(int seatCount)
        {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(DealCardsToAllRows(tcs, seatCount));
            return tcs.Task;
        }

        private IEnumerator DealCardsToAllRows(TaskCompletionSource<bool> tcs, int seatCount)
        {
            if (_displayState == null) { tcs.TrySetResult(true); yield break; }

            int expected = 0;
            int done     = 0;

            // Count active non-eliminated seats with valid hole cards
            for (int i = 0; i < seatCount; i++)
            {
                SeatDisplayState s = _displayState.seats[i];
                if (s.is_eliminated || s.hole_card_0 < 0 || s.hole_card_1 < 0) continue;
                expected++;
            }

            if (expected == 0) { tcs.TrySetResult(true); yield break; }

            System.Action oneSeatDone = () =>
            {
                done++;
                if (done >= expected)
                    tcs.TrySetResult(true);
            };

            // Trigger animated deal on each UIPlayer (table node)
            for (int i = 0; i < seatCount; i++)
            {
                SeatDisplayState s = _displayState.seats[i];
                if (s.is_eliminated || s.hole_card_0 < 0 || s.hole_card_1 < 0) continue;

                UIPlayer uiPlayer = GameManager.Instance?.GetSeatController(i)?.GetComponent<UIPlayer>();
                if (uiPlayer != null)
                    uiPlayer.SetHoleCards(s.hole_card_0, s.hole_card_1, oneSeatDone);
                else
                    oneSeatDone(); // player missing — don't hang
            }

            // Safety timeout
            float timeout = 12f;
            float elapsed = 0f;
            while (!tcs.Task.IsCompleted)
            {
                elapsed += Time.deltaTime;
                if (elapsed >= timeout)
                {
                    Debug.LogWarning("[ActionManager] WaitForAllSeatsDealt timed out — proceeding.");
                    tcs.TrySetResult(true);
                }
                yield return null;
            }
        }

        private Task WaitUntilNextHandQueued()
        {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(PollForNextHand(tcs));
            return tcs.Task;
        }

        private IEnumerator PollForNextHand(TaskCompletionSource<bool> tcs)
        {
            while (true)
            {
                foreach (string queued in _eventQueue)
                {
                    string kind = PeekKind(queued);
                    if (kind == "HandDealt" || kind == "MatchCompleted")
                    {
                        tcs.SetResult(true);
                        yield break;
                    }
                }
                yield return null;
            }
        }

        private string PeekKind(string json)
        {
            try
            {
                JObject obj = JObject.Parse(json);
                return obj["kind"]?.ToString();
            }
            catch { return null; }
        }

        private string PeekNextQueuedKind()
        {
            foreach (string queued in _eventQueue)
                return PeekKind(queued);
            return null;
        }


        /// Looks at the actual payload of the next queued event (not just its
        /// "kind") to see whether IT will bring new community cards.
        private bool PeekNextQueuedEventBringsNewCommunityCards()
        {
            foreach (string queued in _eventQueue)
            {
                try
                {
                    JObject obj  = JObject.Parse(queued);
                    string  kind = obj["kind"]?.ToString();

                    if (kind != "ActionTaken" && kind != "EngineActionTaken")
                        return false;

                    JArray cards = obj["community_cards"] as JArray;
                    int nextCount = cards?.Count ?? 0;
                    return nextCount > _lastCommunityCards.Count;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        private T Deserialize<T>(string json) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ActionManager] Failed to deserialize {typeof(T).Name}: {e.Message}");
                return null;
            }
        }

        private Task WaitSeconds(float seconds)
        {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(WaitCoroutine(seconds, tcs));
            return tcs.Task;
        }

        private IEnumerator WaitCoroutine(float seconds, TaskCompletionSource<bool> tcs)
        {
            yield return new WaitForSeconds(seconds);
            tcs.SetResult(true);
        }
        
        
        private Task CollectBetsToPotAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            if (PokerTableEvents.OnCollectBetsToPot == null)
            {
                tcs.TrySetResult(true);
                return tcs.Task;
            }

            PokerTableEvents.OnCollectBetsToPot.Invoke(() =>
            {
                tcs.TrySetResult(true);
            });

            return tcs.Task;
        }
    }
}