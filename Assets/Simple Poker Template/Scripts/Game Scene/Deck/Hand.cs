using DG.Tweening;
using SimplePoker.Logic;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePoker.Visual
{

    /// <summary>
    /// Responsible for organizing the cards in the hand with a smooth animation.
    /// </summary>
    public class Hand : MonoBehaviour
    {
        [SerializeField] private Vector2 offset = new Vector2(1, 0);

        /// <summary>
        /// Organizes the hand of cards with a smooth animation, arranging the cards symmetrically
        /// </summary>
        /// <param name="cards">The list of cards to organize.</param>
        public void OrganizeHand(List<Card> cards)
        {
            int centerIndex = 0;
            if (cards.Count % 2 == 0)
                centerIndex = Mathf.RoundToInt(cards.Count / 2) - 1;
            else
                centerIndex = Mathf.RoundToInt(cards.Count / 2);

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < cards.Count; i++)
            {
                float posX = i - centerIndex;
                Vector3 pos = new Vector3(posX + offset.x * posX, 0, -(0.01f * i));
                sequence.Join(cards[i].transform.DOLocalMove(pos, 0.3f));
                sequence.Join(cards[i].transform.DOLocalRotate(Vector3.zero, 0.3f));
            }
        }
    }
}