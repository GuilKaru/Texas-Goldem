using System;
using SimplePoker.Audio;
using SimplePoker.Data;
using SimplePoker.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TexasHoldem
{
    public class UIPlayer : MonoBehaviour
    {
        // ── Inspector refs ────────────────────────────────────────────────────

        [Header("Core References")]
        [SerializeField] private SeatController   seatController;
        [SerializeField] private VisualController visualController;

        [Header("Portrait")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private RectTransform portraitRectTransform;
        [SerializeField] private RectTransform emotionRingRectTransform;

        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI  nameText;
        [SerializeField] private TextMeshProUGUI  nameTextHighlighted;
        [SerializeField] private TextMeshProUGUI  betText;
        [SerializeField] private TextMeshProUGUI  actionLabelText;
        [SerializeField] private TextMeshProUGUI  actionLabelHighlightedText;

        [Header("Cards")]
        [SerializeField] private GameObject       cardContainer;
        [SerializeField] private GameObject       cardPrefab;
        [SerializeField] private DeckData         deckData;
        [Tooltip("World-space Transform to deal cards FROM. Falls back to UIManager.DealOrigin.")]
        [SerializeField] private Transform        dealOriginOverride;
        [SerializeField] private float            dealStagger = 0.12f;
        [SerializeField] private float            flipDelay   = 0.05f;

        [SerializeField] private float thinkingDotInterval = 0.22f;
        
        [Header("Active Turn UI")]
        [SerializeField] private GameObject thinkingBox;
        [SerializeField] private GameObject activeNameImage;
        [SerializeField] private float activeNameImageShowDelay = 0.40f;

        [Header("Active Turn Scale Animation")]
        [SerializeField] private float activeTurnScale = 1.15f;
        [SerializeField] private float thinkingBoxActiveScale = 1.12f;
        [SerializeField] private float activeTurnScaleDuration = 0.22f;
        [SerializeField] private Ease activeTurnScaleEase = Ease.OutBack;
        [SerializeField] private Ease inactiveTurnScaleEase = Ease.OutQuad;

        [Header("Active Turn Position Animation")]
        [SerializeField] private Vector2 activeTurnPortraitOffset = new Vector2(-18f, 0f);
        [SerializeField] private Vector2 activeTurnEmotionRingOffset = new Vector2(-18f, 0f);
        
        [Header("Badges")]
        [SerializeField] private GameObject       dealerBadge;
        [SerializeField] private GameObject       smallBlindBadge;
        [SerializeField] private GameObject       bigBlindBadge;

        [Header("Chip Stack Visual (manual order: TOP -> BOTTOM)")]
        [SerializeField] private List<GameObject> coinsTopToBottom = new List<GameObject>(20);
        [SerializeField] private bool             useFloor = true;
        [SerializeField] private float coinGainRevealDelay = 0.045f;
        [SerializeField] private bool animateCoinGains = true;
        
        [Header("Chip Animation Targets")]
        [SerializeField] private RectTransform betBoxRectTransform;
        [SerializeField] private Vector2 betBoxChipLandingOffset = new Vector2(-30f, 0f);

        [Header("Status Overlays")]
        [SerializeField] private GameObject       foldedLabel;
        [SerializeField] private GameObject       allInLabel;
        [SerializeField] private GameObject       eliminatedLabel;

        [Header("Win Percentage")]
        [SerializeField] private GameObject winChanceRoot;
        [SerializeField] private CanvasGroup winChanceCanvasGroup;
        [SerializeField] private TextMeshProUGUI winChanceText;
        [SerializeField] private float winChanceFadeDuration = 0.4f;

        private bool _winChanceVisible = false;
        
        [Header("Fold / Eliminated Panels")]
        [SerializeField] private GameObject foldPanel;
        [SerializeField] private CanvasGroup activePanelCanvasGroup;
        [SerializeField] private CanvasGroup eliminatedCanvasGroup;
        [SerializeField] private TextMeshProUGUI eliminatedNameText;

        [Header("Winner")]
        [SerializeField] private GameObject       winnerPanel;
        [SerializeField] private TextMeshProUGUI  winnerHandText;

        [Header("Action Colors")]
        [SerializeField] private Color colorFold    = Color.gray;
        [SerializeField] private Color colorCheck   = Color.white;
        [SerializeField] private Color colorCall    = new Color(0.3f, 0.8f, 1f);
        [SerializeField] private Color colorRaise   = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color colorAllIn   = new Color(1f, 0.6f, 0.1f);
        [SerializeField] private Color colorBlind   = new Color(0.9f, 0.9f, 0.3f);

        // ── Private ───────────────────────────────────────────────────────────

        private PokerGameAssetData _asset;
        private SoundManager       _sound;
        private Coroutine          _banterCoroutine;
        private Coroutine _thinkingCoroutine;
        private Coroutine _coinVisualCoroutine;

        // ── Card state ────────────────────────────────────────────────────────
        private readonly List<GameObject>             _cardObjects     = new List<GameObject>(2);
        private readonly List<CardVisualController>   _cardControllers = new List<CardVisualController>(2);
        private readonly List<SimplePoker.Logic.Card> _cards           = new List<SimplePoker.Logic.Card>(2);
        private int _holeCard0 = -1;
        private int _holeCard1 = -1;
        private int _stackAtHandStart = -1;   // pre-blind stack this hand — mirrors RosterRow's fold-loss baseline
        private bool _pendingHandStartSnapshot = false; // true until the first post-NewHand stack sync
        private int _foldLostAmount = 0;  // total chips lost this whole hand, computed at fold time
        private int _lastVisibleCoinCount = -1;
        private Vector2 _portraitDefaultSize;
        private Vector2 _emotionRingDefaultSize;
        private bool _hasCachedBanterEmphasisSizes = false;
        private Vector2 _portraitMoveRootDefaultAnchoredPosition;
        private bool _isMyTurn;

        private Vector3 _thinkingBoxOriginalScale;
        private Vector3 _portraitOriginalScale;
        private Vector3 _emotionRingOriginalScale;

        private Vector2 _portraitOriginalAnchoredPosition;
        private Vector2 _emotionRingOriginalAnchoredPosition;

        private bool _cachedActiveTurnOriginals;
        private Tween _activeNameImageDelayTween;
        private bool _activeNameImageDelayRunning;
        
        private PokerGameAssetData Asset
        {
            get
            {
                if (_asset == null && PokerGameAsset.Instance != null)
                    _asset = PokerGameAsset.Instance.PokerGameAssetData;
                return _asset;
            }
        }

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (seatController == null)
                seatController = GetComponent<SeatController>();

            if (visualController == null)
                visualController = GetComponent<VisualController>();

            CacheActiveTurnOriginals();
            ForceHideActiveTurnUI();

            HideCoinsInstant();

            SubscribeToSeatEvents();
            ResetUI();
            
            if (nameTextHighlighted != null)
                nameTextHighlighted.gameObject.SetActive(false);

            PokerTableEvents.OnActionSeatChanged   += HandleActionSeatChanged;
            PokerTableEvents.OnSeatThinkingChanged += HandleSeatThinkingChanged;
            PokerTableEvents.OnFadeOutTableNodes   += HandleFadeOutTableNodes;
            PokerTableEvents.OnFadeInTableNodes    += HandleFadeInTableNodes;
            PokerTableEvents.OnClearHoleCards      += HandleClearHoleCards;
            PokerTableEvents.OnWinningHandRevealed += HandleWinningHandRevealed;
            PokerTableEvents.OnStreetChanged       += HandleStreetChanged;
        }

        private void Start()
        {
            _sound = SoundManager.Instance;
        }

        private void OnDestroy()
        {
            UnsubscribeFromSeatEvents();
            PokerTableEvents.OnActionSeatChanged   -= HandleActionSeatChanged;
            PokerTableEvents.OnSeatThinkingChanged -= HandleSeatThinkingChanged;
            PokerTableEvents.OnFadeOutTableNodes   -= HandleFadeOutTableNodes;
            PokerTableEvents.OnFadeInTableNodes    -= HandleFadeInTableNodes;
            PokerTableEvents.OnClearHoleCards      -= HandleClearHoleCards;
            PokerTableEvents.OnWinningHandRevealed -= HandleWinningHandRevealed;
            PokerTableEvents.OnStreetChanged       -= HandleStreetChanged;
            StopThinkingLabel(true);
            _activeNameImageDelayTween?.Kill();
            _activeNameImageDelayTween = null;
            if (_coinVisualCoroutine != null)
            {
                StopCoroutine(_coinVisualCoroutine);
                _coinVisualCoroutine = null;
            }
        }

        // ── Subscriptions ─────────────────────────────────────────────────────

        private void SubscribeToSeatEvents()
        {
            if (seatController == null) return;

            seatController.OnInitialized     += HandleInitialized;
            seatController.OnStackChanged    += HandleStackChanged;
            seatController.OnBetChanged      += HandleBetChanged;
            seatController.OnStatusChanged   += HandleStatusChanged;
            seatController.OnActionPerformed += HandleActionPerformed;
            seatController.OnBadgeChanged    += HandleBadgeChanged;
            seatController.OnWinner          += HandleWinner;
            seatController.OnWonChips        += HandleWonChips;     // Item 1
            seatController.OnLostChips       += HandleLostChips;    // Item 1
            seatController.OnNewHand         += HandleNewHand;
            seatController.OnWinChanceChanged+= HandleWinChanceChanged;
            seatController.OnClearActionLabel+= HandleClearActionLabel; // Item 3
            seatController.OnClearCurrentActionLabel += HandleClearCurrentActionLabel;
            seatController.OnEliminated              += HandleEliminated;
        }

        private void UnsubscribeFromSeatEvents()
        {
            if (seatController == null) return;

            seatController.OnInitialized     -= HandleInitialized;
            seatController.OnStackChanged    -= HandleStackChanged;
            seatController.OnBetChanged      -= HandleBetChanged;
            seatController.OnStatusChanged   -= HandleStatusChanged;
            seatController.OnActionPerformed -= HandleActionPerformed;
            seatController.OnBadgeChanged    -= HandleBadgeChanged;
            seatController.OnWinner          -= HandleWinner;
            seatController.OnWonChips        -= HandleWonChips;
            seatController.OnLostChips       -= HandleLostChips;
            seatController.OnNewHand         -= HandleNewHand;
            seatController.OnWinChanceChanged-= HandleWinChanceChanged;
            seatController.OnClearActionLabel-= HandleClearActionLabel;
            seatController.OnClearCurrentActionLabel -= HandleClearCurrentActionLabel;
            seatController.OnEliminated              -= HandleEliminated;
        }

        // ── Event Handlers ────────────────────────────────────────────────────

        private void HandleInitialized(Sprite portrait, string displayName)
        {
            if (portraitImage != null && portrait != null)
                portraitImage.sprite = portrait;

            if (nameText != null)
                nameText.SetText(displayName);
            if (nameTextHighlighted != null)
                nameTextHighlighted.SetText(displayName);
        }

        private void HandleStackChanged(int newStack)
        {
            // Phase-0 clean-slate sync fires the first OnStackChanged after
            // OnNewHand. At that point stacks are pre-blind — the correct
            // hand-start baseline for "Folded - Lost X" (mirrors RosterRow's
            // _stackAtHandStart logic, so both show the same total hand loss).
            if (_pendingHandStartSnapshot)
            {
                _stackAtHandStart = newStack;
                _pendingHandStartSnapshot = false;
            }

            UpdateCoinVisual(newStack);
        }

        private void HandleBetChanged(int newBet)
        {
            if (betText == null) return;

            if (newBet > 0)
            {
                betText.gameObject.SetActive(true);
                betText.SetText($"{newBet}");
            }
            else
            {
                betText.gameObject.SetActive(false);
            }
        }

        private void HandleStatusChanged(string newStatus)
        {
            if (foldedOverlayActive(newStatus))    return;
            if (allInOverlayActive(newStatus))     return;
            if (eliminatedOverlayActive(newStatus)) return;

            if (foldedLabel    != null) foldedLabel.SetActive(false);
            if (allInLabel     != null) allInLabel.SetActive(false);
            if (eliminatedLabel!= null) eliminatedLabel.SetActive(false);
        }

        private void HandleActionPerformed(string action, int amount)
        {
            if (string.Equals(action, "thinking", StringComparison.OrdinalIgnoreCase))
            {
                StartThinkingLabel();
                visualController?.PunchActionLabel();
                return;
            }
            
            if (string.Equals(action, "fold", StringComparison.OrdinalIgnoreCase))
            {
                // Whole-hand loss = pre-blind starting stack minus stack right
                // now. The backend only tells us "fold", not the amount, so
                // this is computed locally — same formula RosterRow uses, so
                // both labels agree.
                if (seatController != null && _stackAtHandStart >= 0)
                    _foldLostAmount = Mathf.Max(0, _stackAtHandStart - seatController.Stack);

                // Turn the thinking UI off immediately.
                StopThinkingLabel(true);

                if (thinkingBox != null)
                    thinkingBox.SetActive(false);

                ShowFoldPanel();
            } 
            else
            {
                StopThinkingLabel(false);
            }

            StopThinkingLabel(false);
            ShowActionLabel(action, amount);
            visualController?.PunchActionLabel();
            PlayActionSound(action);
        }

        private void HandleActionSeatChanged(int? activeSeat)
        {
            if (seatController == null)
                return;

            if (seatController.IsEliminated)
            {
                _isMyTurn = false;

                StopThinkingLabel(true);
                ResetActiveTurnAnimation();
                visualController?.SetActiveTurn(false);

                if (nameTextHighlighted != null)
                    nameTextHighlighted.gameObject.SetActive(false);

                return;
            }

            _isMyTurn =
                activeSeat.HasValue &&
                activeSeat.Value == seatController.seatIndex;

            visualController?.SetActiveTurn(_isMyTurn);

            if (_isMyTurn)
            {
                // Mostrar el nombre del jugador activo,
                // también durante small blind y big blind.
                if (nameTextHighlighted != null)
                    nameTextHighlighted.gameObject.SetActive(true);

                ShowActiveTurnUI();
                PlayActiveTurnAnimation();
            }
            else
            {
                StopThinkingLabel(true);
                ResetActiveTurnAnimation();

                if (actionLabelHighlightedText != null)
                    actionLabelHighlightedText.gameObject.SetActive(false);

                // Ocultar el nombre de todos los jugadores no activos.
                if (nameTextHighlighted != null)
                    nameTextHighlighted.gameObject.SetActive(true);
            }
        }
        
        /// Fires only when Unity is speculatively guessing this seat will act next,
        /// ahead of real backend data (see PokerTableEvents.OnSeatThinkingChanged).
        /// This is the ONLY place the "Thinking..." animation is started — it must
        /// never start just because HandleActionSeatChanged marks a seat active
        /// (that happens for blinds posting, resolved-action re-highlights, etc.,
        /// none of which are "thinking").
        private void HandleSeatThinkingChanged(int? seatIndex)
        {
            bool isThisSeatThinking =
                seatController != null &&
                seatIndex.HasValue &&
                seatIndex.Value == seatController.seatIndex;

            if (isThisSeatThinking)
                StartThinkingLabel();
            else
                StopThinkingLabel(false);
        }

        private void HandleEliminated()
        {
            StopThinkingLabel(true);

            if (thinkingBox != null)
                thinkingBox.SetActive(false);

            if (foldPanel != null)
                foldPanel.SetActive(false);

            if (eliminatedNameText != null && seatController != null)
                eliminatedNameText.SetText(seatController.DisplayName);

            SetCanvasGroup(activePanelCanvasGroup, false);
            SetCanvasGroup(eliminatedCanvasGroup, true);

            if (eliminatedLabel != null)
                eliminatedLabel.SetActive(false);

            if (foldedLabel != null)
                foldedLabel.SetActive(false);

            if (allInLabel != null)
                allInLabel.SetActive(false);
        }

        private void HandleBadgeChanged(bool isDealer, bool isSB, bool isBB)
        {
            if (dealerBadge != null)
                dealerBadge.SetActive(isSB);

            if (smallBlindBadge != null)
                smallBlindBadge.SetActive(false);

            if (bigBlindBadge != null)
                bigBlindBadge.SetActive(false);
        }

        private void HandleWinner()
        {
            StopThinkingLabel(true);
            
            if (winnerPanel != null)
                winnerPanel.SetActive(true);

            _sound?.PlayOneShotSong(Asset?.Audio_PlayerWon);
        }

        // ── Item 1: Won / Lost label ──────────────────────────────────────────
        
        /// Shown immediately when HandResult arrives — replaces the last action label.
        /// Displays "Won X,XXX" on the winning seat.
        private void HandleWonChips(int pot)
        {
            StopThinkingLabel(true);
            
            ShowResultLabel($"Won {pot:N0}");
            visualController?.PunchActionLabel();
        }
        
        /// Shown immediately when HandResult arrives — replaces the last action label.
        /// Displays "Lost X,XXX" on the losing seat.
        private void HandleLostChips(int pot)
        {
            StopThinkingLabel(true);
            ShowResultLabel($"Lost\n{pot:N0}");
        }

        private void ShowResultLabel(string text)
        {
            StopThinkingLabel(true);

            if (actionLabelText != null)
            {
                actionLabelText.SetText(text);
                actionLabelText.gameObject.SetActive(!_isMyTurn);
            }

            if (actionLabelHighlightedText != null)
            {
                actionLabelHighlightedText.SetText(text);
                actionLabelHighlightedText.gameObject.SetActive(_isMyTurn);
            }
        }

        private void HandleNewHand()
        {
            StopThinkingLabel(true);

            if (_coinVisualCoroutine != null)
            {
                StopCoroutine(_coinVisualCoroutine);
                _coinVisualCoroutine = null;
            }

            _lastVisibleCoinCount = -1;
            _stackAtHandStart     = -1;
            _pendingHandStartSnapshot = true;
            _foldLostAmount       = 0;
            
            HideCoinsInstant();
            ClearCards();
            ResetUI();
        }

        private void HandleStreetChanged(int street)
        {
            // Clear "Folded - Lost X" from the action label when community cards arrive.
            // Only clears if the seat is actually folded — doesn't touch other seats.
            if (street <= 0) return;
            if (seatController == null || !seatController.IsFolded) return;

            if (actionLabelText != null)
            {
                actionLabelText.SetText("");
                actionLabelText.gameObject.SetActive(false);
            }

            if (actionLabelHighlightedText != null)
            {
                actionLabelHighlightedText.SetText("");
                actionLabelHighlightedText.gameObject.SetActive(false);
            }
        }

        // ── Between-hand fade-out / fade-in ──────────────────────────────────

        private void HandleFadeOutTableNodes()
        {
            // Eliminated seats have their node deactivated already — skip them.
            //if (seatController != null && seatController.IsEliminated) return;
            visualController?.FadeOutSeat();
        }

        private void HandleFadeInTableNodes()
        {
            // Eliminated seats stay hidden — their node is deactivated.
            //if (seatController != null && seatController.IsEliminated) return;
            visualController?.FadeInSeat();
        }

        // ── Item 3: Staged cleanup ────────────────────────────────────────────

        private void HandleClearActionLabel()
        {
            StopThinkingLabel(true);
            
            if (actionLabelText != null)
            {
                actionLabelText.DOFade(0f, 0.4f).OnComplete(() =>
                {
                    actionLabelText.SetText("");
                    actionLabelText.DOFade(1f, 0f);
                });
            }

            if (actionLabelHighlightedText != null)
            {
                actionLabelHighlightedText.DOFade(0f, 0.4f).OnComplete(() =>
                {
                    actionLabelHighlightedText.SetText("");
                    actionLabelHighlightedText.DOFade(1f, 0f);
                });
            }

            if (betText != null)
            {
                betText.DOFade(0f, 0.4f).OnComplete(() =>
                {
                    betText.SetText("");
                    betText.DOFade(1f, 0f);
                });
            }
            
            FadeOutWinChance();
            
            
        }

        // ── Action Label ──────────────────────────────────────────────────────

        private void ShowActionLabel(string action, int amount)
        {
            
            string label;

            switch (action?.ToLower())
            {
                case "fold":
                    label = _foldLostAmount > 0 ? $"Folded - Lost {_foldLostAmount}" : "Folded";
                    break;

                case "check":
                    label = "Check";
                    break;

                case "call":
                    label = amount > 0 ? $"Call ({amount})" : "Call";
                    break;

                case "raise":
                    label = amount > 0 ? $"Raise {amount}" : "Raise";
                    break;

                case "all_in":
                    label = "ALL-IN";
                    break;

                case "blind":
                    label = amount > 0 ? $"Bet {amount}" : "Blind";
                    break;

                default:
                    label = amount > 0 ? $"{action?.ToUpper()} {amount}" : action?.ToUpper() ?? "";
                    break;
            }

            if (actionLabelText != null)
            {
                actionLabelText.SetText(label);
                actionLabelText.gameObject.SetActive(!_isMyTurn);
            }

            if (actionLabelHighlightedText != null)
            {
                actionLabelHighlightedText.SetText(label);
                actionLabelHighlightedText.gameObject.SetActive(_isMyTurn);
            }
        }

        // ── Audio ─────────────────────────────────────────────────────────────

        private void PlayActionSound(string action)
        {
            if (Asset == null || _sound == null) return;

            switch (action?.ToLower())
            {
                case "fold":
                    _sound.PlayOneShotSong(Asset.Audio_DoFold);
                    break;
                case "check":
                    _sound.PlayOneShotSong(Asset.Audio_DoCheckCall);
                    break;
                case "call":
                    _sound.PlayOneShotSong(Asset.Audio_DoCall);
                    break;
                case "raise":
                    _sound.PlayOneShotSong(Asset.Audio_DoRaise);
                    break;
                case "all_in":
                    _sound.PlayOneShotSong(Asset.Audio_DoAllIn);
                    break;
                case "blind":
                    _sound.PlayOneShotSong(Asset.Audio_BetChips);
                    break;
            }
        }

        // ── Status overlay helpers ────────────────────────────────────────────

        private bool foldedOverlayActive(string status)
        {
            if (status != "folded") return false;
            if (foldedLabel != null) foldedLabel.SetActive(true);
            if (allInLabel  != null) allInLabel.SetActive(false);
            return true;
        }

        private bool allInOverlayActive(string status)
        {
            if (status != "all_in") return false;
            if (allInLabel  != null) allInLabel.SetActive(true);
            if (foldedLabel != null) foldedLabel.SetActive(false);
            return true;
        }

        private bool eliminatedOverlayActive(string status)
        {
            if (status != "eliminated") return false;
            if (eliminatedLabel != null) eliminatedLabel.SetActive(true);
            if (foldedLabel     != null) foldedLabel.SetActive(false);
            if (allInLabel      != null) allInLabel.SetActive(false);
            return true;
        }

        // ── Card management ───────────────────────────────────────────────────


        /// Called by ActionManager when hole cards are received for this seat.
        public void SetHoleCards(int card0, int card1, System.Action onComplete = null, bool dealAnimation = true)
        {
            _holeCard0 = card0;
            _holeCard1 = card1;

            // Showdown update — cards already visible, just update sprites and flip
            if (!dealAnimation)
            {
                if (_cardObjects.Count < 2)
                    SpawnCards();

                SetCardSprite(0, card0);
                SetCardSprite(1, card1);

                if (cardContainer != null)
                    cardContainer.SetActive(true);

                foreach (CardVisualController cvc in _cardControllers)
                {
                    if (cvc == null) continue;
                    cvc.FlipRevealInstant();
                }

                onComplete?.Invoke();
                return;
            }

            SpawnCards();
            SetCardSprite(0, card0);
            SetCardSprite(1, card1);

            if (cardContainer != null)
                cardContainer.SetActive(true);

            // Determine deal-from world position
            Vector3 dealFrom = Vector3.zero;
            if (dealOriginOverride != null)
                dealFrom = dealOriginOverride.position;
            else if (UIManager.Instance != null && UIManager.Instance.DealOrigin != null)
                dealFrom = UIManager.Instance.DealOrigin.position;
            else
                dealFrom = transform.position;

            int total = _cardControllers.Count;
            int flipped = 0;

            for (int i = 0; i < total; i++)
            {
                CardVisualController cvc = _cardControllers[i];
                if (cvc == null) continue;

                cvc.PrepareForDeal();

                int capturedIndex = i;
                float dealDelay = i * dealStagger;

                DOVirtual.DelayedCall(dealDelay, () =>
                {
                    cvc.DealIn(dealFrom, () =>
                    {
                        float fDelay = capturedIndex * flipDelay;
                        DOVirtual.DelayedCall(fDelay, () =>
                        {
                            cvc.FlipReveal(() =>
                            {
                                flipped++;
                                if (flipped >= total)
                                    onComplete?.Invoke();
                            });
                        });
                    });
                });
            }

            // Safety: if no controllers, fire immediately
            if (total == 0)
                onComplete?.Invoke();
        }

        private void SpawnCards()
        {
            ClearCards();

            if (cardPrefab == null || cardContainer == null) return;

            float spacing = 55f;
            float startX  = -(spacing * 0.5f);

            for (int i = 0; i < 2; i++)
            {
                GameObject go = Instantiate(cardPrefab, cardContainer.transform, false);
                go.transform.localPosition = new Vector3(startX + i * spacing, 0f, 0f);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale    = Vector3.one;

                CardVisualController cvc = go.GetComponent<CardVisualController>();
                SimplePoker.Logic.Card card = go.GetComponent<SimplePoker.Logic.Card>();

                _cardObjects.Add(go);
                _cardControllers.Add(cvc);
                _cards.Add(card);
            }
        }

        private void SetCardSprite(int index, int cardByte)
        {
            if (index >= _cards.Count) return;
            SimplePoker.Logic.Card card = _cards[index];
            if (card == null || deckData == null) return;

            CardStringParser.ParsedCard parsed = CardStringParser.FromInt(cardByte);
            if (!parsed.IsValid) return;

            Sprite front = deckData.Sprites_CardFront[parsed.SpriteIndex];
            Sprite back  = deckData.Sprite_CardBack;
            card.Setup(parsed.Suit, parsed.Value, back, front);
        }

        private void ClearCards()
        {
            foreach (GameObject go in _cardObjects)
                if (go != null) Destroy(go);

            _cardObjects.Clear();
            _cardControllers.Clear();
            _cards.Clear();

            _holeCard0 = -1;
            _holeCard1 = -1;

            if (cardContainer != null)
                cardContainer.SetActive(false);
        }

        // ── OnClearHoleCards / OnWinningHandRevealed handlers ─────────────────

        private void HandleClearHoleCards()
        {
            ClearCards();
        }

        private void HandleWinningHandRevealed(List<int> winnerSeats,
                                               List<List<int>> bestFivePerSeat,
                                               string handName)
        {
            if (seatController == null) return;
            if (winnerSeats == null || bestFivePerSeat == null) return;
            if (_cards.Count < 2) return;

            int winnerIndex = winnerSeats.IndexOf(seatController.seatIndex);
            bool iWon = winnerIndex >= 0;

            if (!iWon)
            {
                foreach (CardVisualController cvc in _cardControllers)
                    cvc?.DimAsNonWinner();
                return;
            }

            List<int> bestFive = winnerIndex < bestFivePerSeat.Count
                ? bestFivePerSeat[winnerIndex]
                : null;

            if (bestFive == null || bestFive.Count == 0)
            {
                foreach (CardVisualController cvc in _cardControllers)
                    cvc?.HighlightAsWinner();
                return;
            }

            HashSet<int> winSet = new HashSet<int>(bestFive);
            int[] holeCards = { _holeCard0, _holeCard1 };

            for (int i = 0; i < _cardControllers.Count; i++)
            {
                if (i >= holeCards.Length) break;
                if (winSet.Contains(holeCards[i]))
                    _cardControllers[i]?.HighlightAsWinner();
                else
                    _cardControllers[i]?.DimAsNonWinner();
            }
        }

        // ── Reset ─────────────────────────────────────────────────────────────

        private void ResetUI()
        {
            _isMyTurn = false;
            StopThinkingLabel(true);
            ResetActiveTurnAnimationInstant();
            
            if (actionLabelText         != null) actionLabelText.gameObject.SetActive(false);
            if (actionLabelHighlightedText != null) actionLabelHighlightedText.gameObject.SetActive(false);
            if (betText         != null) betText.gameObject.SetActive(false);
            if (dealerBadge     != null) dealerBadge.SetActive(false);
            if (smallBlindBadge != null) smallBlindBadge.SetActive(false);
            if (bigBlindBadge   != null) bigBlindBadge.SetActive(false);
            if (foldedLabel     != null) foldedLabel.SetActive(false);
            if (allInLabel      != null) allInLabel.SetActive(false);
            if (eliminatedLabel != null) eliminatedLabel.SetActive(false);
            if (winnerPanel     != null) winnerPanel.SetActive(false);
            StopThinkingLabel(true);
            ResetWinChanceInstant();
            
            if (seatController != null && seatController.IsEliminated)
            {
                SetCanvasGroup(activePanelCanvasGroup, false);
                SetCanvasGroup(eliminatedCanvasGroup, true);

                if (eliminatedNameText != null)
                    eliminatedNameText.SetText(seatController.DisplayName);

                if (thinkingBox != null)
                    thinkingBox.SetActive(false);

                if (foldPanel != null)
                    foldPanel.SetActive(false);

                if (foldedLabel != null)
                    foldedLabel.SetActive(false);

                if (allInLabel != null)
                    allInLabel.SetActive(false);

                if (eliminatedLabel != null)
                    eliminatedLabel.SetActive(false);

                HideActiveNameImageInstant();
                return;
            }

            SetCanvasGroup(activePanelCanvasGroup, true);
            SetCanvasGroup(eliminatedCanvasGroup, false);
            
            if (thinkingBox != null)
                thinkingBox.SetActive(false);

            HideActiveNameImageInstant();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void ApplyFont(TMPro.TMP_FontAsset font)
        {
            if (font == null) return;
            if (nameText        != null) nameText.font        = font;
            if (betText         != null) betText.font         = font;
            if (actionLabelText != null) actionLabelText.font = font;
            if (winnerHandText  != null) winnerHandText.font  = font;
        }

        private void UpdateCoinVisual(int currentStack)
        {
            if (coinsTopToBottom == null || coinsTopToBottom.Count == 0)
                return;

            int totalTableChips = GetTotalTableChips();

            float chipsPerCoin = totalTableChips / (float)coinsTopToBottom.Count;

            int coinsOn = Mathf.CeilToInt(currentStack / chipsPerCoin);

            if (currentStack <= 0)
                coinsOn = 0;

            coinsOn = Mathf.Clamp(coinsOn, 0, coinsTopToBottom.Count);

            // First setup should be instant.
            if (_lastVisibleCoinCount < 0)
            {
                SetCoinVisualInstant(coinsOn);
                _lastVisibleCoinCount = coinsOn;
                return;
            }

            // Stop any previous coin reveal animation.
            if (_coinVisualCoroutine != null)
            {
                StopCoroutine(_coinVisualCoroutine);
                _coinVisualCoroutine = null;
            }

            // Losing chips or same amount: update instantly.
            // Winning/gaining chips: reveal one by one.
            if (!animateCoinGains || coinsOn <= _lastVisibleCoinCount)
            {
                SetCoinVisualInstant(coinsOn);
                _lastVisibleCoinCount = coinsOn;
                return;
            }

            _coinVisualCoroutine = StartCoroutine(AnimateCoinGain(_lastVisibleCoinCount, coinsOn));
        }

        private void HandleWinChanceChanged(float chance01)
        {
            if (winChanceText == null) return;

            CanvasGroup cg = GetWinChanceCanvasGroup();
            if (cg == null) return;

            float percent = Mathf.Clamp01(chance01) * 100f;
            winChanceText.SetText($"{percent:0.#}%");

            // Already visible this hand, only update the number.
            if (_winChanceVisible)
                return;

            _winChanceVisible = true;

            cg.DOKill();

            // Important: prepare invisible BEFORE enabling the root.
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            if (winChanceRoot != null)
                winChanceRoot.SetActive(true);

            cg.DOFade(1f, winChanceFadeDuration)
                .SetEase(Ease.OutQuad);
        }
        
        private CanvasGroup GetWinChanceCanvasGroup()
        {
            if (winChanceRoot == null && winChanceText != null)
                winChanceRoot = winChanceText.gameObject;

            if (winChanceRoot == null)
                return null;

            if (winChanceCanvasGroup == null)
            {
                winChanceCanvasGroup = winChanceRoot.GetComponent<CanvasGroup>();

                if (winChanceCanvasGroup == null)
                    winChanceCanvasGroup = winChanceRoot.AddComponent<CanvasGroup>();
            }

            return winChanceCanvasGroup;
        }
        
        private void ResetWinChanceInstant()
        {
            CanvasGroup cg = GetWinChanceCanvasGroup();
            if (cg == null || winChanceText == null) return;

            cg.DOKill();

            // Important: alpha goes to 0 BEFORE disabling.
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            winChanceText.SetText("0%");

            if (winChanceRoot != null)
                winChanceRoot.SetActive(false);

            _winChanceVisible = false;
        }

        private void FadeOutWinChance()
        {
            CanvasGroup cg = GetWinChanceCanvasGroup();
            if (cg == null || winChanceText == null) return;

            cg.DOKill();

            cg.interactable = false;
            cg.blocksRaycasts = false;

            cg.DOFade(0f, winChanceFadeDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    winChanceText.SetText("0%");

                    if (winChanceRoot != null)
                        winChanceRoot.SetActive(false);

                    cg.alpha = 0f;
                    _winChanceVisible = false;
                });
        }
        
        
        private void ShowActiveTurnUI()
        {
            if (nameTextHighlighted != null && seatController != null)
                nameTextHighlighted.SetText(seatController.DisplayName);

            ShowActiveNameImageAfterDelay();
            
            if (thinkingBox != null)
                thinkingBox.SetActive(true);

            // Mientras es su turno, ocultamos el label normal.
            if (actionLabelText != null)
                actionLabelText.gameObject.SetActive(false);

            // El highlighted es el que se usa en turno activo.
            if (actionLabelHighlightedText != null)
                actionLabelHighlightedText.gameObject.SetActive(true);
        }

        private void StartThinkingLabel()
        {
            StopThinkingLabel(false);

            ShowActiveTurnUI();

            _thinkingCoroutine = StartCoroutine(ThinkingLabelRoutine());
        }

        private void StopThinkingLabel(bool hideTurnUI)
        {
            if (_thinkingCoroutine != null)
            {
                StopCoroutine(_thinkingCoroutine);
                _thinkingCoroutine = null;
            }

            if (!hideTurnUI)
                return;

            if (thinkingBox != null)
                thinkingBox.SetActive(false);

            HideActiveNameImageInstant();
        }
        private IEnumerator ThinkingLabelRoutine()
        {
            string[] frames = { "Thinking", "Thinking.", "Thinking..", "Thinking..." };
            int index = 0;

            while (true)
            {
                string label = frames[index];

                if (_isMyTurn)
                {
                    if (actionLabelText != null)
                        actionLabelText.gameObject.SetActive(false);

                    if (actionLabelHighlightedText != null)
                    {
                        actionLabelHighlightedText.gameObject.SetActive(true);
                        actionLabelHighlightedText.SetText(label);
                    }
                }
                else
                {
                    if (actionLabelHighlightedText != null)
                        actionLabelHighlightedText.gameObject.SetActive(false);

                    if (actionLabelText != null)
                    {
                        actionLabelText.gameObject.SetActive(true);
                        actionLabelText.SetText(label);
                    }
                }

                index = (index + 1) % frames.Length;
                yield return new WaitForSeconds(thinkingDotInterval);
            }
        }
        
        private void HandleClearCurrentActionLabel()
        {
            StopThinkingLabel(true);
            
            if (actionLabelText != null)
            {
                actionLabelText.SetText("");
                actionLabelText.gameObject.SetActive(false);
            }

            if (actionLabelHighlightedText != null)
            {
                actionLabelHighlightedText.SetText("");
                actionLabelHighlightedText.gameObject.SetActive(false);
            }
            
        }
        
        public Vector3 GetChipStackWorldPosition()
        {
            if (coinsTopToBottom != null)
            {
                // Since list is TOP -> BOTTOM, first active object is the top coin.
                for (int i = 0; i < coinsTopToBottom.Count; i++)
                {
                    GameObject coin = coinsTopToBottom[i];
                    if (coin != null && coin.activeInHierarchy)
                        return coin.transform.position;
                }

                for (int i = 0; i < coinsTopToBottom.Count; i++)
                {
                    GameObject coin = coinsTopToBottom[i];
                    if (coin != null)
                        return coin.transform.position;
                }
            }

            return transform.position;
        }

        public Vector3 GetChipWinTargetWorldPosition()
        {
            if (coinsTopToBottom != null)
            {
                for (int i = 0; i < coinsTopToBottom.Count; i++)
                {
                    GameObject coin = coinsTopToBottom[i];
                    if (coin != null)
                        return coin.transform.position;
                }
            }

            return transform.position;
        }
        
        public List<Vector3> GetActiveChipWorldPositions()
        {
            List<Vector3> positions = new List<Vector3>();

            if (coinsTopToBottom == null)
                return positions;

            for (int i = 0; i < coinsTopToBottom.Count; i++)
            {
                GameObject coin = coinsTopToBottom[i];

                if (coin != null && coin.activeInHierarchy)
                    positions.Add(coin.transform.position);
            }

            return positions;
        }
        private void SetCoinVisualInstant(int coinsOn)
        {
            for (int i = 0; i < coinsTopToBottom.Count; i++)
            {
                GameObject coin = coinsTopToBottom[i];
                if (coin == null) continue;

                coin.SetActive(i < coinsOn);
            }
        }

        private IEnumerator AnimateCoinGain(int fromCount, int toCount)
        {
            SetCoinVisualInstant(fromCount);

            for (int i = fromCount; i < toCount; i++)
            {
                if (i >= 0 && i < coinsTopToBottom.Count)
                {
                    GameObject coin = coinsTopToBottom[i];

                    if (coin != null)
                    {
                        coin.SetActive(true);

                        coin.transform.DOKill();
                        coin.transform.localScale = Vector3.zero;
                        coin.transform.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutBack);
                    }
                }

                yield return new WaitForSeconds(coinGainRevealDelay);
            }

            _lastVisibleCoinCount = toCount;
            _coinVisualCoroutine = null;
        }
        
        public List<Vector3> GetChipPaySourceWorldPositions(int maxCount)
        {
            List<Vector3> positions = new List<Vector3>();

            if (coinsTopToBottom == null || coinsTopToBottom.Count == 0)
                return positions;

            // Your current visibility logic keeps coins from the start of the list:
            // coin.SetActive(i < coinsOn)
            //
            // So the coins that disappear when paying are the LAST active coins.
            for (int i = coinsTopToBottom.Count - 1; i >= 0; i--)
            {
                GameObject coin = coinsTopToBottom[i];

                if (coin == null || !coin.activeInHierarchy)
                    continue;

                positions.Add(coin.transform.position);

                if (positions.Count >= maxCount)
                    break;
            }

            if (positions.Count == 0)
                positions.Add(GetChipStackWorldPosition());

            return positions;
        }
        
        private int GetTotalTableChips()
        {
            if (UIManager.Instance != null && UIManager.Instance.TableChipTotal > 1)
                return UIManager.Instance.TableChipTotal;

            // Fallback temporal antes de que UIManager haya detectado el total real.
            int myStack = seatController != null ? seatController.Stack : 0;

            if (myStack > 0)
                return myStack * 2;

            return 1;
        }
        
        private void HideCoinsInstant()
        {
            if (coinsTopToBottom == null)
                return;

            for (int i = 0; i < coinsTopToBottom.Count; i++)
            {
                GameObject coin = coinsTopToBottom[i];
                if (coin == null) continue;

                coin.transform.DOKill();
                coin.transform.localScale = Vector3.one;
                coin.SetActive(false);
            }

            _lastVisibleCoinCount = -1;
        }
        
        public void RefreshCoinVisual()
        {
            int stack = seatController != null ? seatController.Stack : 0;
            UpdateCoinVisual(stack);
        }
        
        private void CacheActiveTurnOriginals()
        {
            if (_cachedActiveTurnOriginals)
                return;

            if (thinkingBox != null)
                _thinkingBoxOriginalScale = thinkingBox.transform.localScale;

            if (portraitRectTransform != null)
            {
                _portraitOriginalScale = portraitRectTransform.localScale;
                _portraitOriginalAnchoredPosition = portraitRectTransform.anchoredPosition;
            }

            if (emotionRingRectTransform != null)
            {
                _emotionRingOriginalScale = emotionRingRectTransform.localScale;
                _emotionRingOriginalAnchoredPosition = emotionRingRectTransform.anchoredPosition;
            }

            _cachedActiveTurnOriginals = true;
        }

        private void PlayActiveTurnAnimation()
        {
            CacheActiveTurnOriginals();

            if (thinkingBox != null)
            {
                thinkingBox.transform.DOKill();
                thinkingBox.transform
                    .DOScale(_thinkingBoxOriginalScale * thinkingBoxActiveScale, activeTurnScaleDuration)
                    .SetEase(activeTurnScaleEase);
            }

            if (portraitRectTransform != null)
            {
                portraitRectTransform.DOKill();

                portraitRectTransform
                    .DOScale(_portraitOriginalScale * activeTurnScale, activeTurnScaleDuration)
                    .SetEase(activeTurnScaleEase);

                portraitRectTransform
                    .DOAnchorPos(_portraitOriginalAnchoredPosition + activeTurnPortraitOffset, activeTurnScaleDuration)
                    .SetEase(activeTurnScaleEase);
            }

            if (emotionRingRectTransform != null)
            {
                emotionRingRectTransform.DOKill();

                emotionRingRectTransform
                    .DOScale(_emotionRingOriginalScale * activeTurnScale, activeTurnScaleDuration)
                    .SetEase(activeTurnScaleEase);

                emotionRingRectTransform
                    .DOAnchorPos(_emotionRingOriginalAnchoredPosition + activeTurnEmotionRingOffset, activeTurnScaleDuration)
                    .SetEase(activeTurnScaleEase);
            }
        }

        private void ResetActiveTurnAnimation()
        {
            CacheActiveTurnOriginals();

            if (thinkingBox != null)
            {
                thinkingBox.transform.DOKill();
                thinkingBox.transform
                    .DOScale(_thinkingBoxOriginalScale, activeTurnScaleDuration)
                    .SetEase(inactiveTurnScaleEase);
            }

            if (portraitRectTransform != null)
            {
                portraitRectTransform.DOKill();

                portraitRectTransform
                    .DOScale(_portraitOriginalScale, activeTurnScaleDuration)
                    .SetEase(inactiveTurnScaleEase);

                portraitRectTransform
                    .DOAnchorPos(_portraitOriginalAnchoredPosition, activeTurnScaleDuration)
                    .SetEase(inactiveTurnScaleEase);
            }

            if (emotionRingRectTransform != null)
            {
                emotionRingRectTransform.DOKill();

                emotionRingRectTransform
                    .DOScale(_emotionRingOriginalScale, activeTurnScaleDuration)
                    .SetEase(inactiveTurnScaleEase);

                emotionRingRectTransform
                    .DOAnchorPos(_emotionRingOriginalAnchoredPosition, activeTurnScaleDuration)
                    .SetEase(inactiveTurnScaleEase);
            }
        }

        private void ResetActiveTurnAnimationInstant()
        {
            CacheActiveTurnOriginals();

            if (thinkingBox != null)
            {
                thinkingBox.transform.DOKill();
                thinkingBox.transform.localScale = _thinkingBoxOriginalScale;
            }

            if (portraitRectTransform != null)
            {
                portraitRectTransform.DOKill();
                portraitRectTransform.localScale = _portraitOriginalScale;
                portraitRectTransform.anchoredPosition = _portraitOriginalAnchoredPosition;
            }

            if (emotionRingRectTransform != null)
            {
                emotionRingRectTransform.DOKill();
                emotionRingRectTransform.localScale = _emotionRingOriginalScale;
                emotionRingRectTransform.anchoredPosition = _emotionRingOriginalAnchoredPosition;
            }
        }
        
        private void ShowActiveNameImageAfterDelay()
        {
            if (activeNameImage == null)
                return;

            // If it is already visible, do not restart the delay.
            if (activeNameImage.activeSelf || _activeNameImageDelayRunning)
                return;

            activeNameImage.SetActive(false);

            float delay = Mathf.Max(0f, activeNameImageShowDelay);

            if (delay <= 0f)
            {
                activeNameImage.SetActive(true);
                return;
            }

            _activeNameImageDelayTween?.Kill();
            _activeNameImageDelayRunning = true;

            _activeNameImageDelayTween = DOVirtual.DelayedCall(delay, () =>
            {
                _activeNameImageDelayRunning = false;
                _activeNameImageDelayTween = null;

                // Safety: only show it if this player is still the current turn.
                if (_isMyTurn && activeNameImage != null)
                    activeNameImage.SetActive(true);
            });
        }

        private void HideActiveNameImageInstant()
        {
            _activeNameImageDelayTween?.Kill();
            _activeNameImageDelayTween = null;
            _activeNameImageDelayRunning = false;

            if (activeNameImage != null)
                activeNameImage.SetActive(false);
        }
        
        private void ShowFoldPanel()
        {
            if (thinkingBox != null)
                thinkingBox.SetActive(false);

            if (foldPanel != null)
                foldPanel.SetActive(true);

            SetCanvasGroup(activePanelCanvasGroup, true);
            SetCanvasGroup(eliminatedCanvasGroup, false);
        }
        
        private void SetCanvasGroup(CanvasGroup group, bool visible)
        {
            if (group == null) return;

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
        
        public Vector3 GetChipBetStartWorldPosition()
        {
            // New requirement: chips start from the profile picture.
            if (portraitRectTransform != null)
                return portraitRectTransform.position;

            if (portraitImage != null)
                return portraitImage.transform.position;

            return transform.position;
        }

        public Vector3 GetChipBetBoxWorldPosition()
        {
            if (betBoxRectTransform != null)
            {
                Vector3 worldOffset =
                    betBoxRectTransform.right * betBoxChipLandingOffset.x +
                    betBoxRectTransform.up * betBoxChipLandingOffset.y;

                return betBoxRectTransform.position + worldOffset;
            }

            if (betText != null)
                return betText.transform.position;

            return GetChipStackWorldPosition();
        }
        
        private void ForceHideActiveTurnUI()
        {
            _isMyTurn = false;

            _activeNameImageDelayTween?.Kill();
            _activeNameImageDelayTween = null;
            _activeNameImageDelayRunning = false;

            // Oculta el contenedor completo al iniciar.
            if (activeNameImage != null)
                activeNameImage.SetActive(false);

            if (thinkingBox != null)
                thinkingBox.SetActive(false);

            // El texto debe permanecer activo dentro del contenedor.
            if (nameTextHighlighted != null)
                nameTextHighlighted.gameObject.SetActive(true);

            if (actionLabelHighlightedText != null)
                actionLabelHighlightedText.gameObject.SetActive(false);

            visualController?.SetActiveTurn(false);
        }
    }
}