using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TexasHoldem
{
   public class CommunityCardVisual : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Image cardImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation")]
        [SerializeField] private float dealDuration = 0.3f;
        [SerializeField] private float flipHalfDuration = 0.1f;
        [SerializeField] private Ease dealEase = Ease.OutQuad;

        [Header("Winning Hand")]
        [SerializeField] private float dimAlpha = 0.3f;
        [SerializeField] private float highlightFadeDuration = 0.35f;
        [SerializeField] private float highlightPunchScale = 0.08f;

        private RectTransform _rectTransform;
        private Vector2 _targetAnchoredPos;
        private bool _initialized = false;

        public int CurrentCardInt { get; private set; } = -1;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;

            _rectTransform = transform as RectTransform;

            if (!canvasGroup)
                canvasGroup = GetComponent<CanvasGroup>();
            if (!canvasGroup)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (!cardImage)
                cardImage = GetComponent<Image>();

            if (_rectTransform)
                _targetAnchoredPos = _rectTransform.anchoredPosition;

            _initialized = true;
        }

        private Vector2 WorldToAnchoredPosition(Vector3 worldPos)
        {
            RectTransform parentRect = _rectTransform.parent as RectTransform;
            if (!parentRect)
                return _targetAnchoredPos;

            Canvas canvas = GetComponentInParent<Canvas>();
            Camera cam = null;

            if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = canvas.worldCamera;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, cam, out Vector2 localPoint);
            return localPoint;
        }

        public void ResetInstant()
        {
            EnsureInitialized();
            if (!_rectTransform || !canvasGroup) return;

            DOTween.Kill(_rectTransform);
            DOTween.Kill(canvasGroup);

            _rectTransform.anchoredPosition = _targetAnchoredPos;
            _rectTransform.localScale = Vector3.one;
            _rectTransform.localRotation = Quaternion.identity;

            canvasGroup.alpha = 0f;
            CurrentCardInt = -1;
            gameObject.SetActive(false);
        }

        public void ShowFrontInstant(Sprite sprite, int cardInt = -1)
        {
            EnsureInitialized();
            if (!_rectTransform || !canvasGroup) return;

            DOTween.Kill(_rectTransform);
            DOTween.Kill(canvasGroup);

            if (cardImage)
                cardImage.sprite = sprite;

            _rectTransform.anchoredPosition = _targetAnchoredPos;
            _rectTransform.localScale = Vector3.one;
            _rectTransform.localRotation = Quaternion.identity;

            canvasGroup.alpha = 1f;
            CurrentCardInt = cardInt;
            gameObject.SetActive(true);
        }

        public void DealAndFlipFromWorld(Vector3 startWorldPos, Sprite backSprite, Sprite frontSprite,
            int cardInt = -1, float startDelay = 0f, Action onComplete = null)
        {
            EnsureInitialized();
            if (!_rectTransform || !canvasGroup)
            {
                onComplete?.Invoke();
                return;
            }

            DOTween.Kill(_rectTransform);
            DOTween.Kill(canvasGroup);

            gameObject.SetActive(true);
            CurrentCardInt = cardInt;

            Vector2 startAnchoredPos = WorldToAnchoredPosition(startWorldPos);

            _rectTransform.anchoredPosition = startAnchoredPos;
            _rectTransform.localScale = Vector3.one;
            _rectTransform.localRotation = Quaternion.identity;

            if (cardImage != null)
                cardImage.sprite = backSprite;

            canvasGroup.alpha = 1f;

            Sequence seq = DOTween.Sequence();
            if (startDelay > 0f)
                seq.AppendInterval(startDelay);

            seq.Append(_rectTransform.DOAnchorPos(_targetAnchoredPos, dealDuration).SetEase(dealEase));
            seq.Append(_rectTransform.DOScaleX(0f, flipHalfDuration).SetEase(Ease.InQuad));
            seq.AppendCallback(() =>
            {
                if (cardImage != null)
                    cardImage.sprite = frontSprite;
            });
            seq.Append(_rectTransform.DOScaleX(1f, flipHalfDuration).SetEase(Ease.OutQuad));
            seq.OnComplete(() => onComplete?.Invoke());
        }
        public void HighlightAsWinner()
        {
            EnsureInitialized();
            DOTween.Kill(canvasGroup);
            canvasGroup.DOFade(1f, highlightFadeDuration);

            if (_rectTransform != null)
            {
                DOTween.Kill(_rectTransform, true);
                _rectTransform.DOPunchScale(Vector3.one * highlightPunchScale,
                    highlightFadeDuration + 0.1f, 1, 0.3f);
            }
        }

        public void DimAsNonWinner()
        {
            EnsureInitialized();
            DOTween.Kill(canvasGroup);
            canvasGroup.DOFade(dimAlpha, highlightFadeDuration);
        }

        public void ClearHighlight()
        {
            if (!canvasGroup) return;
            DOTween.Kill(canvasGroup);
            if (gameObject.activeSelf)
                canvasGroup.alpha = 1f;
        }
    }
}