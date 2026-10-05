using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TexasHoldem
{
    [RequireComponent(typeof(RectTransform))]
    public class UIStarBreather : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image starImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Play")]
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool randomizeTiming = true;

        [Header("Flicker Size")]
        [SerializeField] private Vector2 hiddenSize = new Vector2(0f, 0f);
        [SerializeField] private Vector2 visibleSizeIncrease = new Vector2(12f, 12f);

        [Header("Flicker Timing")]
        [SerializeField] private float growDuration = 0.18f;
        [SerializeField] private float shrinkDuration = 0.18f;
        [SerializeField] private float visibleHoldDuration = 0.05f;
        [SerializeField] private float startDelay = 0f;

        [Header("Random Timing")]
        [SerializeField] private Vector2 randomStartDelayRange = new Vector2(0f, 2f);
        [SerializeField] private Vector2 randomHiddenDelayRange = new Vector2(1f, 4f);
        [SerializeField] private Vector2 randomGrowDurationRange = new Vector2(0.12f, 0.28f);
        [SerializeField] private Vector2 randomShrinkDurationRange = new Vector2(0.10f, 0.24f);

        [Header("Flicker Alpha")]
        [SerializeField] private float hiddenAlpha = 0f;
        [SerializeField] private float visibleAlpha = 1f;

        [Header("Ease")]
        [SerializeField] private Ease growEase = Ease.OutSine;
        [SerializeField] private Ease shrinkEase = Ease.InSine;
        [SerializeField] private Ease alphaEase = Ease.InOutSine;

        private RectTransform _rectTransform;
        private Vector2 _defaultSize;
        private Vector2 _visibleSize;
        private bool _hasCachedDefaultSize;

        private Sequence _flickerSequence;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            if (starImage == null)
                starImage = GetComponent<Image>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            CacheDefaultSize();
            SetHiddenInstant();
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            Stop(true);
        }

        private void OnDestroy()
        {
            KillTween();
        }

        public void Play()
        {
            CacheDefaultSize();
            KillTween();
            SetHiddenInstant();

            float firstDelay = randomizeTiming
                ? Random.Range(randomStartDelayRange.x, randomStartDelayRange.y)
                : startDelay;

            _flickerSequence = DOTween.Sequence();
            _flickerSequence.AppendInterval(firstDelay);
            _flickerSequence.AppendCallback(PlayNextFlicker);
        }

        public void Stop(bool reset)
        {
            KillTween();

            if (reset)
                SetHiddenInstant();
        }

        private void PlayNextFlicker()
        {
            KillTween();

            float currentGrowDuration = randomizeTiming
                ? Random.Range(randomGrowDurationRange.x, randomGrowDurationRange.y)
                : growDuration;

            float currentShrinkDuration = randomizeTiming
                ? Random.Range(randomShrinkDurationRange.x, randomShrinkDurationRange.y)
                : shrinkDuration;

            float hiddenDelay = randomizeTiming
                ? Random.Range(randomHiddenDelayRange.x, randomHiddenDelayRange.y)
                : 1f;

            _flickerSequence = DOTween.Sequence();

            // Start hidden and small.
            _flickerSequence.AppendCallback(SetHiddenInstant);

            // Appear while growing larger.
            _flickerSequence.Append(_rectTransform
                .DOSizeDelta(_visibleSize, currentGrowDuration)
                .SetEase(growEase));

            _flickerSequence.Join(canvasGroup
                .DOFade(visibleAlpha, currentGrowDuration)
                .SetEase(alphaEase));

            // Tiny moment fully visible.
            if (visibleHoldDuration > 0f)
                _flickerSequence.AppendInterval(visibleHoldDuration);

            // Disappear while shrinking smaller.
            _flickerSequence.Append(_rectTransform
                .DOSizeDelta(hiddenSize, currentShrinkDuration)
                .SetEase(shrinkEase));

            _flickerSequence.Join(canvasGroup
                .DOFade(hiddenAlpha, currentShrinkDuration)
                .SetEase(alphaEase));

            // Stay hidden for random time, then flicker again.
            _flickerSequence.AppendInterval(hiddenDelay);
            _flickerSequence.AppendCallback(PlayNextFlicker);
        }

        private void SetHiddenInstant()
        {
            if (_rectTransform != null)
                _rectTransform.sizeDelta = hiddenSize;

            if (canvasGroup != null)
                canvasGroup.alpha = hiddenAlpha;
        }

        private void CacheDefaultSize()
        {
            if (_hasCachedDefaultSize)
                return;

            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            if (_rectTransform == null)
                return;

            _defaultSize = _rectTransform.sizeDelta;
            _visibleSize = _defaultSize + visibleSizeIncrease;

            _hasCachedDefaultSize = true;
        }

        private void KillTween()
        {
            _flickerSequence?.Kill();
            _flickerSequence = null;
        }


    }
}