using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TexasHoldem
{
    public class RosterRow : MonoBehaviour
    {
        // ── Inspector refs — Active state ─────────────────────────────────────

        [Header("Active Row Content")]
        [SerializeField] private Image            portraitImage;
        [SerializeField] private TextMeshProUGUI  handleText;
        [SerializeField] private TextMeshProUGUI  stackText;

        [Header("Active Turn Content (shown only when this seat is acting)")]
        [SerializeField] private TextMeshProUGUI  handleTextActive;
        [SerializeField] private TextMeshProUGUI  stackTextActive;
        
        [Header("Normal Background")]
        [SerializeField] private GameObject backgroundRoot;
        
        [Header("Turn Emphasis Sorting")]
        [SerializeField] private Canvas rowCanvas;
        [SerializeField] private int normalSortingOrder = 0;
        [SerializeField] private int activeTurnSortingOrder = 50;
        
        [Header("Turn Emphasis Fade")]
        [SerializeField] private CanvasGroup rowCanvasGroup;
        [SerializeField] private float inactiveTurnAlpha = 0.60f;
        [SerializeField] private float activeTurnAlpha = 1f;
        [SerializeField] private float turnFadeDuration = 0.18f;
        
        [Header("Turn Emphasis Layout")]
        [SerializeField] private RectTransform rowContentRoot;
        [SerializeField] private Vector2 normalContentPosition;
        [SerializeField] private Vector2 activeTurnContentPosition;
        [SerializeField] private bool captureNormalContentPositionOnBind = true;
        [SerializeField] private float contentMoveDuration = 0.18f;
        
        [Header("Turn / Winner Emphasis Movement")]
        [SerializeField] private RectTransform leftInfoRoot;     // name + stack
        [SerializeField] private RectTransform marketRoot;       // market price/slippage

        [SerializeField] private Vector2 leftInfoEmphasisOffset = new Vector2(-20f, 0f);
        [SerializeField] private Vector2 marketEmphasisOffset = new Vector2(25f, 0f);

        [SerializeField] private float emphasisMoveDuration = 0.18f;

        [Header("Market Data — Active (3 texts, action seat only)")]
        [SerializeField] private TextMeshProUGUI  marketPriceText;           // "XX%"
        [SerializeField] private TextMeshProUGUI  marketBuyText;             // "$100 Buy"
        [SerializeField] private TextMeshProUGUI  marketSlippageText;        // "To Win +$X.XX" / "To Win -$X.XX"

        [Header("Market Data — Compact (2 texts, inactive seats)")]
        [SerializeField] private TextMeshProUGUI  marketCompactPriceText;    // "XX%"
        [SerializeField] private TextMeshProUGUI  marketCompactSlippageText; // "+$X.XX" / "-$X.XX"

        [Header("Folded State")]
        [SerializeField] private GameObject       foldedTint;         // red overlay, active when folded
        [SerializeField] private float            foldedTextDuration = 6f; // seconds before reverting to stack display

        [Header("Eliminated State")]
        [SerializeField] private GameObject       activeRowContent;   // hide when eliminated
        [SerializeField] private GameObject       eliminatedPanel;    // show when eliminated
        [SerializeField] private TextMeshProUGUI  eliminatedHandleText;
        [SerializeField] private TextMeshProUGUI  eliminatedChipText; // always "Out"

        [Header("Winner Highlight")]
        [SerializeField] private GameObject       winnerHighlightRoot;
        
        [Header("Text Colors")]
        [SerializeField] private Color activeTextColor = Color.white;
        [SerializeField] private Color inactiveTextColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        [SerializeField] private Color foldedTextColor = new Color(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private Color eliminatedTextColor = new Color(0.45f, 0.45f, 0.45f, 1f);

        [Header("Animation")]
        [SerializeField] private float            rowReorderDuration = 0.35f;

        // ── Runtime ───────────────────────────────────────────────────────────

        private SeatController _seat;
        private int            _seatIndex  = -1;
        private bool _isFolded     = false;
        private bool _isWinner     = false;
        private bool  _isActionSeat  = false;
        private float _lastPrice    = float.NaN;
        private float _lastSlippage = float.NaN;
        private bool _isEliminated = false;
        private int _currentStack = 0;
        private int _currentBet = 0;
        private int  _stackAtHandStart         = 0;    // pre-blind stack this hand (for fold-loss display)
        private bool _pendingHandStartSnapshot = false; // true until first post-NewHand stack sync
        private Vector2 _leftInfoNormalPos;
        private Vector2 _marketNormalPos;

        private bool _movementPositionsCaptured = false;
        private Coroutine _foldedTextRevertCoroutine = null;
        private bool      _foldedTextReverted        = false; // true once the fold text has silently reverted to stack display

        // ── Bind ──────────────────────────────────────────────────────────────
        
        /// Called by RosterPanel at match start.
        /// Binds this row to its SeatController and subscribes to all per-seat events.
        public void Bind(SeatController seat)
        {
            _seat      = seat;
            _seatIndex = seat.seatIndex;

            CaptureMovementPositions();
            
            seat.OnInitialized             += HandleInitialized;
            seat.OnStackChanged            += HandleStackChanged;
            seat.OnBetChanged              += HandleBetChanged;
            seat.OnStatusChanged           += HandleStatusChanged;
            seat.OnWinner                  += HandleWinner;
            seat.OnWonChips                += HandleWonChips;
            seat.OnNewHand                 += HandleNewHand;
            seat.OnClearActionLabel        += HandleClearActionLabel;
            seat.OnEliminated              += HandleEliminated;

            PokerTableEvents.OnActionSeatChanged   += HandleActionSeatChanged;

            // SeatController.Initialize() fires OnInitialized BEFORE RosterPanel.Bind()
            // is called — so we missed the event. Apply current seat data immediately.
            if (!string.IsNullOrEmpty(seat.DisplayName) || seat.AvatarSprite != null)
                HandleInitialized(seat.AvatarSprite, seat.DisplayName);

            if (seat.Stack > 0)
                HandleStackChanged(seat.Stack);

            ResetRow();
        }

        private void OnDestroy()
        {
            if (_seat != null)
            {
                _seat.OnInitialized             -= HandleInitialized;
                _seat.OnStackChanged            -= HandleStackChanged;
                _seat.OnBetChanged              -= HandleBetChanged;
                _seat.OnStatusChanged           -= HandleStatusChanged;
                _seat.OnWinner                  -= HandleWinner;
                _seat.OnWonChips                -= HandleWonChips;
                _seat.OnNewHand                 -= HandleNewHand;
                _seat.OnClearActionLabel        -= HandleClearActionLabel;
                _seat.OnEliminated              -= HandleEliminated;
            }

            PokerTableEvents.OnActionSeatChanged   -= HandleActionSeatChanged;
        }

        // ── Public API ────────────────────────────────────────────────────────

        public int SeatIndex => _seatIndex;


        /// Called by ActionManager when market data arrives for this seat.
        public void SetMarketData(float price, float slippage)
        {
            _lastPrice    = price;
            _lastSlippage = slippage;

            // Active set (3 texts)
            if (marketPriceText) marketPriceText.SetText(float.IsNaN(price) ? "--" : $"{price}%");
            if (marketBuyText) marketBuyText.SetText("$100 Buy");
            if (marketSlippageText) marketSlippageText.SetText(FormatSlippageFull(slippage));

            // Compact set (2 texts)
            if (marketCompactPriceText) marketCompactPriceText.SetText(float.IsNaN(price) ? "--" : $"{price}%");
            if (marketCompactSlippageText) marketCompactSlippageText.SetText(FormatSlippageCompact(slippage));

            ApplyMarketVisibility();
        }

        /// <summary>
        /// Clears market data fields to "--". Called when this seat is eliminated
        /// or when market data polling stops.
        /// </summary>
        public void ClearMarketData()
        {
            _lastPrice    = float.NaN;
            _lastSlippage = float.NaN;
            if (marketPriceText) marketPriceText.SetText("--");
            if (marketBuyText) marketBuyText.SetText("$100 Buy");
            if (marketSlippageText) marketSlippageText.SetText("--");
            if (marketCompactPriceText) marketCompactPriceText.SetText("--");
            if (marketCompactSlippageText) marketCompactSlippageText.SetText("--");
            ApplyMarketVisibility();
        }
        
        /// Called by RosterPanel to animate this row to a new vertical position.
        /// Used for reordering after elimination.
        public void AnimateToPosition(Vector2 targetAnchoredPosition)
        {
            RectTransform rt = GetComponent<RectTransform>();
            if (!rt) return;

            rt.DOKill();
            rt.DOAnchorPos(targetAnchoredPosition, rowReorderDuration)
              .SetEase(Ease.OutQuad);
        }

        // ── Seat event handlers ───────────────────────────────────────────────

        private void HandleInitialized(Sprite portrait, string displayName)
        {
            if (handleText) handleText.SetText(displayName);
            if (handleTextActive) handleTextActive.SetText(displayName);

            if (eliminatedHandleText)
                eliminatedHandleText.SetText(displayName);
        }

        private void HandleStackChanged(int newStack)
        {
            _currentStack = newStack;

            // Phase-0 clean-slate sync fires the first OnStackChanged after OnNewHand.
            // At that point stacks are pre-blind, which is the correct hand-start baseline.
            if (_pendingHandStartSnapshot)
            {
                _stackAtHandStart         = newStack;
                _pendingHandStartSnapshot = false;
            }

            UpdateBetStackText();
        }

        private void HandleActionSeatChanged(int? actionSeatIndex)
        {
            if (_isEliminated || _isFolded)
            {
                _isActionSeat = false;
                ApplyRowVisualState();
                return;
            }

            _isActionSeat = actionSeatIndex.HasValue && actionSeatIndex.Value == _seatIndex;
            ApplyRowVisualState();
        }
        
        private void HandleBetChanged(int newBet)
        {
            _currentBet = newBet;
            
            UpdateBetStackText();
        }
        
        private void UpdateBetStackText()
        {
            string text;

            // If folded but the timed revert has already fired, keep showing the stack —
            // don't let subsequent street events (stack/bet changes) flip it back to "Folded".
            if (_isFolded && !_foldedTextReverted)
            {
                int lost = _stackAtHandStart - _currentStack;
                text = lost > 0 ? $"Folded - Lost {lost:N0}" : "Folded";
            }
            else
            {
                text = $"{_currentStack:N0}";
            }

            if (stackText       != null) stackText.SetText(text);
            if (stackTextActive != null) stackTextActive.SetText(text);
        }

        private void HandleStatusChanged(string status)
        {
            switch (status)
            {
                case "folded":
                    _isFolded = true;
                    _foldedTextReverted = false;
                    UpdateBetStackText();   // immediately show "Folded - Lost X"

                    // After foldedTextDuration seconds, revert stackText to chip count.
                    // _isFolded stays true — only the text reverts, not the tint or colors.
                    if (_foldedTextRevertCoroutine != null)
                        StopCoroutine(_foldedTextRevertCoroutine);
                    _foldedTextRevertCoroutine = StartCoroutine(RevertFoldedTextAfterDelay());
                    break;

                case "active":
                case "all_in":
                    _isFolded = false;
                    break;

                case "eliminated":
                    // Let HandleEliminated handle the eliminated panel.
                    break;
            }

            ApplyRowVisualState();
        }
        
        private void ApplyRowVisualState()
        {
            bool showFoldedTint = !_isEliminated && _isFolded;

            // WinnerHighlightRoot is used for BOTH:
            // 1. winner state
            // 2. current player turn state
            //
            // But folded always wins over both.
            bool showHighlight = !_isEliminated
                                 && !showFoldedTint
                                 && (_isWinner || _isActionSeat);

            // Normal background shows only when the row is active,
            // not folded, not winner-highlighted, and not current-turn highlighted.
            bool showBackground = !_isEliminated
                                  && !showFoldedTint
                                  && !showHighlight;

            if (foldedTint)
                foldedTint.SetActive(showFoldedTint);

            if (winnerHighlightRoot)
                winnerHighlightRoot.SetActive(showHighlight);

            if (backgroundRoot)
                backgroundRoot.SetActive(showBackground);

            // Keep your original behavior: hide portrait only when this row is a winner,
            // not merely because it is the current turn.
            if (portraitImage)
                portraitImage.enabled = !_isWinner;
            
            bool isRowEmphasized = !_isEliminated
                                   && !_isFolded
                                   && (_isActionSeat || _isWinner);

            SetTurnSorting(isRowEmphasized);
            SetTurnFade(isRowEmphasized);
            SetEmphasisMovement(isRowEmphasized);
            SetTextColors(isRowEmphasized);
            ApplyMarketVisibility();
            ApplyActiveTMPSwap();
        }
        

        private void HandleWinner()
        {
            _isWinner = true;
            ApplyRowVisualState();
        }

        private void HandleWonChips(int amount)
        {
            // Stack update comes via HandleStackChanged — nothing extra needed here.
            // Winner panel is already shown via HandleWinner.
        }

        private void HandleNewHand()
        {
            _pendingHandStartSnapshot = true;

            // Cancel any pending fold-text revert — new hand resets everything.
            if (_foldedTextRevertCoroutine != null)
            {
                StopCoroutine(_foldedTextRevertCoroutine);
                _foldedTextRevertCoroutine = null;
            }
            _foldedTextReverted = false;

            ClearTurnWinnerFoldVisualsForNewHand();

            // Market data NOT cleared — persists between hands.

            _currentBet = 0;
            UpdateBetStackText();
        }

        private void HandleClearActionLabel()
        {
            // Fold tint stays until next hand — only cleared in HandleNewHand.
            // Bet amount fades out during staged cleanup.
        }

        private void HandleEliminated()
        {
            ClearMarketData();
            SetTurnSorting(false);
            ApplyRowVisualState();
            if (activeRowContent) activeRowContent.SetActive(false);
            if (eliminatedPanel) eliminatedPanel.SetActive(true);
            if (eliminatedChipText) eliminatedChipText.SetText("Out");
        }
        
        private static string FormatSlippageFull(float s)
        {
            if (float.IsNaN(s)) return "--";
            string sign = s >= 0f ? "+" : "-";
            return $"To Win {sign}${Mathf.Abs(s)}";
        }

        private static string FormatSlippageCompact(float s)
        {
            if (float.IsNaN(s)) return "--";
            string sign = s >= 0f ? "+" : "-";
            return $"{sign}${Mathf.Abs(s)}";
        }

        private void ApplyMarketVisibility()
        {
            bool active = _isActionSeat && !_isFolded && !_isEliminated;

            // Only toggle between active/compact if BOTH compact fields are wired up.
            // If compact fields are missing in the Inspector, always show the active set
            // so market data is never accidentally hidden.
            bool compactAvailable = marketCompactPriceText && marketCompactSlippageText;

            if (compactAvailable)
            {
                // Active set — 3 texts, shown only on the action seat
                if (marketPriceText) marketPriceText.gameObject.SetActive(active);
                if (marketBuyText) marketBuyText.gameObject.SetActive(active);
                if (marketSlippageText) marketSlippageText.gameObject.SetActive(active);

                // Compact set — 2 texts, shown on every non-action seat
                // Slippage is hidden for folded players — no point betting on someone who's out.
                marketCompactPriceText.gameObject.SetActive(!active);
                marketCompactSlippageText.gameObject.SetActive(!active && !_isFolded && !_isEliminated);
            }
            else
            {
                // Compact fields not wired — always show active set so data is visible
                if (marketPriceText) marketPriceText.gameObject.SetActive(true);
                if (marketBuyText) marketBuyText.gameObject.SetActive(true);
                if (marketSlippageText) marketSlippageText.gameObject.SetActive(true);
            }
        }

        private void ClearTurnWinnerFoldVisualsForNewHand()
        {
            _isFolded     = false;
            _isWinner     = false;
            _isActionSeat = false;
            _isEliminated = false;

            if (foldedTint)
                foldedTint.SetActive(false);

            if (winnerHighlightRoot)
                winnerHighlightRoot.SetActive(false);

            if (backgroundRoot)
                backgroundRoot.SetActive(true);

            if (portraitImage)
                portraitImage.enabled = true;
            
            SetTurnSorting(false);
            SetTurnFade(false, true);
            SetTurnContentPosition(false, true);
            SetTextColors(false);
        }
        
        private void SetTurnSorting(bool isPlayersTurn)
        {
            if (!rowCanvas)
                rowCanvas = GetComponent<Canvas>();

            if (!rowCanvas)
                return;

            // Important for nested UI canvases.
            rowCanvas.overrideSorting = true;

            rowCanvas.sortingOrder = isPlayersTurn
                ? activeTurnSortingOrder
                : normalSortingOrder;
        }
        
        private void SetTurnFade(bool isPlayersTurn, bool instant = false)
        {
            if (!rowCanvasGroup)
                rowCanvasGroup = GetComponent<CanvasGroup>();

            if (!rowCanvasGroup)
                return;

            float targetAlpha = isPlayersTurn ? activeTurnAlpha : inactiveTurnAlpha;

            rowCanvasGroup.DOKill();

            if (instant)
            {
                rowCanvasGroup.alpha = targetAlpha;
                return;
            }

            rowCanvasGroup
                .DOFade(targetAlpha, turnFadeDuration)
                .SetEase(Ease.OutQuad);
        }
        
        private void SetTurnContentPosition(bool isPlayersTurn, bool instant = false)
        {
            if (!rowContentRoot)
                return;

            Vector2 targetPosition = isPlayersTurn
                ? activeTurnContentPosition
                : normalContentPosition;

            rowContentRoot.DOKill();

            if (instant)
            {
                rowContentRoot.anchoredPosition = targetPosition;
                return;
            }

            rowContentRoot
                .DOAnchorPos(targetPosition, contentMoveDuration)
                .SetEase(Ease.OutQuad);
        }
        
        private void CaptureMovementPositions()
        {
            if (_movementPositionsCaptured)
                return;

            if (leftInfoRoot)
                _leftInfoNormalPos = leftInfoRoot.anchoredPosition;

            if (marketRoot)
                _marketNormalPos = marketRoot.anchoredPosition;

            _movementPositionsCaptured = true;
        }

        private void MoveRect(RectTransform target, Vector2 normalPos, Vector2 offset, bool emphasized, bool instant)
        {
            if (!target)
                return;

            Vector2 targetPos = emphasized
                ? normalPos + offset
                : normalPos;

            target.DOKill();

            if (instant)
            {
                target.anchoredPosition = targetPos;
                return;
            }

            target
                .DOAnchorPos(targetPos, emphasisMoveDuration)
                .SetEase(Ease.OutQuad);
        }

        private void SetEmphasisMovement(bool emphasized, bool instant = false)
        {
            CaptureMovementPositions();

            MoveRect(leftInfoRoot, _leftInfoNormalPos, leftInfoEmphasisOffset, emphasized, instant);
            MoveRect(marketRoot,   _marketNormalPos,   marketEmphasisOffset,   emphasized, instant);
        }

        // ── Active TMP swap ───────────────────────────────────────────────────
        
        /// Shows handleTextActive + stackTextActive when this seat is the action seat.
        /// Shows handleText + stackText at all other times.
        /// Both pairs always carry the same text content — only visibility differs.
        private void ApplyActiveTMPSwap()
        {
            bool showActive = _isActionSeat && !_isFolded && !_isEliminated;

            if (handleText) handleText.gameObject.SetActive(!showActive);
            if (stackText) stackText.gameObject.SetActive(!showActive);
            if (handleTextActive) handleTextActive.gameObject.SetActive(showActive);
            if (stackTextActive) stackTextActive.gameObject.SetActive(showActive);
        }

        // ── Fold text revert ──────────────────────────────────────────────────

        private System.Collections.IEnumerator RevertFoldedTextAfterDelay()
        {
            yield return new WaitForSeconds(foldedTextDuration);

            // Mark as reverted so UpdateBetStackText won't re-show the fold message.
            _foldedTextReverted = true;

            // Only revert the text — _isFolded stays true so tint/colors are unaffected.
            string stackStr = $"{_currentStack:N0}";
            if (stackText) stackText.SetText(stackStr);
            if (stackTextActive) stackTextActive.SetText(stackStr);

            _foldedTextRevertCoroutine = null;
        }

        // ── Reset ─────────────────────────────────────────────────

        private void ResetRow()
        {
            if (eliminatedPanel) eliminatedPanel.SetActive(false);
            if (activeRowContent) activeRowContent.SetActive(true);

            ClearTurnWinnerFoldVisualsForNewHand();

            ClearMarketData();
        }
        
        private void SetTextColors(bool isRowEmphasized)
        {
            Color targetColor;

            if (_isEliminated)
                targetColor = eliminatedTextColor;
            else if (_isFolded)
                targetColor = foldedTextColor;
            else
                targetColor = isRowEmphasized ? activeTextColor : inactiveTextColor;

            if (handleText) handleText.color       = targetColor;
            if (stackText) stackText.color        = targetColor;
            if (handleTextActive) handleTextActive.color = targetColor;
            if (stackTextActive) stackTextActive.color  = targetColor;

            if (marketPriceText)
                marketPriceText.color = targetColor;

            if (marketBuyText)
                marketBuyText.color = targetColor;

            if (marketSlippageText)
                marketSlippageText.color = targetColor;

            if (marketCompactPriceText)
                marketCompactPriceText.color = targetColor;

            if (marketCompactSlippageText)
                marketCompactSlippageText.color = targetColor;

            if (eliminatedHandleText)
                eliminatedHandleText.color = eliminatedTextColor;

            if (eliminatedChipText)
                eliminatedChipText.color = eliminatedTextColor;
        }
        
        
    }
}