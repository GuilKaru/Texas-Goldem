using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TexasHoldem
{
    public class BanterBar : MonoBehaviour
    {
        public static BanterBar Instance { get; private set; }

        // ── Inspector refs ─────────────────────────────────────────────────

        [Header("Left Side (current / most recent speaker)")]
        [SerializeField] private Image           leftAvatarImage;
        [SerializeField] private TextMeshProUGUI leftBubbleText;
        [SerializeField] private RectTransform   leftBubbleRoot;
        [SerializeField] private GameObject      leftSideRoot;

        [Header("Right Side (previous speaker / receiver)")]
        [SerializeField] private Image           rightAvatarImage;
        [SerializeField] private TextMeshProUGUI rightBubbleText;
        [SerializeField] private RectTransform   rightBubbleRoot;
        [SerializeField] private GameObject      rightSideRoot;

        [Header("Bubble Background Images")]
        [Tooltip("Image component left bubble")]
        [SerializeField] private Image leftBubbleImage;

        [Tooltip("Image component right bubble")]
        [SerializeField] private Image rightBubbleImage;

        [Header("Left Bubble Sprites")]
        [SerializeField] private Sprite leftBubbleOneLine;
        [SerializeField] private Sprite leftBubbleTwoLines;
        [SerializeField] private Sprite leftBubbleThreeLines;

        [Header("Right Bubble Sprites")]
        [SerializeField] private Sprite rightBubbleOneLine;
        [SerializeField] private Sprite rightBubbleTwoLines;
        [SerializeField] private Sprite rightBubbleThreeLines;

        [Header("Left TMP Sizes")]
        [Tooltip("1 Line")]
        [SerializeField] private Vector2 leftTextSizeOneLine = new Vector2(210f, 28f);

        [Tooltip("2 Lines")]
        [SerializeField] private Vector2 leftTextSizeTwoLines = new Vector2(210f, 48f);

        [Tooltip("3 Lines")]
        [SerializeField] private Vector2 leftTextSizeThreeLines = new Vector2(210f, 68f);

        [Header("Right TMP Sizes")]
        [Tooltip("1 Line")]
        [SerializeField] private Vector2 rightTextSizeOneLine = new Vector2(210f, 28f);

        [Tooltip("2 Lines")]
        [SerializeField] private Vector2 rightTextSizeTwoLines = new Vector2(210f, 48f);

        [Tooltip("3 Lines")]
        [SerializeField] private Vector2 rightTextSizeThreeLines = new Vector2(210f, 68f);

        [Header("Bubble Character Detection")]
        [SerializeField] private int oneLineMaxCharacters = 38;
        
        [SerializeField] private int twoLinesMaxCharacters = 72;

        [Header("Empty State")]
        [Tooltip("Shown when no banter has fired yet. Hidden on first banter.")]
        [SerializeField] private GameObject emptyStateRoot;

        [Header("Punch Animation")]
        [SerializeField] private float punchScale    = 0.08f;
        [SerializeField] private float punchDuration = 0.18f;

        // ── Runtime ────────────────────────────────────────────────────────

        private int    _leftSeatIndex       = -1;
        private int    _rightSeatIndex      = -1;
        private string _leftText            = "";
        private string _rightText           = "";
        private bool   _hasReceivedBanter   = false;
        private bool   _nextMessageGoesLeft = true;

        // ── Unity lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            PokerTableEvents.OnBanterReceived   += HandleBanterReceived;
            PokerTableEvents.OnMatchInitialized += HandleMatchInitialized;
            PokerTableEvents.OnNewHand          += HandleNewHand;

            ResetToEmpty();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            PokerTableEvents.OnBanterReceived   -= HandleBanterReceived;
            PokerTableEvents.OnMatchInitialized -= HandleMatchInitialized;
            PokerTableEvents.OnNewHand          -= HandleNewHand;
        }

        // ── Public API ─────────────────────────────────────────────────────

        public void ShowBanter(int speakerSeat, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (!_hasReceivedBanter)
            {
                _hasReceivedBanter = true;

                if (emptyStateRoot)
                    emptyStateRoot.SetActive(false);
            }

            if (_nextMessageGoesLeft)
            {
                _leftSeatIndex = speakerSeat;
                _leftText = text;

                if (leftSideRoot)
                    leftSideRoot.SetActive(true);

                ApplyLeft(_leftSeatIndex, _leftText);
                PunchBubble(leftBubbleRoot);
            }
            else
            {
                _rightSeatIndex = speakerSeat;
                _rightText = text;

                SetRightVisible(true);

                ApplyRight(_rightSeatIndex, _rightText);
                PunchBubble(rightBubbleRoot);
            }

            _nextMessageGoesLeft = !_nextMessageGoesLeft;
        }

        // ── Event handlers ─────────────────────────────────────────────────

        private void HandleBanterReceived(int seatIndex, string text)
        {
            ShowBanter(seatIndex, text);
        }

        private void HandleMatchInitialized()
        {
            ResetToEmpty();
        }

        private void HandleNewHand(int handIndex)
        {
            ResetToEmpty();
        }

        // ── Apply UI ───────────────────────────────────────────────────────

        private void ApplyLeft(int seatIndex, string text)
        {
            ApplyBubbleVisuals(
                textComponent: leftBubbleText,
                bubbleImage: leftBubbleImage,
                text: text,
                isLeft: true
            );

            Sprite avatar = GetAvatarSprite(seatIndex);

            if (leftAvatarImage != null && avatar != null)
                leftAvatarImage.sprite = avatar;

            if (leftSideRoot != null)
                leftSideRoot.SetActive(true);
        }

        private void ApplyRight(int seatIndex, string text)
        {
            ApplyBubbleVisuals(
                textComponent: rightBubbleText,
                bubbleImage: rightBubbleImage,
                text: text,
                isLeft: false
            );

            Sprite avatar = GetAvatarSprite(seatIndex);

            if (rightAvatarImage != null && avatar != null)
                rightAvatarImage.sprite = avatar;
        }

        private void ApplyBubbleVisuals(
            TextMeshProUGUI textComponent,
            Image bubbleImage,
            string text,
            bool isLeft)
        {
            if (!textComponent || !bubbleImage)
                return;

            int lineCount = GetBubbleLineCountByCharacters(text);

            Debug.Log(
                $"BANTER {(isLeft ? "LEFT" : "RIGHT")} | " +
                $"Chars: {text.Trim().Length} | " +
                $"LineCount: {lineCount} | " +
                $"BubbleImage: {bubbleImage.name} | " +
                $"Text: {text}"
            );

            Sprite selectedSprite = GetBubbleSprite(lineCount, isLeft);
            Vector2 selectedTextSize = GetTextSize(lineCount, isLeft);

            if (selectedSprite)
                bubbleImage.sprite = selectedSprite;

            RectTransform textRect = textComponent.rectTransform;
            textRect.sizeDelta = selectedTextSize;

            textComponent.enableWordWrapping = true;
            textComponent.overflowMode = TextOverflowModes.Overflow;
            textComponent.SetText(text);

            Canvas.ForceUpdateCanvases();
            textComponent.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
        }

        private int GetBubbleLineCountByCharacters(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 1;

            int characterCount = text.Trim().Length;

            int lineCount;

            if (characterCount <= oneLineMaxCharacters)
            {
                lineCount = 1;
            }
            else if (characterCount <= twoLinesMaxCharacters)
            {
                lineCount = 2;
            }
            else
            {
                lineCount = 3;
            }

            Debug.Log(
                $"BANTER CHARACTER CHECK | " +
                $"Chars: {characterCount} | " +
                $"OneMax: {oneLineMaxCharacters} | " +
                $"TwoMax: {twoLinesMaxCharacters} | " +
                $"Result Lines: {lineCount}"
            );

            return lineCount;
        }

        private Sprite GetBubbleSprite(int lineCount, bool isLeft)
        {
            lineCount = Mathf.Clamp(lineCount, 1, 3);

            if (isLeft)
            {
                if (lineCount == 1)
                    return leftBubbleOneLine;

                if (lineCount == 2)
                    return leftBubbleTwoLines;

                return leftBubbleThreeLines;
            }

            if (lineCount == 1)
                return rightBubbleOneLine;

            if (lineCount == 2)
                return rightBubbleTwoLines;

            return rightBubbleThreeLines;
        }

        private Vector2 GetTextSize(int lineCount, bool isLeft)
        {
            lineCount = Mathf.Clamp(lineCount, 1, 3);

            if (isLeft)
            {
                if (lineCount <= 1)
                    return leftTextSizeOneLine;

                if (lineCount == 2)
                    return leftTextSizeTwoLines;

                return leftTextSizeThreeLines;
            }

            if (lineCount <= 1)
                return rightTextSizeOneLine;

            if (lineCount == 2)
                return rightTextSizeTwoLines;

            return rightTextSizeThreeLines;
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private void SetRightVisible(bool visible)
        {
            if (rightSideRoot)
                rightSideRoot.SetActive(visible);
        }

        private Sprite GetAvatarSprite(int seatIndex)
        {
            SeatController seat = GameManager.Instance?.GetSeatController(seatIndex);
            return seat?.AvatarSprite;
        }

        private void PunchBubble(RectTransform rt)
        {
            if (!rt)
                return;

            rt.DOKill();
            rt.localScale = Vector3.one;

            rt.DOPunchScale(Vector3.one * punchScale, punchDuration, 1, 0.5f)
              .SetEase(Ease.OutQuad);
        }

        private void ResetToEmpty()
        {
            if (leftBubbleRoot)
            {
                leftBubbleRoot.DOKill();
                leftBubbleRoot.localScale = Vector3.one;
            }

            if (rightBubbleRoot)
            {
                rightBubbleRoot.DOKill();
                rightBubbleRoot.localScale = Vector3.one;
            }

            _leftSeatIndex       = -1;
            _rightSeatIndex      = -1;
            _leftText            = "";
            _rightText           = "";
            _hasReceivedBanter   = false;
            _nextMessageGoesLeft = true;

            if (leftBubbleText)
            {
                leftBubbleText.SetText("");
                leftBubbleText.rectTransform.sizeDelta = leftTextSizeOneLine;
            }

            if (rightBubbleText)
            {
                rightBubbleText.SetText("");
                rightBubbleText.rectTransform.sizeDelta = rightTextSizeOneLine;
            }

            if (leftSideRoot)
                leftSideRoot.SetActive(false);

            if (rightSideRoot)
                rightSideRoot.SetActive(false);

            if (emptyStateRoot)
                emptyStateRoot.SetActive(true);
        }
    }
}