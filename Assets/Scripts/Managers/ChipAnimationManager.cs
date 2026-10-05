using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TexasHoldem
{
    public class ChipAnimationManager : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private Canvas animationCanvas;
        [SerializeField] private RectTransform animationRoot;

        [Header("Prefab")]
        [Tooltip("Small UI Image/GameObject used as the flying chip.")]
        [SerializeField] private GameObject chipPrefab;

        [Header("Targets")]
        [Tooltip("Point where flying chips should land. Usually an empty RectTransform near the visible pot chips.")]
        [SerializeField] private RectTransform potLandingTarget;

        [Tooltip("Visual pot object that should punch/scale when chips arrive.")]
        [SerializeField] private RectTransform potVisualTarget;

        [Header("Bet Animation")]
        [SerializeField] private int maxChipsPerBetAnimation = 6;
        [SerializeField] private float chipSpawnRadius = 18f;
        [SerializeField] private float chipTravelDuration = 0.45f;
        [SerializeField] private float chipStagger = 0.045f;
        [SerializeField] private Ease chipMoveEase = Ease.OutQuad;

        [Header("Win Animation")]
        [SerializeField] private int maxChipsForPotAward = 12;
        [SerializeField] private float winTravelDuration = 0.65f;
        [SerializeField] private float winChipStagger = 0.035f;
        [SerializeField] private Ease winMoveEase = Ease.InOutQuad;

        [Header("Punch")]
        [SerializeField] private float potPunchScale = 0.12f;
        [SerializeField] private float targetPunchScale = 0.10f;
        [SerializeField] private float punchDuration = 0.18f;

        private readonly List<GameObject> _activeFlyingChips =
            new List<GameObject>();

        private readonly Dictionary<int, int> _heldBetChipCountBySeat =
            new Dictionary<int, int>();

        private Camera CanvasCamera
        {
            get
            {
                if (!animationCanvas)
                    animationCanvas = GetComponentInParent<Canvas>();

                if (!animationCanvas)
                    return null;

                return animationCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : animationCanvas.worldCamera;
            }
        }

        private RectTransform PotLandingTarget => potLandingTarget;
        private RectTransform PotVisualTarget => potVisualTarget;

        private void Awake()
        {
            if (!animationCanvas)
                animationCanvas = GetComponentInParent<Canvas>();

            if (!animationRoot)
                animationRoot = transform as RectTransform;
        }

        private void OnEnable()
        {
            PokerTableEvents.OnChipsToPot += HandleChipsToPot;
            PokerTableEvents.OnCollectBetsToPot += HandleCollectBetsToPot;
            PokerTableEvents.OnPotAwarded += HandlePotAwarded;
            PokerTableEvents.OnNewHand += HandleNewHand;
            PokerTableEvents.OnClearBoardAndPot += HandleClearBoardAndPot;
        }

        private void OnDisable()
        {
            PokerTableEvents.OnChipsToPot -= HandleChipsToPot;
            PokerTableEvents.OnCollectBetsToPot -= HandleCollectBetsToPot;
            PokerTableEvents.OnPotAwarded -= HandlePotAwarded;
            PokerTableEvents.OnNewHand -= HandleNewHand;
            PokerTableEvents.OnClearBoardAndPot -= HandleClearBoardAndPot;
        }

        // ─────────────────────────────────────────────────────────────────────
        // PLAYER → BET BOX
        // ─────────────────────────────────────────────────────────────────────

        private void HandleChipsToPot(int seatIndex, int amount)
        {
            if (amount <= 0)
                return;

            UIPlayer playerUI = GetPlayerUI(seatIndex);

            if (!playerUI)
                return;

            int chipCount = GetVisualChipCount(
                amount,
                maxChipsPerBetAnimation);

            Vector3 fromWorld =
                playerUI.GetChipBetStartWorldPosition();

            Vector3 betBoxWorld =
                playerUI.GetChipBetBoxWorldPosition();

            PlayChipsToBetBox(
                seatIndex,
                fromWorld,
                betBoxWorld,
                chipCount,
                chipTravelDuration,
                chipStagger,
                chipMoveEase);
        }

        private void PlayChipsToBetBox(
            int seatIndex,
            Vector3 fromWorld,
            Vector3 betBoxWorld,
            int chipCount,
            float duration,
            float stagger,
            Ease ease)
        {
            if (!chipPrefab || !animationRoot)
                return;

            Vector2 from =
                WorldToAnchoredPosition(fromWorld);

            Vector2 betBox =
                WorldToAnchoredPosition(betBoxWorld);

            if (!_heldBetChipCountBySeat.ContainsKey(seatIndex))
                _heldBetChipCountBySeat[seatIndex] = 0;

            for (int i = 0; i < chipCount; i++)
            {
                GameObject chip =
                    Instantiate(chipPrefab, animationRoot);

                _activeFlyingChips.Add(chip);

                RectTransform rect =
                    chip.transform as RectTransform;

                if (!rect)
                {
                    _activeFlyingChips.Remove(chip);
                    Destroy(chip);
                    continue;
                }

                CanvasGroup canvasGroup =
                    chip.GetComponent<CanvasGroup>();

                if (!canvasGroup)
                    canvasGroup = chip.AddComponent<CanvasGroup>();

                Vector2 heldOffset =
                    Random.insideUnitCircle *
                    (chipSpawnRadius * 0.45f);

                rect.anchoredPosition = from;
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;

                canvasGroup.alpha = 0f;

                float delay = i * stagger;

                Sequence sequence = DOTween.Sequence();
                sequence.SetTarget(chip);

                sequence.AppendInterval(delay);

                sequence.Append(
                    canvasGroup.DOFade(1f, 0.08f));

                sequence.Join(
                    rect.DOAnchorPos(
                            betBox + heldOffset,
                            duration)
                        .SetEase(ease));

                sequence.Join(
                    rect.DOPunchScale(
                        Vector3.one * 0.06f,
                        duration * 0.5f,
                        1,
                        0.25f));

                sequence.Append(
                    canvasGroup.DOFade(0f, 0.08f));

                sequence.OnComplete(() =>
                {
                    _activeFlyingChips.Remove(chip);

                    if (chip != null)
                        Destroy(chip);

                    if (!_heldBetChipCountBySeat.ContainsKey(seatIndex))
                        _heldBetChipCountBySeat[seatIndex] = 0;

                    _heldBetChipCountBySeat[seatIndex]++;
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // BET BOXES → CENTRAL POT
        // ─────────────────────────────────────────────────────────────────────

        private void HandleCollectBetsToPot(Action onComplete)
        {
            if (!PotLandingTarget ||
                !chipPrefab ||
                !animationRoot)
            {
                onComplete?.Invoke();
                return;
            }

            int totalChips = 0;

            foreach (KeyValuePair<int, int> pair
                     in _heldBetChipCountBySeat)
            {
                totalChips += pair.Value;
            }

            if (totalChips <= 0)
            {
                onComplete?.Invoke();
                return;
            }

            Vector2 destination =
                WorldToAnchoredPosition(
                    PotLandingTarget.position);

            int started = 0;
            int completed = 0;

            foreach (KeyValuePair<int, int> pair
                     in _heldBetChipCountBySeat)
            {
                int seatIndex = pair.Key;
                int chipCount = pair.Value;

                UIPlayer playerUI =
                    GetPlayerUI(seatIndex);

                if (!playerUI)
                {
                    completed += chipCount;

                    if (completed >= totalChips)
                    {
                        FinishCollectBets(onComplete);
                    }

                    continue;
                }

                Vector2 origin =
                    WorldToAnchoredPosition(
                        playerUI.GetChipBetBoxWorldPosition());

                for (int i = 0; i < chipCount; i++)
                {
                    GameObject chip =
                        Instantiate(chipPrefab, animationRoot);

                    _activeFlyingChips.Add(chip);

                    RectTransform rect =
                        chip.transform as RectTransform;

                    if (!rect)
                    {
                        _activeFlyingChips.Remove(chip);
                        Destroy(chip);

                        completed++;

                        if (completed >= totalChips)
                            FinishCollectBets(onComplete);

                        continue;
                    }

                    CanvasGroup canvasGroup =
                        chip.GetComponent<CanvasGroup>();

                    if (!canvasGroup)
                        canvasGroup =
                            chip.AddComponent<CanvasGroup>();

                    Vector2 randomStartOffset =
                        Random.insideUnitCircle *
                        (chipSpawnRadius * 0.45f);

                    Vector2 randomEndOffset =
                        Random.insideUnitCircle *
                        (chipSpawnRadius * 0.6f);

                    rect.anchoredPosition =
                        origin + randomStartOffset;

                    rect.localScale = Vector3.one;
                    rect.localRotation = Quaternion.identity;

                    canvasGroup.alpha = 0f;

                    float delay = started * chipStagger;
                    started++;

                    Sequence sequence = DOTween.Sequence();
                    sequence.SetTarget(chip);

                    sequence.AppendInterval(delay);

                    sequence.Append(
                        canvasGroup.DOFade(1f, 0.08f));

                    sequence.Join(
                        rect.DOAnchorPos(
                                destination + randomEndOffset,
                                chipTravelDuration)
                            .SetEase(chipMoveEase));

                    sequence.Join(
                        rect.DOPunchScale(
                            Vector3.one * 0.06f,
                            chipTravelDuration * 0.5f,
                            1,
                            0.25f));

                    sequence.Append(
                        canvasGroup.DOFade(0f, 0.08f));

                    sequence.OnComplete(() =>
                    {
                        _activeFlyingChips.Remove(chip);

                        if (chip)
                            Destroy(chip);

                        completed++;

                        if (completed >= totalChips)
                            FinishCollectBets(onComplete);
                    });
                }
            }
        }

        private void FinishCollectBets(Action onComplete)
        {
            _heldBetChipCountBySeat.Clear();

            UIManager.Instance?.RefreshPotStackVisual();

            Punch(
                PotVisualTarget,
                potPunchScale);

            onComplete?.Invoke();
        }

        // ─────────────────────────────────────────────────────────────────────
        // CENTRAL POT → WINNERS
        // ─────────────────────────────────────────────────────────────────────

        private void HandlePotAwarded(
            List<int> winnerSeats,
            int pot)
        {
            if (pot <= 0 || !PotLandingTarget)
                return;

            if (winnerSeats == null || winnerSeats.Count == 0)
            {
                Debug.LogWarning(
                    "[ChipAnimationManager] Pot awarded without winners.");

                UIManager.Instance?.HidePotStackInstant();
                return;
            }

            List<int> validWinners =
                new List<int>();

            for (int i = 0; i < winnerSeats.Count; i++)
            {
                int seatIndex = winnerSeats[i];

                if (seatIndex < 0)
                    continue;

                if (validWinners.Contains(seatIndex))
                    continue;

                UIPlayer playerUI =
                    GetPlayerUI(seatIndex);

                if (!playerUI)
                {
                    Debug.LogWarning(
                        $"[ChipAnimationManager] No UIPlayer found for seat {seatIndex}.");

                    continue;
                }

                validWinners.Add(seatIndex);
            }

            if (validWinners.Count == 0)
            {
                UIManager.Instance?.HidePotStackInstant();
                return;
            }

            int totalVisualChips =
                UIManager.Instance
                    ? UIManager.Instance.GetVisiblePotCoinCount()
                    : GetVisualChipCount(
                        pot,
                        maxChipsForPotAward);

            if (totalVisualChips <= 0)
            {
                totalVisualChips =
                    GetVisualChipCount(
                        pot,
                        maxChipsForPotAward);
            }

            // Debe existir como mínimo una ficha visual por ganador.
            totalVisualChips = Mathf.Max(
                totalVisualChips,
                validWinners.Count);

            int winnerCount = validWinners.Count;

            int baseChipsPerWinner =
                totalVisualChips / winnerCount;

            int remainingChips =
                totalVisualChips % winnerCount;

            List<int> chipsPerWinner =
                new List<int>();

            int largestBurst = 0;

            for (int i = 0; i < winnerCount; i++)
            {
                int chipsForWinner =
                    baseChipsPerWinner +
                    (i < remainingChips ? 1 : 0);

                chipsPerWinner.Add(chipsForWinner);

                largestBurst = Mathf.Max(
                    largestBurst,
                    chipsForWinner);
            }

            float totalAnimationDuration =
                winTravelDuration +
                Mathf.Max(0, largestBurst - 1) *
                winChipStagger +
                0.16f;
            
            UIManager.Instance
                ?.HidePotStackAnimatedOverDuration(
                    totalAnimationDuration);

            Vector3 fromWorld =
                PotLandingTarget.position;

            int completedBursts = 0;

            for (int i = 0; i < winnerCount; i++)
            {
                int winnerSeat =
                    validWinners[i];

                int chipCount =
                    chipsPerWinner[i];

                UIPlayer winnerUI =
                    GetPlayerUI(winnerSeat);

                if (!winnerUI)
                {
                    completedBursts++;
                    continue;
                }

                Vector3 toWorld =
                    winnerUI.GetChipBetStartWorldPosition();

                int capturedWinnerSeat =
                    winnerSeat;

                PlayChipBurst(
                    fromWorld,
                    toWorld,
                    chipCount,
                    winTravelDuration,
                    winChipStagger,
                    winMoveEase,
                    () =>
                    {
                        PunchTargetSeat(
                            capturedWinnerSeat);

                        completedBursts++;

                        if (completedBursts >= winnerCount)
                        {
                            UIManager.Instance
                                ?.HidePotStackInstant();
                        }
                    });
            }
        }

        private void PlayChipBurst(
            Vector3 fromWorld,
            Vector3 toWorld,
            int chipCount,
            float duration,
            float stagger,
            Ease ease,
            TweenCallback onComplete = null,
            TweenCallback onEachChipStart = null)
        {
            if (!chipPrefab ||
                !animationRoot ||
                chipCount <= 0)
            {
                onComplete?.Invoke();
                return;
            }

            Vector2 from =
                WorldToAnchoredPosition(fromWorld);

            Vector2 to =
                WorldToAnchoredPosition(toWorld);

            int completed = 0;

            for (int i = 0; i < chipCount; i++)
            {
                GameObject chip =
                    Instantiate(chipPrefab, animationRoot);

                _activeFlyingChips.Add(chip);

                RectTransform rect =
                    chip.transform as RectTransform;

                if (rect == null)
                {
                    _activeFlyingChips.Remove(chip);
                    Destroy(chip);

                    completed++;

                    if (completed >= chipCount)
                        onComplete?.Invoke();

                    continue;
                }

                CanvasGroup canvasGroup =
                    chip.GetComponent<CanvasGroup>();

                if (canvasGroup == null)
                    canvasGroup =
                        chip.AddComponent<CanvasGroup>();

                Vector2 randomEndOffset =
                    Random.insideUnitCircle *
                    (chipSpawnRadius * 0.6f);

                rect.anchoredPosition = from;
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;

                canvasGroup.alpha = 0f;

                float delay = i * stagger;

                Sequence sequence = DOTween.Sequence();
                sequence.SetTarget(chip);

                sequence.AppendInterval(delay);

                sequence.AppendCallback(() =>
                {
                    onEachChipStart?.Invoke();
                });

                sequence.Append(
                    canvasGroup.DOFade(1f, 0.08f));

                sequence.Join(
                    rect.DOAnchorPos(
                            to + randomEndOffset,
                            duration)
                        .SetEase(ease));

                sequence.Join(
                    rect.DOPunchScale(
                        Vector3.one * 0.06f,
                        duration * 0.5f,
                        1,
                        0.25f));

                sequence.Append(
                    canvasGroup.DOFade(0f, 0.08f));

                sequence.OnComplete(() =>
                {
                    completed++;

                    _activeFlyingChips.Remove(chip);

                    if (chip != null)
                        Destroy(chip);

                    if (completed >= chipCount)
                        onComplete?.Invoke();
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private int GetVisualChipCount(
            int amount,
            int max)
        {
            if (amount <= 0)
                return 0;

            int count =
                Mathf.CeilToInt(
                    Mathf.Log10(amount + 1f) * 2f);

            return Mathf.Clamp(
                count,
                1,
                max);
        }

        private Vector2 WorldToAnchoredPosition(
            Vector3 worldPosition)
        {
            if (animationRoot == null)
                return Vector2.zero;

            Vector2 screenPoint =
                RectTransformUtility.WorldToScreenPoint(
                    CanvasCamera,
                    worldPosition);

            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    animationRoot,
                    screenPoint,
                    CanvasCamera,
                    out Vector2 localPoint);

            return localPoint;
        }

        private UIPlayer GetPlayerUI(int seatIndex)
        {
            SeatController seat =
                GameManager.Instance
                    ?.GetSeat(seatIndex);

            if (seat == null)
                return null;

            return seat.GetComponent<UIPlayer>();
        }

        private void Punch(
            RectTransform target,
            float amount)
        {
            if (target == null)
                return;

            target.DOKill();
            target.localScale = Vector3.one;

            target.DOPunchScale(
                Vector3.one * amount,
                punchDuration,
                1,
                0.4f);
        }

        private void PunchTargetSeat(int seatIndex)
        {
            UIPlayer playerUI =
                GetPlayerUI(seatIndex);

            if (playerUI == null)
                return;

            RectTransform rect =
                playerUI.transform as RectTransform;

            if (rect != null)
            {
                Punch(
                    rect,
                    targetPunchScale);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // CLEANUP
        // ─────────────────────────────────────────────────────────────────────

        private void HandleNewHand(int handNumber)
        {
            ClearFlyingChips();
        }

        private void HandleClearBoardAndPot()
        {
            ClearFlyingChips();
        }

        private void ClearFlyingChips()
        {
            for (int i = _activeFlyingChips.Count - 1;
                 i >= 0;
                 i--)
            {
                GameObject chip =
                    _activeFlyingChips[i];

                if (chip == null)
                    continue;

                DOTween.Kill(chip);
                Destroy(chip);
            }

            _activeFlyingChips.Clear();
            _heldBetChipCountBySeat.Clear();
        }
    }
}