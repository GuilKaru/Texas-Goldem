using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TexasHoldem
{
    public class FeaturedSeatDisplay : MonoBehaviour
    {
        public static FeaturedSeatDisplay Instance { get; private set; }

        // ── Inspector refs ────────────────────────────────────────────────────

        [Header("Portrait")]
        [SerializeField] private Image          portraitImage;
        [SerializeField] private GameObject     activeTurnRing;     // orange ring shown when thinking

        [Header("Name")]
        [SerializeField] private TextMeshProUGUI nameText;

        [Header("Thinking Box")]
        [SerializeField] private GameObject     thinkingBox;        // shown only when to_act is set
        [SerializeField] private TextMeshProUGUI thinkingText;
        [SerializeField] private float          thinkingDotInterval = 0.22f;

        [Header("Neutral State")]
        [SerializeField] private GameObject     neutralStateRoot;   // shown when no active player

        [Header("Transition")]
        [SerializeField] private CanvasGroup    canvasGroup;
        [SerializeField] private float          switchFadeDuration  = 0.15f;

        // ── Private ───────────────────────────────────────────────────────────

        private Coroutine _thinkingCoroutine;
        private int       _currentActiveSeat = -1;
        private int       _thinkingSeat      = -1;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            PokerTableEvents.OnActionSeatChanged   += HandleActionSeatChanged;
            PokerTableEvents.OnSeatThinkingChanged += HandleSeatThinkingChanged;
            PokerTableEvents.OnMatchInitialized    += HandleMatchInitialized;

            SetNeutral();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            PokerTableEvents.OnActionSeatChanged   -= HandleActionSeatChanged;
            PokerTableEvents.OnSeatThinkingChanged -= HandleSeatThinkingChanged;
            PokerTableEvents.OnMatchInitialized    -= HandleMatchInitialized;

            StopThinking();
        }

        // ── Public API ────────────────────────────────────────────────────────
        
        /// Clears the featured display — no active player.
        /// Called between streets and between hands.
        public void SetNeutral()
        {
            StopThinking();

            _currentActiveSeat = -1;
            _thinkingSeat      = -1;

            if (thinkingBox) thinkingBox.SetActive(false);
            if (activeTurnRing) activeTurnRing.SetActive(false);
            if (neutralStateRoot) neutralStateRoot.SetActive(true);
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void HandleMatchInitialized()
        {
            SetNeutral();
        }

        private void HandleActionSeatChanged(int? seatIndex)
        {
            if (!seatIndex.HasValue)
            {
                SetNeutral();
                return;
            }

            int seat = seatIndex.Value;

            // Same seat is still thinking — no visual change needed
            if (seat == _currentActiveSeat) return;

            _currentActiveSeat = seat;

            // Fetch seat data from GameManager
            SeatController controller = GameManager.Instance
                ? GameManager.Instance.GetSeatController(seat)
                : null;

            if (!controller)
            {
                SetNeutral();
                return;
            }

            ShowActiveSeat(controller);
        }
        
        /// Fires only when Unity is speculatively guessing which seat acts next,
        /// ahead of real backend data. This is the sole trigger for the
        /// "Thinking..." animation — NOT OnActionSeatChanged by itself.
        private void HandleSeatThinkingChanged(int? seatIndex)
        {
            _thinkingSeat = seatIndex ?? -1;
            UpdateThinkingVisual();
        }

        // ── Internal display ──────────────────────────────────────────────────

        private void ShowActiveSeat(SeatController controller)
        {
            // Brief fade-out → update → fade-in for smooth seat switching
            if (canvasGroup && _currentActiveSeat >= 0)
            {
                canvasGroup.DOKill();
                canvasGroup.DOFade(0f, switchFadeDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => ApplySeatData(controller));
            }
            else
            {
                ApplySeatData(controller);
            }
        }

        private void ApplySeatData(SeatController controller)
        {
            // Portrait
            if (portraitImage && controller.AvatarSprite)
                portraitImage.sprite = controller.AvatarSprite;

            // Name
            if (nameText)
                nameText.SetText(controller.DisplayName);

            // Hide neutral state
            if (neutralStateRoot) neutralStateRoot.SetActive(false);

            // Show active turn ring
            if (activeTurnRing) activeTurnRing.SetActive(true);

            // Thinking box visibility depends on whether this seat is the one
            // Unity is speculatively guessing will act next — not automatic.
            UpdateThinkingVisual();

            // Fade back in
            if (canvasGroup)
            {
                canvasGroup.DOKill();
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, switchFadeDuration).SetEase(Ease.OutQuad);
            }
        }

        // ── Thinking animation ────────────────────────────────────────────────
        
        /// Shows/hides the thinking box + animation based on whether the seat
        /// currently displayed here is also the seat Unity is speculatively
        /// guessing will act next. Called whenever either piece of state changes.
        private void UpdateThinkingVisual()
        {
            bool shouldThink = _currentActiveSeat >= 0 && _currentActiveSeat == _thinkingSeat;

            if (shouldThink)
            {
                if (thinkingBox) thinkingBox.SetActive(true);
                StopThinking();
                _thinkingCoroutine = StartCoroutine(ThinkingRoutine());
            }
            else
            {
                StopThinking();
                if (thinkingBox) thinkingBox.SetActive(false);
            }
        }

        private IEnumerator ThinkingRoutine()
        {
            string[] frames = { "Thinking", "Thinking.", "Thinking..", "Thinking..." };
            int index = 0;

            while (true)
            {
                if (thinkingText)
                    thinkingText.SetText(frames[index]);

                index = (index + 1) % frames.Length;
                yield return new WaitForSeconds(thinkingDotInterval);
            }
        }

        private void StopThinking()
        {
            if (_thinkingCoroutine != null)
            {
                StopCoroutine(_thinkingCoroutine);
                _thinkingCoroutine = null;
            }

            if (thinkingText)
                thinkingText.SetText("Thinking");
        }
    }
}