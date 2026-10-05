using System;
using System.Collections.Generic;
using DG.Tweening;
using SimplePoker.Data;
using SimplePoker.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace TexasHoldem
{
    public class VisualController : MonoBehaviour
    {
        // ── Inspector refs ────────────────────────────────────────────────────

        [Header("Portrait Frame")]
        [SerializeField] private Image        turnFrameImage;
        [SerializeField] private Image        backgroundPortraitImage;
        [SerializeField] private GameObject   myTurnHighlightEffect;
        [SerializeField] private Image        myTurnHaloRotateImage;

        [Header("Emotion Border")]
        [Tooltip("The Image that shows the emotion border around the portrait. " +
                 "Sprite is swapped instantly when emotion changes.")]
        [SerializeField] private Image        emotionBorderImage;

        [Header("Winner")]
        [SerializeField] private GameObject winningPanel;
        [SerializeField] private GameObject   crownObject;
        [SerializeField] private GameObject   winnerEffect;
        [SerializeField] private Image        backgroundWinnerImage;
        [SerializeField] private Image        backgroundWinnerLightImage;
        [SerializeField] private Image        winnerProfilePictureImage;
        [SerializeField] private Image        winnerProfilePictureRingImage;
        [SerializeField] private Image        winnerHaloRotateImage;

        [Header("Status Overlay")]
        [SerializeField] private GameObject   foldedOverlay;
        [SerializeField] private CanvasGroup  seatCanvasGroup;

        [Header("Action Label")]
        [SerializeField] private RectTransform actionLabelTransform;

        [Header("Deal Settings")]
        [SerializeField] private float dealStagger = 0.15f;

        [Header("Animation Timings")]
        [SerializeField] private float haloRotateSpeed    = 90f;
        [SerializeField] private float foldFadeDuration   = 0.3f;
        [SerializeField] private float crownScaleDuration = 0.3f;
        [SerializeField] private float actionPunchDuration = 0.12f;
        
        [Header("Turn Halo Show / Hide")]
        [SerializeField] private float turnHaloShowDuration = 0.25f;
        [SerializeField] private float turnHaloHideDuration = 0.25f;
        [SerializeField] private Ease turnHaloShowEase = Ease.OutBack;
        [SerializeField] private Ease turnHaloHideEase = Ease.InBack;
        [SerializeField] private Vector2 turnHaloHiddenSize = Vector2.zero;
        
        [Header("Turn Halo Pulse")]
        [SerializeField] private Vector2 turnHaloPulseSizeIncrease = new Vector2(20f, 20f);
        [SerializeField] private float turnHaloPulseDuration = 0.45f;
        [SerializeField] private Ease turnHaloPulseEase = Ease.InOutSine;
        
        [Header("Winner Light")]
        [SerializeField] private Vector2 winnerLightHiddenSize = Vector2.zero;
        [SerializeField] private float winnerLightShowDuration = 0.45f;
        [SerializeField] private float winnerLightPulseDuration = 0.75f;
        [SerializeField] private Vector2 winnerLightPulseSizeIncrease = new Vector2(25f, 25f);
        [SerializeField] private Ease winnerLightShowEase = Ease.OutSine;
        [SerializeField] private Ease winnerLightPulseEase = Ease.InOutSine;
        

        [Header("Auto Highlighted Objects")]
        [SerializeField] private string            highlightedNameToken        = "Highlighted";
        [SerializeField] private bool              autoFindHighlightedObjects  = true;
        [SerializeField] private List<GameObject>  defaultObjectsToHideOnTurn  = new List<GameObject>();

        // ── Emotion sprites — id 0–6, index matches emotion id ───────────────
        // Assign 7 sprites in the Inspector. Index 3 = Neutral.

        [Header("Emotion Border Sprites (index = emotion id, 3 = Neutral)")]
        [SerializeField] private Sprite[] emotionSprites = new Sprite[7];

        // ── Private state ─────────────────────────────────────────────────────

        private PokerGameAssetData _asset;
        private Tween _haloTween;
        private Tween _haloPulseTween;
        private Tween _haloShowHideTween;
        private Tween _winnerHaloTween;

        private Vector2 _turnHaloDefaultSize;
        private bool _hasCachedTurnHaloDefaultSize = false;
        private bool _isActiveTurnVisual = false;
        private Tween              _winnerLightShowTween;
        private Tween              _winnerLightPulseTween;
        private Tween              _seatFadeTween;

        private Vector2            _winnerLightDefaultSize;
        private bool               _hasCachedWinnerLightDefaultSize = false;
        private readonly List<GameObject>  _highlightedObjects  = new List<GameObject>();

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
            ResetVisuals();
            CacheHighlightedObjects();
        }

        private void OnDestroy()
        
        {
            _winnerLightShowTween?.Kill();
            _winnerLightPulseTween?.Kill();
            _seatFadeTween?.Kill();
            _haloTween?.Kill();
            _haloPulseTween?.Kill();
            _haloShowHideTween?.Kill();
            _winnerHaloTween?.Kill();
        }

        // ── Active Turn ───────────────────────────────────────────────────────

        public void SetActiveTurn(bool isActive)
        {
            if (_isActiveTurnVisual == isActive)
                return;

            _isActiveTurnVisual = isActive;

            if (turnFrameImage == null) return;

            turnFrameImage.gameObject.SetActive(isActive);

            if (myTurnHighlightEffect != null)
                myTurnHighlightEffect.SetActive(isActive);

            SetDefaultObjects(!isActive);
            SetHighlightedObjects(isActive);

            if (myTurnHaloRotateImage != null)
            {
                if (isActive)
                    ShowTurnHaloSmooth();
                else
                    HideTurnHaloSmooth();
            }
        }

        // ── Fold ──────────────────────────────────────────────────────────────

        public void PlayFold(Action onComplete = null)
        {
            // Cards live in RosterRow — compact node has no cards to animate.
            // Just signal completion immediately so callers don't hang.
            onComplete?.Invoke();
        }

        // ── Winner ────────────────────────────────────────────────────────────

        public void PlayWinner()
        {
            Debug.Log($"[VisualController] PlayWinner called on {gameObject.name}");

            if (winningPanel != null)
                winningPanel.SetActive(true);

            if (backgroundWinnerImage != null)
                backgroundWinnerImage.gameObject.SetActive(true);

            if (backgroundWinnerLightImage != null)
                backgroundWinnerLightImage.gameObject.SetActive(true);

            if (crownObject != null)
            {
                crownObject.SetActive(true);
                crownObject.transform.localScale = Vector3.zero;
                crownObject.transform
                    .DOScale(Vector3.one, crownScaleDuration)
                    .SetEase(Ease.OutBounce);
            }

            if (winnerEffect != null)
            {
                winnerEffect.SetActive(true);
                winnerEffect.transform.localScale = Vector3.zero;
                winnerEffect.transform
                    .DOScale(Vector3.one, crownScaleDuration + 0.2f)
                    .SetEase(Ease.OutBounce);
            }

            if (winnerProfilePictureRingImage != null)
                winnerProfilePictureRingImage.gameObject.SetActive(true);

            if (winnerProfilePictureImage != null)
                winnerProfilePictureImage.gameObject.SetActive(true);

            if (backgroundPortraitImage != null)
                backgroundPortraitImage.gameObject.SetActive(false);
        }

        // ── Action Label Punch ────────────────────────────────────────────────

        public void PunchActionLabel()
        {
            if (actionLabelTransform == null) return;
            actionLabelTransform.localScale = Vector3.one;
            actionLabelTransform.DOPunchScale(Vector3.one * 0.3f, actionPunchDuration, 1, 0.5f);
        }

        // ── Winner / Non-winner compact node visual ───────────────────────────
        
        /// Dims the compact node portrait to indicate this seat did not win.
        /// Called by SeatController.HighlightWinningCards when isWinner = false.
        public void DimAsNonWinner()
        {
            if (seatCanvasGroup != null)
                seatCanvasGroup.DOFade(0.35f, 0.35f).SetEase(Ease.OutQuad);
        }

        // ── Emotion Border ────────────────────────────────────────────────────
        
        /// Swaps the emotion border sprite to match emotionId (0–6).
        /// Id 3 = Neutral. Called by SeatController on OnEmotionChanged.
        public void SetEmotionById(int emotionId)
        {
            if (emotionBorderImage == null) return;

            Sprite target = GetEmotionSprite(emotionId);
            if (target == null) return;

            emotionBorderImage.sprite = target;
            emotionBorderImage.gameObject.SetActive(true);
        }

        // ── Reset ─────────────────────────────────────────────────────────────

        public void ResetVisuals()
        {
            
            
            _isActiveTurnVisual = false;

            _haloTween?.Kill();
            _haloTween = null;

            _haloPulseTween?.Kill();
            _haloPulseTween = null;

            _haloShowHideTween?.Kill();
            _haloShowHideTween = null;

            _winnerHaloTween?.Kill();
            _winnerHaloTween = null;
            
            ResetWinnerLightInstant();
            StopTurnHaloPulse(true);
            SetDefaultObjects(true);
            SetHighlightedObjects(false);

           // if (myTurnHighlightEffect  != null) myTurnHighlightEffect.SetActive(false);
            if (crownObject            != null) crownObject.SetActive(false);
            if (winnerEffect           != null) winnerEffect.SetActive(false);
            if (foldedOverlay          != null) foldedOverlay.SetActive(false);
            if (backgroundWinnerImage  != null) backgroundWinnerImage.gameObject.SetActive(false);
            if (myTurnHaloRotateImage != null)
            {
                CacheTurnHaloDefaultSize();
                myTurnHaloRotateImage.rectTransform.sizeDelta = turnHaloHiddenSize;
                myTurnHaloRotateImage.gameObject.SetActive(false);
            }

            if (winnerHaloRotateImage != null)
                winnerHaloRotateImage.transform.localRotation = Quaternion.identity;
            
            if (winningPanel != null)
                winningPanel.SetActive(false);

            if (backgroundWinnerImage != null)
                backgroundWinnerImage.gameObject.SetActive(false);

            if (backgroundWinnerLightImage != null)
                backgroundWinnerLightImage.gameObject.SetActive(false);

            // Keep alpha at whatever FadeOutSeat left it —
            // FadeInSeat() is called by OnFadeInTableNodes AFTER Phase-0
            // positions and UI are fully reset, preventing ghosting.
            // If no fade-out occurred (first hand), snap to 1 immediately.
            if (seatCanvasGroup != null && seatCanvasGroup.alpha > 0f)
                seatCanvasGroup.alpha = 1f;

            // Snap emotion border back to Neutral sprite instantly on new hand
            if (emotionBorderImage != null)
            {
                Sprite neutral = GetEmotionSprite(3);
                if (neutral != null) emotionBorderImage.sprite = neutral;
            }
        }

        // ── Seat fade (between-hand reset animation) ─────────────────────────

        [Header("Between-Hand Seat Fade")]
        [SerializeField] private float seatFadeOutDuration = 0.5f;
        [SerializeField] private float seatFadeInDuration  = 0.4f;
        
        /// Fades the entire compact seat node to alpha 0.
        /// Called by UIPlayer when OnFadeOutTableNodes fires (end of hand).
        /// Node stays invisible while UI resets. FadeInSeat() restores it.
        public void FadeOutSeat()
        {
            if (seatCanvasGroup == null) return;
            _seatFadeTween?.Kill();
            _seatFadeTween = seatCanvasGroup
                .DOFade(0f, seatFadeOutDuration)
                .SetEase(Ease.InQuad);
        }
        
        /// Fades the compact seat node back to alpha 1.
        /// Called by UIPlayer when OnFadeInTableNodes fires — AFTER Phase-0
        /// positions and UI have been fully reset, so there is no position ghosting.
        public void FadeInSeat()
        {
            if (seatCanvasGroup == null) return;
            _seatFadeTween?.Kill();
            // Ensure we start from 0 in case alpha drifted
            seatCanvasGroup.alpha = 0f;
            _seatFadeTween = seatCanvasGroup
                .DOFade(1f, seatFadeInDuration)
                .SetEase(Ease.OutQuad);
        }

        // ── Legacy string emotion (kept for backwards compat, unused) ─────────

        public Color GetEmotionTint(string emotionalState) => Color.white;

        // ── Private helpers ───────────────────────────────────────────────────

        private Sprite GetEmotionSprite(int emotionId)
        {
            if (emotionSprites == null || emotionSprites.Length != 7)
            {
                Debug.LogWarning("[VisualController] emotionSprites array must have exactly 7 entries.");
                return null;
            }

            int clamped = Mathf.Clamp(emotionId, 0, 6);
            return emotionSprites[clamped];
        }

        private void ShowFoldedOverlay()
        {
            if (foldedOverlay != null)
                foldedOverlay.SetActive(true);

            if (seatCanvasGroup != null)
                seatCanvasGroup.DOFade(0.45f, foldFadeDuration);
        }

        private void CacheHighlightedObjects()
        {
            _highlightedObjects.Clear();
            if (!autoFindHighlightedObjects) return;

            var all = GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
            {
                if (t == null || t == transform) continue;
                if (t.name.Contains(highlightedNameToken))
                    _highlightedObjects.Add(t.gameObject);
            }

            SetHighlightedObjects(false);
        }

        private void SetHighlightedObjects(bool active)
        {
            for (int i = 0; i < _highlightedObjects.Count; i++)
            {
                var go = _highlightedObjects[i];
                if (go != null) go.SetActive(active);
            }
        }

        private void SetDefaultObjects(bool active)
        {
            for (int i = 0; i < defaultObjectsToHideOnTurn.Count; i++)
            {
                var go = defaultObjectsToHideOnTurn[i];
                if (go != null) go.SetActive(active);
            }
        }
        
        private void CacheTurnHaloDefaultSize()
        {
            if (_hasCachedTurnHaloDefaultSize)
                return;

            if (myTurnHaloRotateImage == null)
                return;

            _turnHaloDefaultSize = myTurnHaloRotateImage.rectTransform.sizeDelta;
            _hasCachedTurnHaloDefaultSize = true;
        }

        private void StartTurnHaloPulse()
        {
            if (myTurnHaloRotateImage == null)
                return;

            CacheTurnHaloDefaultSize();

            RectTransform haloRect = myTurnHaloRotateImage.rectTransform;

            _haloPulseTween?.Kill();

            Vector2 targetSize = _turnHaloDefaultSize + turnHaloPulseSizeIncrease;

            _haloPulseTween = haloRect
                .DOSizeDelta(targetSize, turnHaloPulseDuration)
                .SetEase(turnHaloPulseEase)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopTurnHaloPulse(bool resetSize)
        {
            _haloPulseTween?.Kill();
            _haloPulseTween = null;

            if (!resetSize)
                return;

            if (myTurnHaloRotateImage == null)
                return;

            CacheTurnHaloDefaultSize();

            myTurnHaloRotateImage.rectTransform.sizeDelta = _turnHaloDefaultSize;
        }
        
        private void ShowTurnHaloSmooth()
        {
            if (myTurnHaloRotateImage == null)
                return;

            CacheTurnHaloDefaultSize();

            RectTransform haloRect = myTurnHaloRotateImage.rectTransform;

            _haloShowHideTween?.Kill();
            _haloShowHideTween = null;

            myTurnHaloRotateImage.gameObject.SetActive(true);

            haloRect.localRotation = Quaternion.identity;
            haloRect.sizeDelta = turnHaloHiddenSize;

            _haloShowHideTween = haloRect
                .DOSizeDelta(_turnHaloDefaultSize, turnHaloShowDuration)
                .SetEase(turnHaloShowEase)
                .OnComplete(() =>
                {
                    StartTurnHaloRotation();
                    StartTurnHaloPulse();
                });
        }

        private void HideTurnHaloSmooth()
        {
            if (myTurnHaloRotateImage == null)
                return;

            CacheTurnHaloDefaultSize();

            RectTransform haloRect = myTurnHaloRotateImage.rectTransform;

            _haloTween?.Kill();
            _haloTween = null;

            _haloPulseTween?.Kill();
            _haloPulseTween = null;

            _haloShowHideTween?.Kill();
            _haloShowHideTween = null;

            _haloShowHideTween = haloRect
                .DOSizeDelta(turnHaloHiddenSize, turnHaloHideDuration)
                .SetEase(turnHaloHideEase)
                .OnComplete(() =>
                {
                    if (myTurnHaloRotateImage != null)
                        myTurnHaloRotateImage.gameObject.SetActive(true);
                });
        }

        private void StartTurnHaloRotation()
        {
            if (myTurnHaloRotateImage == null)
                return;

            RectTransform haloRect = myTurnHaloRotateImage.rectTransform;

            _haloTween?.Kill();
            _haloTween = haloRect
                .DOLocalRotate(
                    new Vector3(0f, 0f, -360f),
                    360f / haloRotateSpeed,
                    RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
        }
        
        private void CacheWinnerLightDefaultSize()
        {
            if (_hasCachedWinnerLightDefaultSize)
                return;

            if (backgroundWinnerLightImage == null)
                return;

            _winnerLightDefaultSize = backgroundWinnerLightImage.rectTransform.sizeDelta;
            _hasCachedWinnerLightDefaultSize = true;
        }

        private void PlayWinnerLight()
        {
            if (backgroundWinnerLightImage == null)
                return;

            CacheWinnerLightDefaultSize();

            RectTransform lightRect = backgroundWinnerLightImage.rectTransform;

            _winnerLightShowTween?.Kill();
            _winnerLightShowTween = null;

            _winnerLightPulseTween?.Kill();
            _winnerLightPulseTween = null;

            backgroundWinnerLightImage.gameObject.SetActive(true);

            // Start visually hidden, then grow into normal size.
            lightRect.sizeDelta = winnerLightHiddenSize;

            _winnerLightShowTween = lightRect
                .DOSizeDelta(_winnerLightDefaultSize, winnerLightShowDuration)
                .SetEase(winnerLightShowEase)
                .OnComplete(() =>
                {
                    StartWinnerLightPulse();
                });
        }

        private void StartWinnerLightPulse()
        {
            if (backgroundWinnerLightImage == null)
                return;

            CacheWinnerLightDefaultSize();

            RectTransform lightRect = backgroundWinnerLightImage.rectTransform;
            Vector2 targetSize = _winnerLightDefaultSize + winnerLightPulseSizeIncrease;

            _winnerLightPulseTween?.Kill();

            _winnerLightPulseTween = lightRect
                .DOSizeDelta(targetSize, winnerLightPulseDuration)
                .SetEase(winnerLightPulseEase)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void ResetWinnerLightInstant()
        {
            _winnerLightShowTween?.Kill();
            _winnerLightShowTween = null;

            _winnerLightPulseTween?.Kill();
            _winnerLightPulseTween = null;

            if (backgroundWinnerLightImage == null)
                return;

            CacheWinnerLightDefaultSize();

            backgroundWinnerLightImage.rectTransform.sizeDelta = winnerLightHiddenSize;
        }
    }
}