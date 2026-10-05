using System.Collections;
using System.Collections.Generic;
using SimplePoker.Audio;
using SimplePoker.Data;
using SimplePoker.ScriptableObjects;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TexasHoldem
{ 
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        // ── Community Cards ───────────────────────────────────────────────────

        [Header("Community Cards")]
        [SerializeField] private List<Image>               communityCardImages;
        [SerializeField] private List<CommunityCardVisual> communityCardVisuals;
        [SerializeField] private DeckData                  deckData;
        [SerializeField] private Sprite                    cardBackSprite;

        [Header("Deal Animation")]
        [SerializeField] private Transform dealOrigin;
        [SerializeField] private float communityDealInitialDelay = 0.08f;
        [SerializeField] private float communityDealStagger = 0.12f;
        public Transform DealOrigin => dealOrigin;

        // ── Pot ───────────────────────────────────────────────────────────────

        [Header("Pot")]
        [SerializeField] private TextMeshProUGUI potText;
        [SerializeField] private float           potCountDuration = 0.4f;
        [SerializeField] private UIPotStack potStackVisual;

        // ── Street ────────────────────────────────────────────────────────────

        [Header("Street")]
        [SerializeField] private TextMeshProUGUI streetLabel;
        [SerializeField] private Image           streetLabelBackground;

        // ── Hand Info ─────────────────────────────────────────────────────────

        [Header("Hand Info")]
        [SerializeField] private TextMeshProUGUI handNumberText;

        // ── Blind Info ────────────────────────────────────────────────────────

        [Header("Blinds")]
        [SerializeField] private TextMeshProUGUI currentBlindText;
        [SerializeField] private TextMeshProUGUI blindIncreaseCountdownText;
        [SerializeField] private int             blindsIncreaseEvery = 4;
        [SerializeField] private CanvasGroup currentBlindCanvasGroup;
        [SerializeField] private float blindFadeInDuration = 0.5f;
        [SerializeField] private float blindVisibleDuration = 10f;
        [SerializeField] private float blindFadeDuration = 0.5f;
        
        
        // ── Trade Link ─────────────────────────────────────────────────────────

        [Header("Trade Link")]
        [SerializeField] private GameObject tradeLink;
        [SerializeField] private CanvasGroup tradeLinkCanvasGroup;
        [SerializeField] private float tradeLinkFadeDuration = 1f;
        [SerializeField] private int tradeLinkPotThreshold = 100;

        private bool _tradeLinkShownThisHand = false;
        private bool _blindIntroFinished = false;
        

        // ── Action HUD ────────────────────────────────────────────────────────

        [Header("Action HUD")]
        [SerializeField] private TextMeshProUGUI turnPromptText;
        [SerializeField] private string          turnPromptTemplate = "{0}'s Turn ";
        [SerializeField] private string          firstTurnPromptTemplate = "{0}'s Turn ";
        [SerializeField] private TextMeshProUGUI lastActionLine1Text;
        [SerializeField] private TextMeshProUGUI lastActionLine2Text;
        [SerializeField] private string systemLogColor = "#D4AF37";
        
        // ── Frame Glint ──────────────────────────────────────────────────────
        
        [Header("Frame Glint")]
        [SerializeField] private RectTransform frameGlint;
        [SerializeField] private Vector2 frameGlintPointA;
        [SerializeField] private Vector2 frameGlintPointB;
        [SerializeField] private float frameGlintDuration = 0.85f;
        [SerializeField] private float frameGlintWaitAtEnd = 2f;
        
        // ── Rotating Star ──────────────────────────────────────────────────────
        [Header("Rotating Star")]
        [SerializeField] private RectTransform rotatingStar;
        [SerializeField] private float starRotationSpeed = 35f; // degrees per second
        [SerializeField] private bool rotateStarOnStart = true;

        
        // ── Winner Panel ──────────────────────────────────────────────────────

        [Header("Winner Panel")]
        [SerializeField] private GameObject      winnerPanel;
        [SerializeField] private TextMeshProUGUI winnerNameText;
        [SerializeField] private TextMeshProUGUI winnerAmountText;
        [SerializeField] private TextMeshProUGUI winnerHandText;
        [SerializeField] private float           winnerPanelDuration = 4f;

        // ── Match Over ────────────────────────────────────────────────────────

        [Header("Match Over")]
        [SerializeField] private GameObject      matchOverPanel;
        [SerializeField] private TextMeshProUGUI matchOverText;

        // ── Street Colors ─────────────────────────────────────────────────────

        [Header("Street Colors")]
        [SerializeField] private Color colorPreflop  = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] private Color colorFlop     = new Color(0.1f, 0.4f, 0.1f);
        [SerializeField] private Color colorTurn     = new Color(0.1f, 0.2f, 0.5f);
        [SerializeField] private Color colorRiver    = new Color(0.5f, 0.1f, 0.1f);
        [SerializeField] private Color colorShowdown = new Color(0.5f, 0.4f, 0.0f);

        // ── Private ───────────────────────────────────────────────────────────

        private PokerGameAssetData _asset;
        private SoundManager       _sound;
        private int                _currentPot    = 0;
        private int                _currentHand   = 0;
        private int?               _currentToActSeat = null;
        private readonly List<int> _shownCommunityCards = new List<int>();
        private Coroutine _blindFadeCoroutine;
        private Coroutine _tradeLinkThresholdCoroutine;
        private bool _isFirstTurnOfHand = false;
        private string _previousHandWinnerSummary = "";

        private readonly List<string> _lastActionBySeat = new List<string>();
        private string _lastOverallAction = "";
        private string _secondLastOverallAction = "";
        private string _pendingStreetDealtLine = "";
        private int _tableChipTotal = 0;
        private Coroutine _frameGlintCoroutine;
        private Coroutine _starRotationCoroutine;
        
        public int CurrentPot => _currentPot;
        
        public int TableChipTotal => Mathf.Max(1, _tableChipTotal);

        private PokerGameAssetData Asset
        {
            get
            {
                if (!_asset && PokerGameAsset.Instance)
                    _asset = PokerGameAsset.Instance.PokerGameAssetData;
                return _asset;
            }
        }

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            PokerTableEvents.OnNewHand               += HandleNewHand;
            PokerTableEvents.OnStreetChanged         += HandleStreetChanged;
            PokerTableEvents.OnPotChanged            += HandlePotChanged;
            PokerTableEvents.OnCommunityCardsChanged += HandleCommunityCardsChanged;
            PokerTableEvents.OnActionSeatChanged     += HandleActionSeatChanged;
            PokerTableEvents.OnWinningHandRevealed   += HandleWinningHandRevealed;
            PokerTableEvents.OnClearBoardAndPot      += HandleClearBoardAndPot;
            PokerTableEvents.OnClearActionLog        += HandleClearActionLog;

            ResetTable();
        }

        private void Start()
        {
            _sound = SoundManager.Instance;
            if (rotateStarOnStart)
                StartRotatingStar();
        }

        private void OnDestroy()
        {
            PokerTableEvents.OnNewHand               -= HandleNewHand;
            PokerTableEvents.OnStreetChanged         -= HandleStreetChanged;
            PokerTableEvents.OnPotChanged            -= HandlePotChanged;
            PokerTableEvents.OnCommunityCardsChanged -= HandleCommunityCardsChanged;
            PokerTableEvents.OnActionSeatChanged     -= HandleActionSeatChanged;
            PokerTableEvents.OnWinningHandRevealed   -= HandleWinningHandRevealed;
            PokerTableEvents.OnClearBoardAndPot      -= HandleClearBoardAndPot;
            PokerTableEvents.OnClearActionLog        -= HandleClearActionLog;
            
            if (_blindFadeCoroutine != null)
            {
                StopCoroutine(_blindFadeCoroutine);
                _blindFadeCoroutine = null;
            }

            if (currentBlindCanvasGroup != null)
            {
                currentBlindCanvasGroup.DOKill();
                currentBlindCanvasGroup.alpha = 1f;
            }
            
            if (tradeLinkCanvasGroup != null)
            {
                tradeLinkCanvasGroup.DOKill();
            }
            
            if (_frameGlintCoroutine != null)
            {
                StopCoroutine(_frameGlintCoroutine);
                _frameGlintCoroutine = null;
            }
            if (_starRotationCoroutine != null)
            {
                StopCoroutine(_starRotationCoroutine);
                _starRotationCoroutine = null;
            }
        }

        // ── PokerTableEvents handlers ─────────────────────────────────────────

        private void HandleNewHand(int handNumber)
        {
            StartFrameGlint();
            
            _isFirstTurnOfHand = true;
            _currentHand = handNumber;
            _currentToActSeat = null;
            
            _tradeLinkShownThisHand = false;
            _blindIntroFinished = false;

            if (handNumberText)
                handNumberText.SetText($"HAND #{handNumber}");

            UpdateBlindIncreaseCountdown(handNumber);

            _shownCommunityCards.Clear();
            ResetCommunityCardsVisuals();

            if (winnerPanel != null) winnerPanel.SetActive(false);

            ClearActionHistory();

            SetVisibleLog(
                FormatSystemLog($"Dealing Hand #{handNumber}"),
                string.IsNullOrEmpty(_previousHandWinnerSummary) ? "" : FormatSystemLog(_previousHandWinnerSummary),
                ""
            );

            ShowBlindInfoForNewHand();
        }

        private void HandleStreetChanged(int street)
        {
            string name = StreetNames.FromInt(street);

            if (streetLabel)
                streetLabel.SetText(name.ToUpper());

            if (streetLabelBackground)
                streetLabelBackground.DOColor(GetStreetColor(street), 0.3f);

            string dealingText = "";
            string dealtText   = "";

            switch (street)
            {
                case 1:
                    dealingText = "Dealing Flop";
                    dealtText   = "Flop dealt";
                    _sound.PlayOneShotSong(Asset.Audio_3Cards);
                    break;
                case 2:
                    dealingText = "Dealing Turn";
                    dealtText   = "Turn dealt";
                    _sound.PlayOneShotSong(Asset.Audio_1Card);
                    break;
                case 3:
                    dealingText = "Dealing River";
                    dealtText   = "River dealt";
                    _sound.PlayOneShotSong(Asset.Audio_1Card);
                    break;
                case 4:
                    dealingText = "Showdown";
                    dealtText   = "Showdown";
                    break;
            }

            if (string.IsNullOrEmpty(dealingText))
                return;

            _pendingStreetDealtLine = dealtText;

            SetVisibleLog(
                FormatSystemLog(dealingText),
                _lastOverallAction,
                _secondLastOverallAction
            );
        }

        private void HandlePotChanged(int prev, int next)
        {
            _currentPot = next;

            TryShowTradeLinkForPot(next);
            

            if (potText == null) return;

            DOTween.To(() => prev, v => potText.SetText($"POT: {v}"), next, potCountDuration);
        }

        private void HandleCommunityCardsChanged(List<int> cards)
        {
            SyncCommunityCards(cards);
        }

        private void HandleActionSeatChanged(int? seatIndex)
        {
            if (!seatIndex.HasValue)
                return;

            LogTurn(seatIndex);
        }

        public void LogTurn(int? seatIndex)
        {
            if (!seatIndex.HasValue)
                return;

            int activeSeat = seatIndex.Value;
            _currentToActSeat = activeSeat;

            string activeName = GetLogSeatName(activeSeat);

            string top = FormatSystemLog(
                _isFirstTurnOfHand
                    ? string.Format(firstTurnPromptTemplate, activeName)
                    : string.Format(turnPromptTemplate, activeName)
            );

            if (!string.IsNullOrEmpty(_pendingStreetDealtLine))
            {
                string dealtLine = FormatSystemLog(_pendingStreetDealtLine);

                SetVisibleLog(
                    top,
                    dealtLine,
                    _lastOverallAction ?? ""
                );

                RecordSystemAction(dealtLine);
                _pendingStreetDealtLine = "";
            }
            else
            {
                SetVisibleLog(
                    top,
                    _lastOverallAction ?? "",
                    _secondLastOverallAction ?? ""
                );
            }

            if (_isFirstTurnOfHand)
                _isFirstTurnOfHand = false;
        }

        // ── Winning hand highlight on community cards ─────────────────────────
        // Unions all winners' best-five lists. Any community card that appears
        // in any winner's best five is highlighted; all others are dimmed.
        // Handles single winner, split pot, and draw uniformly.

        private void HandleWinningHandRevealed(List<int> winnerSeats,
                                               List<List<int>> bestFivePerSeat,
                                               string handName)
        {
            if (communityCardVisuals == null) return;

            // Union all winners' best-five lists so every contributing community
            // card highlights correctly — covers normal win, split pot, and draw.
            HashSet<int> winSet = new HashSet<int>();
            if (bestFivePerSeat != null)
                foreach (List<int> five in bestFivePerSeat)
                    if (five != null)
                        foreach (int c in five) winSet.Add(c);

            if (winSet.Count == 0) return;

            for (int i = 0; i < communityCardVisuals.Count; i++)
            {
                CommunityCardVisual ccv = communityCardVisuals[i];
                if (ccv == null || !ccv.gameObject.activeSelf) continue;

                if (winSet.Contains(ccv.CurrentCardInt))
                    ccv.HighlightAsWinner();
                else
                    ccv.DimAsNonWinner();
            }
        }

        // Staged cleanup event handlers ─────────────────────────────

        private void HandleClearBoardAndPot()
        {
            
            FadeOutTradeLink();
            
            // Animate pot to 0 then reset display
            int prevPot = _currentPot;
            _currentPot = 0;
            if (potText)
                DOTween.To(() => prevPot, v => potText.SetText($"POT: {v}"), 0, potCountDuration)
                       .OnComplete(() => potText.SetText("POT: 0"));
            
            HidePotStackAnimated();

            // Fade out community cards
            if (communityCardVisuals != null)
            {
                foreach (CommunityCardVisual ccv in communityCardVisuals)
                {
                    if (!ccv || !ccv.gameObject.activeSelf) continue;
                    ccv.ClearHighlight();
                    StartCoroutine(FadeOutCommunityCard(ccv, 0.4f));
                }
            }
            else if (communityCardImages != null)
            {
                foreach (Image img in communityCardImages)
                    if (img && img.gameObject.activeSelf)
                        img.DOFade(0f, 0.4f).OnComplete(() => img.gameObject.SetActive(false));
            }

            _shownCommunityCards.Clear();

            // Clear street label
            if (streetLabel) streetLabel.DOFade(0f, 0.3f)
                .OnComplete(() => { streetLabel.SetText(""); streetLabel.DOFade(1f, 0f); });
        }

        private IEnumerator FadeOutCommunityCard(CommunityCardVisual ccv, float duration)
        {
            CanvasGroup cg = ccv.GetComponent<CanvasGroup>();
            if (cg)
                yield return cg.DOFade(0f, duration).WaitForCompletion();
            else
                yield return new WaitForSeconds(duration);

            ccv.ResetInstant();
        }

        private void HandleClearActionLog()
        {
            if (lastActionLine1Text)
                lastActionLine1Text.DOFade(0f, 0.5f).OnComplete(() =>
                {
                    lastActionLine1Text.SetText("");
                    lastActionLine1Text.DOFade(1f, 0f);
                });

            if (lastActionLine2Text)
                lastActionLine2Text.DOFade(0f, 0.5f).OnComplete(() =>
                {
                    lastActionLine2Text.SetText("");
                    lastActionLine2Text.DOFade(1f, 0f);
                });

            if (turnPromptText)
                turnPromptText.DOFade(0f, 0.5f).OnComplete(() =>
                {
                    turnPromptText.SetText("");
                    turnPromptText.DOFade(1f, 0f);
                });

            ClearActionHistory();
        }

        // ── Called by ActionManager ───────────────────────────────────────────

        public void SyncFromDisplayState(MatchDisplayState state)
        {
            // Pot
            if (state.pot != _currentPot)
            {
                int prev = _currentPot;
                _currentPot = state.pot;

                TryShowTradeLinkForPot(state.pot);
                

                if (potText)
                    DOTween.To(() => prev, v => potText.SetText($"POT: {v}"), state.pot, potCountDuration);
            }
            else
            {
                TryShowTradeLinkForPot(state.pot);
                
            }
            
            UpdateTableChipTotalIfNeeded();
            RefreshAllPlayerCoinVisuals();


            // Hand number
            _currentHand = state.hand_index;
            if (handNumberText)
                handNumberText.SetText($"HAND #{state.hand_index}");

            if (currentBlindText)
                currentBlindText.SetText($"\n{state.small_blind_amount:N0} / {state.big_blind_amount:N0}");

            Debug.Log($"[UIManager] SyncFromDisplayState blinds: SB={state.small_blind_amount}, BB={state.big_blind_amount}, hand={state.hand_index}");
        }

        public void ShowActionLabel(int seatIndex, string action, int amount)
        {
            SeatController seat = GameManager.Instance?.GetSeatController(seatIndex);
            if (!seat) return;

            seat.NotifyAction(action, amount);

            string name = GetLogSeatName(seatIndex);
            string a    = action?.ToLower() ?? "";

            if (a == "thinking")
                return;

            string line;
            switch (a)
            {
                case "check":
                    line = $"{name} CHECKED";
                    break;
                case "fold":
                    line = $"{name} FOLDED";
                    break;
                case "call":
                    line = amount > 0 ? $"{name} CALLED {amount}" : $"{name} CALLED";
                    break;
                case "raise":
                    line = amount > 0 ? $"{name} RAISED {amount}" : $"{name} RAISED";
                    break;
                case "blind":
                    return; // blind payment is handled by LogBlindPayment
                case "all_in":
                    line = amount > 0 ? $"{name} Went ALL-IN {amount}" : $"{name} Went ALL-IN";
                    break;
                default:
                    line = amount > 0
                        ? $"{name} {action.ToUpper()} {amount}"
                        : $"{name} {action.ToUpper()}";
                    break;
            }

            RecordSeatAction(seatIndex, line);
            Debug.Log($"[UIManager] {line}");
        }
        
        /// Pushes a single blind payment entry into the rolling log.
        /// Called once per blind by ActionManager, timed with the blind intro sequence.
        /// blindType: "big" or "small"
        public void LogBlindPayment(int seatIndex, string blindType, int amount)
        {
            string name  = GetSeatName(seatIndex);
            string label = blindType?.ToLower() == "big" ? "big blind" : "small blind";
            string line = $"{name} pays {label} {amount}";

            RecordSeatAction(seatIndex, line);

            SetVisibleLog(
                line,
                _lastOverallAction == line ? _secondLastOverallAction : _lastOverallAction,
                ""
            );
        }
        
        /// Shows the hand result panel and updates the rolling log.
        /// winnerSeat == -1 signals a draw — both seats split the pot.
        public void ShowHandWinner(List<int> winners, int pot, bool isShowdown, string handName = "")
        {
            bool isDraw = winners != null && winners.Count > 1;
            bool isNoWinner = winners == null || winners.Count == 0;

            if (isNoWinner) return;

            if (isDraw)
            {
                _previousHandWinnerSummary = $"Draw — Hand #{_currentHand}";

                if (winnerPanel != null)
                {
                    winnerPanel.SetActive(true);
                    winnerPanel.transform.localScale = Vector3.zero;
                    winnerPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                }

                int splitAmount = pot / winners.Count;
                if (winnerNameText) winnerNameText.SetText("Split Pot!");
                if (winnerAmountText) winnerAmountText.SetText($"+{splitAmount:N0} each");

                if (winnerHandText)
                {
                    if (isShowdown && !string.IsNullOrEmpty(handName))
                    {
                        winnerHandText.SetText(handName);
                        winnerHandText.gameObject.SetActive(true);
                    }
                    else
                    {
                        winnerHandText.gameObject.SetActive(false);
                    }
                }

                _sound?.PlayOneShotSong(Asset?.Audio_PlayerWon);
                StartCoroutine(HideAfter(winnerPanel, winnerPanelDuration));

                SetVisibleLog(
                    FormatSystemLog("Split Pot!"),
                    FormatSystemLog($"Each Won {splitAmount:N0} chips"),
                    isShowdown && !string.IsNullOrEmpty(handName)
                        ? FormatSystemLog(handName)
                        : ""
                );

                Debug.Log($"[UIManager] Split pot — {winners.Count} winners, {splitAmount:N0} chips each in hand #{_currentHand}");
                return;
            }

            // ── Single winner ─────────────────────────────────────────────────
            int winnerSeat = winners[0];
            SeatController seat = GameManager.Instance?.GetSeatController(winnerSeat);
            string name = seat
                ? (!string.IsNullOrWhiteSpace(seat.LogName) ? seat.LogName : seat.DisplayName)
                : $"Agent {winnerSeat}";

            _previousHandWinnerSummary = $"{name} Won Hand #{_currentHand}";

            if (winnerPanel)
            {
                winnerPanel.SetActive(true);
                winnerPanel.transform.localScale = Vector3.zero;
                winnerPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }

            if (winnerNameText) winnerNameText.SetText(name);
            if (winnerAmountText) winnerAmountText.SetText($"+{pot:N0}");

            if (winnerHandText)
            {
                if (isShowdown && !string.IsNullOrEmpty(handName))
                {
                    winnerHandText.SetText(handName);
                    winnerHandText.gameObject.SetActive(true);
                }
                else
                {
                    winnerHandText.gameObject.SetActive(false);
                }
            }

            _sound?.PlayOneShotSong(Asset?.Audio_PlayerWon);
            StartCoroutine(HideAfter(winnerPanel, winnerPanelDuration));

            SetVisibleLog(
                FormatSystemLog($"{name} Wins!"),
                FormatSystemLog($"Won {pot:N0} chips"),
                isShowdown && !string.IsNullOrEmpty(handName)
                    ? FormatSystemLog($"Won with a {handName}")
                    : ""
            );

            Debug.Log($"[UIManager] {name} wins {pot:N0} chips" +
                      $"{(isShowdown && !string.IsNullOrEmpty(handName) ? $" with {handName}" : "")} in hand #{_currentHand}");
        }

        public void ShowMatchOver(string winnerName)
        {
            if (matchOverPanel) matchOverPanel.SetActive(true);
            if (matchOverText) matchOverText.SetText($"{winnerName} wins the match!");
            Debug.Log($"[UIManager] Match over. Winner: {winnerName}");
        }

        public void InitializeTable(MatchSetupPayload setup)
        {
            ResetTable();

            // Override the Inspector default if the backend sent a value.
            if (setup.hands_per_level > 0)
                blindsIncreaseEvery = setup.hands_per_level;

            Debug.Log($"[UIManager] Table initialized — match: {setup.match_id}, hands_per_level: {blindsIncreaseEvery}");
        }
        
        /// Updates the top rolling-log line after a hand result.
        /// winnerSeat == -1 means draw.
        public void ShowWinnerTurnPrompt(List<int> winners)
        {
            if (!turnPromptText) return;

            if (winners == null || winners.Count == 0) return;

            if (winners.Count > 1)
            {
                turnPromptText.SetText(FormatSystemLog("Split Pot!"));
                return;
            }

            SeatController seat = GameManager.Instance?.GetSeatController(winners[0]);
            string name = seat
                ? (!string.IsNullOrWhiteSpace(seat.LogName) ? seat.LogName : seat.DisplayName)
                : $"Seat {winners[0]}";
            turnPromptText.SetText(FormatSystemLog($"{name} Wins!"));
        }

        public void ClearSeatActionLabel(int seatIndex)
        {
            SeatController seat = GameManager.Instance?.GetSeatController(seatIndex);
            if (!seat) return;
            seat.ClearCurrentActionLabel();
        }

        public void ShowWaitingForResults()
        {
            SetVisibleLog(
                FormatSystemLog("Waiting for Results"),
                "",
                ""
            );
        }

        public void ClearLatestActionText()
        {
            ClearActionHistory();
            SetVisibleLog("", "", "");
        }

        // ── Reset ─────────────────────────────────────────────────────────────

        public void ResetTable()
        {
            if (_blindFadeCoroutine != null)
            {
                StopCoroutine(_blindFadeCoroutine);
                _blindFadeCoroutine = null;
            }

            if (currentBlindCanvasGroup)
            {
                currentBlindCanvasGroup.DOKill();
                currentBlindCanvasGroup.alpha = 0f;
                currentBlindCanvasGroup.interactable = false;
                currentBlindCanvasGroup.blocksRaycasts = false;
            }
            
            HideTradeLinkInstant();
            _tradeLinkShownThisHand = false;
            _blindIntroFinished = false;

            _currentHand = 0;
            _isFirstTurnOfHand = false;
            _currentToActSeat = null;
            _previousHandWinnerSummary = "";
            HidePotStackInstant();
            ClearActionHistory();
            

            if (lastActionLine1Text) lastActionLine1Text.SetText("");
            if (lastActionLine2Text) lastActionLine2Text.SetText("");
            if (turnPromptText) turnPromptText.SetText("");
            _currentToActSeat = null;

            if (currentBlindText) currentBlindText.SetText("\n- / -");

            _currentPot = 0;
            _tableChipTotal = 0;

            _shownCommunityCards.Clear();
            ResetCommunityCardsVisuals();

            if (potText) potText.SetText("POT: 0");
            if (streetLabel) streetLabel.SetText("");
            if (handNumberText) handNumberText.SetText("HAND #0");
            if (winnerPanel) winnerPanel.SetActive(false);
            if (matchOverPanel) matchOverPanel.SetActive(false);
        }

        // ── Community Cards ───────────────────────────────────────────────────

        // ReSharper disable Unity.PerformanceAnalysis
        private void SyncCommunityCards(List<int> cards)
        {
            if (communityCardImages == null && communityCardVisuals == null) return;
            if (cards == null) cards = new List<int>();

            int previousCount = _shownCommunityCards.Count;
            int newCount = cards.Count;

            for (int i = 0; i < newCount; i++)
            {
                Sprite sprite = GetCardSprite(cards[i]);
                int cardInt = cards[i];
                if (!sprite) continue;

                bool isNewCard = i >= previousCount;

                if (isNewCard)
                {
                    if (communityCardVisuals != null &&
                        i < communityCardVisuals.Count &&
                        communityCardVisuals[i] != null)
                    {
                        Vector3 startPos = DealOrigin != null
                            ? DealOrigin.position
                            : communityCardVisuals[i].transform.position;

                        float delay = communityDealInitialDelay + ((i - previousCount) * communityDealStagger);

                        communityCardVisuals[i].DealAndFlipFromWorld(
                            startPos,
                            cardBackSprite,
                            sprite,
                            cardInt,
                            delay);
                    }
                    else if (communityCardImages != null &&
                             i < communityCardImages.Count &&
                             communityCardImages[i] != null)
                    {
                        communityCardImages[i].sprite = sprite;
                        communityCardImages[i].gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (communityCardVisuals != null &&
                        i < communityCardVisuals.Count &&
                        communityCardVisuals[i])
                    {
                        CommunityCardVisual visual = communityCardVisuals[i];

                        if (visual.CurrentCardInt != cardInt)
                            visual.ShowFrontInstant(sprite, cardInt);
                    }
                    else if (communityCardImages != null &&
                             i < communityCardImages.Count &&
                             communityCardImages[i])
                    {
                        communityCardImages[i].sprite = sprite;
                        communityCardImages[i].gameObject.SetActive(true);
                    }
                }
            }

            // Reset any community card slots beyond the new count
            if (communityCardVisuals != null)
            {
                for (int i = newCount; i < communityCardVisuals.Count; i++)
                    communityCardVisuals[i]?.ResetInstant();
            }
            else if (communityCardImages != null)
            {
                for (int i = newCount; i < communityCardImages.Count; i++)
                    if (communityCardImages[i] != null)
                        communityCardImages[i].gameObject.SetActive(false);
            }

            _shownCommunityCards.Clear();
            _shownCommunityCards.AddRange(cards);
        }

        private Sprite GetCardSprite(int cardByte)
        {
            if (!deckData) return cardBackSprite;
            CardStringParser.ParsedCard parsed = CardStringParser.FromInt(cardByte);
            if (!parsed.IsValid) return cardBackSprite;
            if (parsed.SpriteIndex < 0 || parsed.SpriteIndex >= deckData.Sprites_CardFront.Length)
                return cardBackSprite;
            return deckData.Sprites_CardFront[parsed.SpriteIndex];
        }

        // ── Rolling Log ───────────────────────────────────────────────────────

        private void RecordSeatAction(int seatIndex, string line)
        {
            int seatCount = GameManager.Instance ? GameManager.Instance.SeatCount : 2;
            if (seatIndex < 0 || seatIndex >= seatCount || string.IsNullOrWhiteSpace(line))
                return;

            // Grow list to fit if needed
            while (_lastActionBySeat.Count <= seatIndex)
                _lastActionBySeat.Add("");

            _lastActionBySeat[seatIndex] = line;
            _secondLastOverallAction = _lastOverallAction;
            _lastOverallAction = line;
        }

        private void ClearActionHistory()
        {
            _lastActionBySeat.Clear();
            _lastOverallAction = "";
            _secondLastOverallAction = "";
            _pendingStreetDealtLine = "";
        }

        private void SetVisibleLog(string top, string middle, string bottom)
        {
            if (turnPromptText)      turnPromptText.SetText(top ?? "");
            if (lastActionLine1Text) lastActionLine1Text.SetText(middle ?? "");
            if (lastActionLine2Text) lastActionLine2Text.SetText(bottom ?? "");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private Color GetStreetColor(int street)
        {
            switch (street)
            {
                case 0:  return colorPreflop;
                case 1:  return colorFlop;
                case 2:  return colorTurn;
                case 3:  return colorRiver;
                case 4:  return colorShowdown;
                default: return colorPreflop;
            }
        }

        private IEnumerator HideAfter(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (obj) obj.SetActive(false);
        }

        private void ApplyFont(TMPro.TMP_FontAsset font)
        {
            if (font == null) return;
            if (potText          != null) potText.font          = font;
            if (streetLabel      != null) streetLabel.font      = font;
            if (handNumberText   != null) handNumberText.font   = font;
            if (winnerNameText   != null) winnerNameText.font   = font;
            if (winnerAmountText != null) winnerAmountText.font = font;
            if (matchOverText    != null) matchOverText.font    = font;
        }

        private string FormatSystemLog(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            return $"<color={systemLogColor}>{text}</color>";
        }

        private string GetSeatName(int seatIndex)
        {
            return GetLogSeatName(seatIndex);
        }

        private void ResetCommunityCardsVisuals()
        {
            if (communityCardVisuals != null)
                foreach (CommunityCardVisual ccv in communityCardVisuals)
                    ccv?.ResetInstant();

            if (communityCardImages != null)
                foreach (Image img in communityCardImages)
                    if (img) img.gameObject.SetActive(false);
        }

        private void UpdateBlindIncreaseCountdown(int handNumber)
        {
            if (!blindIncreaseCountdownText) return;

            int handsCompleted    = handNumber - 1;
            int handsIntoInterval = handsCompleted % blindsIncreaseEvery;
            int handsUntil        = blindsIncreaseEvery - handsIntoInterval;

            if (handsUntil == 1)
                blindIncreaseCountdownText.SetText("Blind will increase next hand");
            else
                blindIncreaseCountdownText.SetText($"Blind increases in {handsUntil} hands");
        }

        private void ShowBlindInfoForNewHand()
        {
            if (!currentBlindCanvasGroup) return;

            if (_blindFadeCoroutine != null)
            {
                StopCoroutine(_blindFadeCoroutine);
                _blindFadeCoroutine = null;
            }

            HideTradeLinkInstant();

            currentBlindCanvasGroup.DOKill();
            currentBlindCanvasGroup.gameObject.SetActive(true);
            currentBlindCanvasGroup.alpha = 0f;
            currentBlindCanvasGroup.interactable = false;
            currentBlindCanvasGroup.blocksRaycasts = false;

            _blindFadeCoroutine = StartCoroutine(FadeBlindInfoSequence());
        }

        private IEnumerator FadeBlindInfoSequence()
        {
            if (currentBlindCanvasGroup)
            {
                currentBlindCanvasGroup.DOKill();

                yield return currentBlindCanvasGroup
                    .DOFade(1f, blindFadeInDuration)
                    .WaitForCompletion();
            }

            yield return new WaitForSeconds(blindVisibleDuration);

            if (currentBlindCanvasGroup)
            {
                currentBlindCanvasGroup.DOKill();

                yield return currentBlindCanvasGroup
                    .DOFade(0f, blindFadeDuration)
                    .WaitForCompletion();

                currentBlindCanvasGroup.interactable = false;
                currentBlindCanvasGroup.blocksRaycasts = false;
            }

            _blindIntroFinished = true;
            _blindFadeCoroutine = null;
        }

        private string GetLogSeatName(int seatIndex)
        {
            var seat = GameManager.Instance?.GetSeatController(seatIndex);
            if (!seat)
                return $"Seat {seatIndex}";

            return !string.IsNullOrWhiteSpace(seat.LogName)
                ? seat.LogName
                : seat.DisplayName;
        }
        
        private void RecordSystemAction(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;

            _secondLastOverallAction = _lastOverallAction;
            _lastOverallAction = line;
        }
        
        private CanvasGroup GetTradeLinkCanvasGroup()
        {
            if (!tradeLink)
                return null;

            if (!tradeLinkCanvasGroup)
            {
                tradeLinkCanvasGroup = tradeLink.GetComponent<CanvasGroup>();

                if (!tradeLinkCanvasGroup)
                    tradeLinkCanvasGroup = tradeLink.AddComponent<CanvasGroup>();
            }

            return tradeLinkCanvasGroup;
        }

        private void HideTradeLinkInstant()
        {
            CanvasGroup cg = GetTradeLinkCanvasGroup();
            if (!cg) return;

            cg.DOKill();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            tradeLink.SetActive(false);
        }

        private void FadeInTradeLink()
        {
            CanvasGroup cg = GetTradeLinkCanvasGroup();
            if (!cg) return;

            tradeLink.SetActive(true);

            cg.DOKill();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            cg.DOFade(1f, tradeLinkFadeDuration)
                .OnComplete(() =>
                {
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                });
        }

        private void FadeOutTradeLink()
        {
            CanvasGroup cg = GetTradeLinkCanvasGroup();
            if (!cg) return;

            cg.DOKill();

            cg.interactable = false;
            cg.blocksRaycasts = false;

            cg.DOFade(0f, tradeLinkFadeDuration)
                .OnComplete(() =>
                {
                    if (tradeLink)
                        tradeLink.SetActive(false);
                });
        }
        
        private void TryShowTradeLinkForPot(int pot)
        {
            if (_tradeLinkShownThisHand)
                return;

            if (pot <= tradeLinkPotThreshold)
                return;

            _tradeLinkShownThisHand = true;

            if (_blindFadeCoroutine != null)
            {
                StopCoroutine(_blindFadeCoroutine);
                _blindFadeCoroutine = null;
            }

            if (_tradeLinkThresholdCoroutine != null)
            {
                StopCoroutine(_tradeLinkThresholdCoroutine);
                _tradeLinkThresholdCoroutine = null;
            }

            _tradeLinkThresholdCoroutine = StartCoroutine(ShowTradeLinkBecausePotReachedThreshold());
        }
        private IEnumerator ShowTradeLinkBecausePotReachedThreshold()
        {
            if (currentBlindCanvasGroup)
            {
                currentBlindCanvasGroup.DOKill();

                if (currentBlindCanvasGroup.gameObject.activeSelf && currentBlindCanvasGroup.alpha > 0f)
                {
                    yield return currentBlindCanvasGroup
                        .DOFade(0f, blindFadeDuration)
                        .WaitForCompletion();
                }

                currentBlindCanvasGroup.interactable = false;
                currentBlindCanvasGroup.blocksRaycasts = false;
            }

            FadeInTradeLink();

            _tradeLinkThresholdCoroutine = null;
        }
        
        private int GetTotalChipsInGame(int pot)
        {
            if (_tableChipTotal > 0)
                return _tableChipTotal;

            int total = pot;

            List<SeatController> seats = GameManager.Instance
                ? GameManager.Instance.AllSeats
                : null;

            if (seats != null)
            {
                foreach (SeatController seat in seats)
                {
                    if (seat)
                        total += seat.Stack;
                }
            }

            return Mathf.Max(1, total);
        }
        
        public void RefreshPotStackVisual()
        {
            if (!potStackVisual)
                return;

            potStackVisual.UpdatePotVisual(_currentPot, GetTotalChipsInGame(_currentPot));
        }

        public void HidePotStackAnimated()
        {
            if (!potStackVisual)
                return;

            potStackVisual.HideAllAnimated();
        }

        public void HidePotStackInstant()
        {
            if (!potStackVisual)
                return;

            potStackVisual.HideAllInstant();
        }
        
        public void HidePotStackAnimatedOverDuration(float duration)
        {
            if (!potStackVisual)
                return;

            potStackVisual.HideAllAnimatedOverDuration(duration);
        }
        
        public int GetVisiblePotCoinCount()
        {
            if (!potStackVisual)
                return 0;

            return potStackVisual.VisibleCoinCount;
        }
        
        
        public void HideOnePotCoinAnimated()
        {
            if (potStackVisual == null)
                return;

            potStackVisual.HideOneTopCoinAnimated();
        }
        
        private void RefreshAllPlayerCoinVisuals()
        {
            if (!GameManager.Instance)
                return;

            List<SeatController> seats = GameManager.Instance.AllSeats;

            if (seats == null)
                return;

            foreach (SeatController seat in seats)
            {
                if (!seat)
                    continue;

                UIPlayer uiPlayer = seat.GetComponent<UIPlayer>();

                if (uiPlayer)
                    uiPlayer.RefreshCoinVisual();
            }
        }
        
        private void UpdateTableChipTotalIfNeeded()
        {
            if (!GameManager.Instance)
                return;

            int total = _currentPot;

            List<SeatController> seats = GameManager.Instance.AllSeats;

            if (seats != null)
            {
                foreach (SeatController seat in seats)
                {
                    if (seat)
                        total += seat.Stack;
                }
            }
            
            if (seats == null || seats.Count < 2)
                return;

            if (!seats[0] || !seats[1])
                return;

            if (seats[0].Stack <= 0 && seats[1].Stack <= 0)
                return;
            
            if (total > _tableChipTotal)
                _tableChipTotal = total;
        }
        
        private void StartFrameGlint()
        {
            if (!frameGlint)
                return;

            if (_frameGlintCoroutine != null)
            {
                StopCoroutine(_frameGlintCoroutine);
                _frameGlintCoroutine = null;
            }

            _frameGlintCoroutine = StartCoroutine(FrameGlintLoop());
        }

        private IEnumerator FrameGlintLoop()
        {
            frameGlint.anchoredPosition = frameGlintPointA;

            while (true)
            {
                yield return MoveFrameGlint(
                    frameGlint,
                    frameGlintPointA,
                    frameGlintPointB,
                    frameGlintDuration
                );

                if (frameGlintWaitAtEnd > 0f)
                    yield return new WaitForSecondsRealtime(frameGlintWaitAtEnd);

                // Instant reset back to A to start the cycle again.
                frameGlint.anchoredPosition = frameGlintPointA;

                yield return null;
            }
        }

        private static IEnumerator MoveFrameGlint(
            RectTransform glint,
            Vector2 from,
            Vector2 to,
            float duration)
        {
            if (!glint)
                yield break;

            if (duration <= 0f)
            {
                glint.anchoredPosition = to;
                yield break;
            }

            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(timer / duration);
                t = Mathf.SmoothStep(0f, 1f, t);

                glint.anchoredPosition = Vector2.LerpUnclamped(from, to, t);

                yield return null;
            }

            glint.anchoredPosition = to;
        }
        
        private void StartRotatingStar()
        {
            if (rotatingStar == null)
                return;

            if (_starRotationCoroutine != null)
            {
                StopCoroutine(_starRotationCoroutine);
                _starRotationCoroutine = null;
            }

            _starRotationCoroutine = StartCoroutine(RotateStarLoop());
        }

        private IEnumerator RotateStarLoop()
        {
            while (true)
            {
                if (rotatingStar)
                {
                    // Negative Z rotates clockwise in Unity UI.
                    rotatingStar.Rotate(0f, 0f, -starRotationSpeed * Time.unscaledDeltaTime);
                }

                yield return null;
            }
        }
    }
}