using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TexasHoldem
{
    public class UIPotStack : MonoBehaviour
    {
        [Header("Pot Chip Visuals")]
        [Tooltip("Manual order: first chip to appear -> last chip to appear.")]
        [SerializeField] private List<GameObject> potCoinsBottomToTop = new List<GameObject>();

        [Header("Animation")]
        [SerializeField] private bool useFloor = false;
        [SerializeField] private bool animateChanges = true;
        [SerializeField] private float coinRevealDelay = 0.045f;
        [SerializeField] private float coinHideDelay = 0.035f;
        [SerializeField] private float coinScaleDuration = 0.12f;

        private Coroutine _coinVisualCoroutine;
        private int _lastVisibleCoinCount = -1;

        public int MaxCoins => potCoinsBottomToTop != null ? potCoinsBottomToTop.Count : 0;
        public int VisibleCoinCount
        {
            get
            {
                if (potCoinsBottomToTop == null)
                    return 0;

                int count = 0;

                for (int i = 0; i < potCoinsBottomToTop.Count; i++)
                {
                    GameObject coin = potCoinsBottomToTop[i];

                    if (coin != null && coin.activeSelf)
                        count++;
                }

                return count;
            }
        }

        private void Awake()
        {
            HideAllInstant();
        }

        private void OnEnable()
        {
            PokerTableEvents.OnClearBoardAndPot += HandleClearBoardAndPot;
            PokerTableEvents.OnNewHand += HandleNewHand;
        }

        private void OnDisable()
        {
            PokerTableEvents.OnClearBoardAndPot -= HandleClearBoardAndPot;
            PokerTableEvents.OnNewHand -= HandleNewHand;
        }

        public void UpdatePotVisual(int pot, int totalChipsInGame)
        {
            if (potCoinsBottomToTop == null || potCoinsBottomToTop.Count == 0)
                return;

            totalChipsInGame = Mathf.Max(1, totalChipsInGame);

            int chipsPerCoin = Mathf.CeilToInt(totalChipsInGame / (float)potCoinsBottomToTop.Count);
            int coinsOn = Mathf.CeilToInt(pot / (float)chipsPerCoin);

            if (pot <= 0)
                coinsOn = 0;

            coinsOn = Mathf.Clamp(coinsOn, 0, potCoinsBottomToTop.Count);

            if (_lastVisibleCoinCount < 0)
            {
                SetCoinVisualInstant(coinsOn);
                _lastVisibleCoinCount = coinsOn;
                return;
            }

            if (_coinVisualCoroutine != null)
            {
                StopCoroutine(_coinVisualCoroutine);
                _coinVisualCoroutine = null;
            }

            if (!animateChanges || coinsOn == _lastVisibleCoinCount)
            {
                SetCoinVisualInstant(coinsOn);
                _lastVisibleCoinCount = coinsOn;
                return;
            }

            if (coinsOn > _lastVisibleCoinCount)
                _coinVisualCoroutine = StartCoroutine(AnimateCoinGain(_lastVisibleCoinCount, coinsOn));
            else
                _coinVisualCoroutine = StartCoroutine(AnimateCoinLoss(_lastVisibleCoinCount, coinsOn));
        }

        public void HideAllAnimated()
        {
            if (_coinVisualCoroutine != null)
            {
                StopCoroutine(_coinVisualCoroutine);
                _coinVisualCoroutine = null;
            }

            int fromCount = Mathf.Max(0, _lastVisibleCoinCount);

            _coinVisualCoroutine = StartCoroutine(AnimateCoinLoss(fromCount, 0));
        }

        public void HideAllInstant()
        {
            if (_coinVisualCoroutine != null)
            {
                StopCoroutine(_coinVisualCoroutine);
                _coinVisualCoroutine = null;
            }

            SetCoinVisualInstant(0);
            _lastVisibleCoinCount = 0;
        }

        private void SetCoinVisualInstant(int coinsOn)
        {
            coinsOn = Mathf.Clamp(coinsOn, 0, potCoinsBottomToTop.Count);

            for (int i = 0; i < potCoinsBottomToTop.Count; i++)
            {
                GameObject coin = potCoinsBottomToTop[i];
                if (coin == null) continue;

                coin.transform.DOKill();
                coin.transform.localScale = Vector3.one;
                coin.SetActive(i < coinsOn);
            }
        }

        private IEnumerator AnimateCoinGain(int fromCount, int toCount)
        {
            fromCount = Mathf.Clamp(fromCount, 0, potCoinsBottomToTop.Count);
            toCount = Mathf.Clamp(toCount, 0, potCoinsBottomToTop.Count);

            SetCoinVisualInstant(fromCount);
            _lastVisibleCoinCount = fromCount;

            for (int i = fromCount; i < toCount; i++)
            {
                GameObject coin = potCoinsBottomToTop[i];

                if (coin != null)
                {
                    coin.SetActive(true);

                    // Important: keep the logical count synced immediately.
                    _lastVisibleCoinCount = Mathf.Max(_lastVisibleCoinCount, i + 1);

                    coin.transform.DOKill();
                    coin.transform.localScale = Vector3.zero;
                    coin.transform
                        .DOScale(Vector3.one, coinScaleDuration)
                        .SetEase(Ease.OutBack);
                }

                yield return new WaitForSeconds(coinRevealDelay);
            }

            _lastVisibleCoinCount = toCount;
            _coinVisualCoroutine = null;
        }

        private IEnumerator AnimateCoinLoss(int fromCount, int toCount, float delayOverride = -1f)
        {
            fromCount = Mathf.Clamp(fromCount, 0, potCoinsBottomToTop.Count);
            toCount = Mathf.Clamp(toCount, 0, potCoinsBottomToTop.Count);

            float delay = delayOverride > 0f ? delayOverride : coinHideDelay;

            for (int i = fromCount - 1; i >= toCount; i--)
            {
                GameObject coin = potCoinsBottomToTop[i];

                if (coin != null && coin.activeSelf)
                {
                    GameObject coinToHide = coin;

                    coinToHide.transform.DOKill();
                    coinToHide.transform
                        .DOScale(Vector3.zero, coinScaleDuration)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            if (coinToHide != null)
                            {
                                coinToHide.SetActive(false);
                                coinToHide.transform.localScale = Vector3.one;
                            }
                        });
                }

                yield return new WaitForSeconds(delay);
            }

            _lastVisibleCoinCount = toCount;
            _coinVisualCoroutine = null;
        }

        private void HandleClearBoardAndPot()
        {
            HideAllAnimated();
        }

        private void HandleNewHand(int handNumber)
        {
            HideAllInstant();
        }
        
        public void HideAllAnimatedOverDuration(float totalDuration)
        {
            if (_coinVisualCoroutine != null)
            {
                StopCoroutine(_coinVisualCoroutine);
                _coinVisualCoroutine = null;
            }

            int fromCount = Mathf.Max(0, _lastVisibleCoinCount);

            if (fromCount <= 0)
                return;

            float delayPerCoin = totalDuration / fromCount;
            _coinVisualCoroutine = StartCoroutine(AnimateCoinLoss(fromCount, 0, delayPerCoin));
        }
        public void HideOneTopCoinAnimated()
        {
            if (potCoinsBottomToTop == null || potCoinsBottomToTop.Count == 0)
                return;

            if (_coinVisualCoroutine != null)
            {
                StopCoroutine(_coinVisualCoroutine);
                _coinVisualCoroutine = null;
            }

            int index = -1;

            for (int i = potCoinsBottomToTop.Count - 1; i >= 0; i--)
            {
                GameObject candidate = potCoinsBottomToTop[i];

                if (candidate != null && candidate.activeSelf)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                _lastVisibleCoinCount = 0;
                return;
            }

            GameObject coin = potCoinsBottomToTop[index];

            _lastVisibleCoinCount = Mathf.Max(0, VisibleCoinCount - 1);

            coin.transform.DOKill();

            coin.transform
                .DOScale(Vector3.zero, coinScaleDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    if (coin != null)
                    {
                        coin.SetActive(false);
                        coin.transform.localScale = Vector3.one;
                    }

                    _lastVisibleCoinCount = VisibleCoinCount;
                });
        }
    }
}