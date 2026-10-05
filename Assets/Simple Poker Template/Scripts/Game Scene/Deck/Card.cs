using UnityEngine;
using UnityEngine.UI;

namespace SimplePoker.Logic
{

    /// <summary>
    /// Class representing a playing card with a suit, value, and visual appearance.
    /// </summary>
    public class Card : MonoBehaviour
    {
       public enum SUIT
        {
            DIAMONDS = 1,
            SPADES,
            HEART,
            CLUBS
        }

        public enum VALUE
        {
            ACE = 14, TWO = 2, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, TEN, JACK, QUEEN, KING, NOTHING = 0
        }

        public SUIT Suit;
        public VALUE Value;

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Image cardImage;
        private Sprite cardFront;
        private Sprite cardBack;

        public bool ShowCard;
        public bool HideCard;

        [SerializeField] private GameObject outlineCard;

        /// <summary>
        /// Sets up the card with the specified suit, value, card back sprite, and card front sprite.
        /// </summary>
        /// <param name="suit">The suit of the card.</param>
        /// <param name="value">The value of the card.</param>
        /// <param name="cardBackSprite">The sprite for the card back.</param>
        /// <param name="cardFrontSprite">The sprite for the card front.</param>
        public void Setup(SUIT suit, VALUE value, Sprite cardBackSprite, Sprite cardFrontSprite)
        {
            Suit = suit;
            Value = value;
            cardBack = cardBackSprite;
            cardFront = cardFrontSprite;
        }

        /// <summary>
        /// Sets the name of the game object based on the card's value and suit.
        /// </summary>
        private void Start()
        {
            gameObject.name = $"{Value}_{Suit}";
        }

        /// <summary>
        /// Updates the card's appearance based on the show state.
        /// </summary>
        private void Update()
        {
            //CardShowState(ShowCard);
        }

        /// <summary>
        /// Sets the card's display state.
        /// </summary>
        /// <param name="state">The state to set (true for showing the card, false for hiding it).</param>
        public void CardShowState(bool state)
        {
            if (HideCard)
            {
                cardImage.sprite = cardBack;
                return;
            }

            if (state)
                cardImage.sprite = cardFront;
            else
                cardImage.sprite = cardBack;
        }

        /// <summary>
        /// Shows an outline around the card with the specified color.
        /// </summary>
        /// <param name="outlineColor">The color of the outline.</param>
        public void ShowOutline(Color outlineColor)
        {
            outlineCard.transform.GetChild(0).GetComponent<Image>().color = outlineColor;
            outlineCard.SetActive(true);
        }

        /// <summary>
        /// Disables the outline around the card.
        /// </summary>
        public void DisableOutline()
        {
            outlineCard.SetActive(false);
        }

    }
}