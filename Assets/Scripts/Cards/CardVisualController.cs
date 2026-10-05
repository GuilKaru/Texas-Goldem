using System;
using DG.Tweening;
using UnityEngine;

namespace TexasHoldem
{
    [RequireComponent(typeof(SimplePoker.Logic.Card))]
    public class CardVisualController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float dealDuration      = 0.3f;
        [SerializeField] private float flipHalfDuration  = 0.1f;
        [SerializeField] private float foldSlideDuration = 0.4f;
        [SerializeField] private float foldFadeDuration  = 0.3f;

        [Header("Deal")]
        [SerializeField] private Ease dealEase = Ease.OutQuad;

        [Header("Fold")]
        [SerializeField] private Vector3 foldSlideOffset = new Vector3(0f, -300f, 0f);

        [Header("Winning Hand")]
        [Tooltip("Alpha applied to a card that is NOT part of the winning hand.")]
        [SerializeField] private float dimAlpha              = 0.3f;
        [SerializeField] private float highlightFadeDuration = 0.35f;
        [SerializeField] private float highlightPunchScale   = 0.08f;

        // ── Private ───────────────────────────────────────────────────────────

        private SimplePoker.Logic.Card _card;
        private CanvasGroup            _canvasGroup;

        private Action _onDealComplete;
        private Action _onFlipComplete;
        private Action _onFoldComplete;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _card        = GetComponent<SimplePoker.Logic.Card>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // ── Deal ──────────────────────────────────────────────────────────────

        public void DealIn(Vector3 startWorldPosition, Action onComplete = null)
        {
            _onDealComplete = onComplete;

            Vector3 targetWorldPos = transform.position;

            DOTween.Kill(transform);
            DOTween.Kill(_canvasGroup);

            transform.position      = startWorldPosition;
            transform.localRotation = Quaternion.identity;
            transform.localScale    = Vector3.one;

            _canvasGroup.alpha = 1f;
            _card.ShowCard     = false;
            _card.HideCard     = false;
            _card.CardShowState(false);

            transform.DOMove(targetWorldPos, dealDuration)
                .SetEase(dealEase)
                .OnComplete(() => _onDealComplete?.Invoke());
        }

        // ── Flip reveal ───────────────────────────────────────────────────────

        public void FlipReveal(Action onComplete = null)
        {
            Debug.Log($"[CardVisualController] FlipReveal card='{gameObject.name}'");

            _onFlipComplete = onComplete;

            DOTween.Kill(transform);
            transform.localRotation = Quaternion.identity;
            transform.localScale    = Vector3.one;

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScaleX(0f, flipHalfDuration).SetEase(Ease.InQuad));
            seq.AppendCallback(() =>
            {
                _card.ShowCard = true;
                _card.HideCard = false;
                _card.CardShowState(true);
            });
            seq.Append(transform.DOScaleX(1f, flipHalfDuration).SetEase(Ease.OutQuad));
            seq.OnComplete(() =>
            {
                Debug.Log($"[CardVisualController] Flip complete card='{gameObject.name}'");
                _onFlipComplete?.Invoke();
            });
        }
        
        public void FlipRevealInstant()
        {
            DOTween.Kill(transform);
            DOTween.Kill(_canvasGroup);

            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            if (_canvasGroup)
                _canvasGroup.alpha = 1f;

            if (_card)
            {
                _card.ShowCard = true;
                _card.HideCard = false;
                _card.CardShowState(true);
            }

            gameObject.SetActive(true);
        }

        // ── Fold ──────────────────────────────────────────────────────────────

        public void FoldSlide(Action onComplete = null)
        {
            _onFoldComplete = onComplete;

            Sequence seq = DOTween.Sequence();
            seq.Join(transform.DOLocalMove(
                transform.localPosition + foldSlideOffset,
                foldSlideDuration).SetEase(Ease.InQuad));
            seq.Join(_canvasGroup.DOFade(0f, foldFadeDuration)
                .SetDelay(foldSlideDuration - foldFadeDuration));
            seq.OnComplete(() =>
            {
                gameObject.SetActive(false);
                _onFoldComplete?.Invoke();
            });
        }

        // ── Winning hand ──────────────────────────────────────────────────────

        /// <summary>Full opacity + scale punch — this card IS part of the winning hand.</summary>
        public void HighlightAsWinner()
        {
            DOTween.Kill(_canvasGroup);
            _canvasGroup.DOFade(1f, highlightFadeDuration);

            DOTween.Kill(transform, true);
            transform.DOPunchScale(
                Vector3.one * highlightPunchScale,
                highlightFadeDuration + 0.1f, 1, 0.3f);
        }

        /// <summary>Dims to dimAlpha — this card is NOT part of the winning hand.</summary>
        public void DimAsNonWinner()
        {
            DOTween.Kill(_canvasGroup);
            _canvasGroup.DOFade(dimAlpha, highlightFadeDuration);
        }

        /// <summary>Restores full opacity instantly. Called on reset / new hand.</summary>
        public void ClearHighlight()
        {
            DOTween.Kill(_canvasGroup);
            _canvasGroup.alpha = 1f;
        }

        // ── Staged cleanup ────────────────────────────────────────────────────

        /// <summary>Gently fades the card out then destroys its GameObject.</summary>
        public void FadeOutAndDestroy(float duration = 0.5f)
        {
            DOTween.Kill(transform);
            DOTween.Kill(_canvasGroup);

            _canvasGroup.DOFade(0f, duration).OnComplete(() =>
            {
                if (gameObject != null)
                    Destroy(gameObject);
            });
        }

        // ── Reset ─────────────────────────────────────────────────────────────

        public void ResetInstant()
        {
            DOTween.Kill(transform);
            DOTween.Kill(_canvasGroup);

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale    = Vector3.one;

            _canvasGroup.alpha = 1f;
            _card.ShowCard     = false;
            _card.HideCard     = false;
            _card.CardShowState(false);
            gameObject.SetActive(true);

            ClearHighlight();
        }

        public void ShowFaceDown()
        {
            DOTween.Kill(transform);
            _card.ShowCard  = false;
            _card.HideCard  = true;
            _card.CardShowState(false);
            _card.CardShowState(false);
            _canvasGroup.alpha = 1f;
            gameObject.SetActive(true);
        }

        public void PrepareForDeal()
        {
            if (!_canvasGroup)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup)
                _canvasGroup.alpha = 0f;

            transform.localRotation = Quaternion.identity;
            transform.localScale    = Vector3.one;

            if (_card)
            {
                _card.ShowCard = false;
                _card.HideCard = false;
                _card.CardShowState(false);
            }

            //ClearHighlight();
            gameObject.SetActive(true);
        }
    }
}