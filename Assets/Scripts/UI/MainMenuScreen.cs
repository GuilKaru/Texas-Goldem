using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TexasHoldem
{
    public class MainMenuScreen : MonoBehaviour
    {
        public static MainMenuScreen Instance { get; private set; }

        [Header("UI")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider slider;

        [Header("Star Animation")]
        [SerializeField] private RectTransform rotatingStar;
        [SerializeField] private float starRotationSpeed = 180f;
        [SerializeField] private bool rotateStarOnlyWhenVisible = true;

        [Header("Timing")]
        [Tooltip("Duración en segundos para llenar la barra.")]
        [SerializeField] private float fillDuration = 1.6f;

        [Tooltip("Duración en segundos del fade out.")]
        [SerializeField] private float fadeOutDuration = 0.35f;

        [Tooltip("Pausa opcional cuando llega a 100% antes del fade.")]
        [SerializeField] private float holdAtFull = 0.05f;

        [Header("Behaviour")]
        [Tooltip("Si está activo, hace autoplay en Start(). Para tu caso normalmente NO.")]
        [SerializeField] private bool playOnStart = false;
        
        [Header("Glint Animation")]
        [SerializeField] private RectTransform borderGlint;
        [SerializeField] private Vector2 borderPointA;
        [SerializeField] private Vector2 borderPointB;
        [SerializeField] private float borderGlintDuration = 1.25f;
        [SerializeField] private float borderGlintPauseAtEnds = 0.15f;

        [SerializeField] private RectTransform titleGlint;
        [SerializeField] private Vector2 titlePointA;
        [SerializeField] private Vector2 titlePointB;
        [SerializeField] private float titleGlintDuration = 0.65f;
        

        private CancellationTokenSource _cts;
        private bool _isPlaying;
        private Coroutine _borderGlintRoutine;
        private Coroutine _titleGlintRoutine;
        

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();

            if (slider)
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = 0f;
            }

            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void Start()
        {
            if (playOnStart)
                _ = PlayAsync();
        }

        private void Update()
        {
            RotateStar();
        }

        private void RotateStar()
        {
            if (!rotatingStar) return;

            if (rotateStarOnlyWhenVisible && !_isPlaying)
                return;

            rotatingStar.Rotate(
                0f,
                0f,
                -starRotationSpeed * Time.unscaledDeltaTime
            );
        }

        private void OnDisable()
        {
            StopGlintAnimation();
            
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _isPlaying = false;
        }
        
        /// Reproduce la "carga falsa": aparece, llena barra, y hace fade out.
        /// Se puede llamar desde ActionManager (turno 0).
        public async Task PlayAsync()
        {
            if (_isPlaying) return;
            _isPlaying = true;
            
            StartGlintAnimation();

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            if (canvasGroup)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            if (slider) slider.value = 0f;

            try
            {
                await FillSliderAsync(fillDuration, token);

                if (holdAtFull > 0f)
                    await DelaySecondsAsync(holdAtFull, token);

                await FadeOutAsync(fadeOutDuration, token);
            }
            catch (OperationCanceledException)
            {
                // Ignorar: cancelación normal si se desactiva o reinicia.
            }
            finally
            {
                if (canvasGroup)
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }

                _isPlaying = false;
            }
        }

        private async Task FillSliderAsync(float seconds, CancellationToken token)
        {
            if (!slider)
            {
                await DelaySecondsAsync(seconds, token);
                return;
            }

            float t = 0f;

            while (t < seconds)
            {
                token.ThrowIfCancellationRequested();

                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / seconds);

                p = Mathf.SmoothStep(0f, 1f, p);

                slider.value = p;

                await Task.Yield();
            }

            slider.value = 1f;
        }

        private async Task FadeOutAsync(float seconds, CancellationToken token)
        {
            if (!canvasGroup)
            {
                await DelaySecondsAsync(seconds, token);
                return;
            }

            float start = canvasGroup.alpha;
            float t = 0f;

            while (t < seconds)
            {
                token.ThrowIfCancellationRequested();

                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / seconds);

                canvasGroup.alpha = Mathf.Lerp(start, 0f, p);

                await Task.Yield();
            }

            canvasGroup.alpha = 0f;
        }

        private static async Task DelaySecondsAsync(float seconds, CancellationToken token)
        {
            float t = 0f;

            while (t < seconds)
            {
                token.ThrowIfCancellationRequested();

                t += Time.unscaledDeltaTime;

                await Task.Yield();
            }
        }
        
        private void StartGlintAnimation()
        {
            StopGlintAnimation();

            if (borderGlint)
                _borderGlintRoutine = StartCoroutine(BorderGlintLoop());
        }

        private void StopGlintAnimation()
        {
            if (_borderGlintRoutine != null)
            {
                StopCoroutine(_borderGlintRoutine);
                _borderGlintRoutine = null;
            }

            if (_titleGlintRoutine != null)
            {
                StopCoroutine(_titleGlintRoutine);
                _titleGlintRoutine = null;
            }
        }

        private IEnumerator BorderGlintLoop()
        {
            borderGlint.anchoredPosition = borderPointA;

            if (titleGlint)
                titleGlint.anchoredPosition = titlePointA;

            while (true)
            {
                // Way out: only Border glint moves.
                yield return MoveGlintAsync(
                    borderGlint,
                    borderPointA,
                    borderPointB,
                    borderGlintDuration
                );

                if (borderGlintPauseAtEnds > 0f)
                    yield return new WaitForSecondsRealtime(borderGlintPauseAtEnds);

                // Way back: only Title glint moves.
                if (titleGlint)
                {
                    titleGlint.anchoredPosition = titlePointA;

                    yield return MoveGlintAsync(
                        titleGlint,
                        titlePointA,
                        titlePointB,
                        titleGlintDuration
                    );
                }
                else
                {
                    yield return MoveGlintAsync(
                        borderGlint,
                        borderPointB,
                        borderPointA,
                        borderGlintDuration
                    );
                }

                // Reset border instantly so it is ready for the next "way out".
                borderGlint.anchoredPosition = borderPointA;

                if (titleGlint)
                    titleGlint.anchoredPosition = titlePointA;

                if (borderGlintPauseAtEnds > 0f)
                    yield return new WaitForSecondsRealtime(borderGlintPauseAtEnds);
            }
        }

        private static IEnumerator MoveGlintAsync(
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
    }
}