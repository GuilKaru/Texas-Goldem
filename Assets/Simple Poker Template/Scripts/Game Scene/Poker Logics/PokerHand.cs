using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace SimplePoker.Logic
{
    public enum HAND
    {
        HIGH_CARD,
        ONE_PAIR,
        TWO_PAIR,
        THREE_KIND,
        STRAIGHT,
        FLUSH,
        FULL_HOUSE,
        FOUR_KIND,
        STRAIGHT_FLUSH,
        ROYAL_STRAIGHT_FLUSH
    }

    [System.Serializable]
    /// <summary>
    /// The PokerHand class represents a hand in poker, containing information about the hand ranking, 
    /// cards in the hand, and relevant high cards and kickers.
    /// </summary>
    public class PokerHand
    {
        public HAND Hand;

        public List<Card> Cards = new List<Card>();
        public Card FirstHighCardHand;
        public Card SecondHighCardHand;
        public Card HighCard;
        public Card KickerHighCard;
        public List<Card> Kickers = new List<Card>();

        public PokerHand(HAND hand, List<Card> cards, Card firstHighCardHand, Card secondHighCardHand, List<Card> kickers)
        {
            Hand = hand;
            Cards = cards;
            FirstHighCardHand = firstHighCardHand;
            SecondHighCardHand = secondHighCardHand;
            KickerHighCard = firstHighCardHand;
            Kickers = kickers;
        }
    }

    /// <summary>
    /// Class responsible for determining the best possible poker hand from a given set of cards.
    /// </summary>
    public class CheckHand
    {
        /// <summary>
        /// Determines the best possible poker hand from the given set of cards.
        /// </summary>
        /// <param name="cards">The list of cards to evaluate.</param>
        /// <returns>The best poker hand from the given cards.</returns>
        public PokerHand GetBestHand(List<Card> cards)
        {
            cards = cards.OrderBy(card => card.Value).ToList();

            // Initialize the poker hand as a high card
            PokerHand pokerHand = new PokerHand(HAND.HIGH_CARD, cards,
                cards.LastOrDefault(),
                cards.LastOrDefault(),
                cards.OrderByDescending(card => card.Value).ToList());

            // Check for the highest-ranking (Poker texas hold'em) hands in descending order of precedence
            if (HasRoyalStraightFlush(cards, out pokerHand))
            {
                return pokerHand;
            }
            else if (HasStraightFlush(cards, out pokerHand))
            {
                return pokerHand;
            }
            else if (HasFourOfAKind(cards, out pokerHand))
            {
                return pokerHand;
            }
            else if (HasFullHouse(cards, out pokerHand))
            {
                return pokerHand;
            }
            else if (HasFlush(cards, out pokerHand))
            {
                return pokerHand;
            }
            else if (HasStraight(cards, out pokerHand))
            {
                return pokerHand;
            }
            else if (HasThreeOfAKind(cards, out pokerHand))
            {
                return pokerHand;
            }
            else if (HasTwoPair(cards, out pokerHand))
            {
                return pokerHand;
            }
            else if (HasOnePair(cards, out pokerHand))
            {
                return pokerHand;
            }
            else
            {
                // If no valid combination is found, return the high card
                return new PokerHand(HAND.HIGH_CARD, cards,
                cards.LastOrDefault(),
                cards.LastOrDefault(),
                cards.OrderByDescending(card => card.Value).ToList());
            }
        }

        /// <summary>
        /// Determines the remaining cards (kickers) after forming a poker hand.
        /// </summary>
        /// <param name="allCards">The list of all cards.</param>
        /// <param name="handCards">The list of cards forming the hand.</param>
        /// <returns>The list of remaining cards (kickers).</returns>
        private List<Card> GetKickers(List<Card> allCards, List<Card> handCards)
        {
            // Remove the main hand cards from the total cards to get the kickers
            List<Card> kickers = allCards.Except(handCards).ToList();

            return kickers.OrderByDescending(card => card.Value).ToList();
        }

        /// <summary>
        /// Determines if the given cards form a Royal Straight Flush.
        /// </summary>
        /// <param name="cards">The list of cards to evaluate.</param>
        /// <param name="pokerHand">The resulting poker hand, if found.</param>
        /// <returns>True if a Royal Straight Flush is found, otherwise false.</returns>
        private bool HasRoyalStraightFlush(List<Card> cards, out PokerHand pokerHand)
        {
            List<Card> kickers = cards.OrderByDescending(c => c.Value).ToList();

            // Group the cards by suit
            var groupedBySuit = cards.GroupBy(c => c.Suit);

            foreach (var suitGroup in groupedBySuit)
            {
                // Filter only cards of the same suit
                var sameSuitCards = suitGroup.ToList();

                // Check if there are at least 5 cards to form a sequence
                if (sameSuitCards.Count >= 5)
                {
                    var sortedCards = sameSuitCards.OrderByDescending(c => c.Value).Take(5).ToList();
                    // Define the values for a Royal Straight Flush
                    var royalStraightFlushValues = new List<Card.VALUE>
                {
                    Card.VALUE.ACE,
                    Card.VALUE.KING,
                    Card.VALUE.QUEEN,
                    Card.VALUE.JACK,
                    Card.VALUE.TEN
                };


                    // Check if the cards form a Royal Straight Flush
                    bool isRoyalStraightFlush = sortedCards.Select(c => c.Value).SequenceEqual(royalStraightFlushValues);
                    if (isRoyalStraightFlush)
                    {
                        pokerHand = new PokerHand(HAND.ROYAL_STRAIGHT_FLUSH, sortedCards,
                            sortedCards.FirstOrDefault(),
                            null,
                            kickers);

                        return true;
                    }
                }
            }
            pokerHand = null;
            return false;
        }

        /// <summary>
        /// Determines if the given cards form a Straight Flush.
        /// </summary>
        /// <param name="cards">The list of cards to evaluate.</param>
        /// <param name="hand">The resulting poker hand, if found.</param>
        /// <returns>True if a Straight Flush is found, otherwise false.</returns>
        private bool HasStraightFlush(List<Card> cards, out PokerHand hand)
        {
            // Group the cards by suit
            var groupedBySuit = cards.GroupBy(c => c.Suit);
            List<Card> kickers = cards.OrderByDescending(c => c.Value).ToList();

            // The ace finalize Straight in poker
            bool hasAce = cards.Any(c => c.Value == Card.VALUE.ACE);

            foreach (var group in groupedBySuit)
            {
                // Check if any suit group has 5 or more cards
                if (group.Count() >= 5)
                {
                    cards = group.OrderByDescending(c => c.Value).ToList();
                    for (int i = 0; i <= cards.Count - 5; i++)
                    {
                        bool isStraightFlush = true;

                        // Check if the next five cards are consecutive and of the same suit
                        for (int j = i + 1; j < i + 5; j++)
                        {
                            if (cards[j].Value != cards[j - 1].Value - 1 || cards[j].Suit != cards[j - 1].Suit)
                            {
                                isStraightFlush = false;
                                break;
                            }
                        }


                        if (isStraightFlush)
                        {
                            List<Card> straightFlushCards = cards.GetRange(i, 5);

                            hand = new PokerHand(HAND.STRAIGHT_FLUSH,
                                straightFlushCards,
                                straightFlushCards.FirstOrDefault(),
                                null,
                                kickers);
                            return true;
                        }
                    }

                    // Check if the ace finalize Straight
                    if (hasAce)
                    {
                        var fourInitialCards = cards.OrderBy(c => c.Value).Take(4).ToList();
                        var lowerStraightValues = new List<Card.VALUE>
                {
                    Card.VALUE.TWO,
                    Card.VALUE.THREE,
                    Card.VALUE.FOUR,
                    Card.VALUE.FIVE
                };

                        bool isLowerStraight = fourInitialCards.Select(c => c.Value).SequenceEqual(lowerStraightValues);
                        if (isLowerStraight)
                        {

                            Card highcard = fourInitialCards.Last();
                            Card aceCard = cards.Where(c => c.Value == Card.VALUE.ACE).FirstOrDefault();

                            fourInitialCards.Add(aceCard);
                            List<Card> straightFlushCards = fourInitialCards.OrderByDescending(c => c.Value).ToList();
                            hand = new PokerHand(HAND.STRAIGHT_FLUSH,
                                straightFlushCards,
                                highcard,
                                null,
                                kickers);
                            return true;
                        }
                    }
                    break;
                }
            }

            hand = null;
            return false;
        }

        /// <summary>
        /// Determines if the given cards form a Four of a Kind.
        /// </summary>
        /// <param name="cards">The list of cards to evaluate.</param>
        /// <param name="hand">The resulting poker hand, if found.</param>
        /// <returns>True if a Four of a Kind is found, otherwise false.</returns>
        private bool HasFourOfAKind(List<Card> cards, out PokerHand hand)
        {
            var groupedByValue = cards.GroupBy(c => c.Value);

            foreach (var group in groupedByValue)
            {
                if (group.Count() == 4)
                {
                    var fourOfAKindCards = group.ToList();
                    List<Card> kickers = cards.OrderByDescending(c => c.Value).ToList();
                    hand = new PokerHand(HAND.FOUR_KIND,
                        fourOfAKindCards,
                        group.First(),
                        null,
                        kickers);
                    return true;
                }
            }

            hand = null;
            return false;
        }

        /// <summary>
        /// Determines if the given cards form a Full House.
        /// </summary>
        /// <param name="cards">The list of cards to evaluate.</param>
        /// <param name="hand">The resulting poker hand, if found.</param>
        /// <returns>True if a Full House is found, otherwise false.</returns>
        private bool HasFullHouse(List<Card> cards, out PokerHand hand)
        {
            var groupedByValue = cards.GroupBy(c => c.Value);

            // Find the groups of three and two cards
            var threeOfAKind = groupedByValue.FirstOrDefault(group => group.Count() == 3);
            var pair = groupedByValue.FirstOrDefault(group => group.Count() >= 2 && group.Key != threeOfAKind?.Key);

            // If both a three of a kind and a pair are found, it's a Full House
            if (threeOfAKind != null && pair != null)
            {
                var fullHouseCards = threeOfAKind.Concat(pair.Take(2)).ToList();
                hand = new PokerHand(HAND.FULL_HOUSE,
                    fullHouseCards,
                    threeOfAKind.First(),
                    pair.First(),
                    GetKickers(cards, fullHouseCards));
                return true;
            }

            hand = null;
            return false;
        }

        /// <summary>
        /// Determines if the given cards form a Flush.
        /// </summary>
        /// <param name="cards">The list of cards to evaluate.</param>
        /// <param name="hand">The resulting poker hand, if found.</param>
        /// <returns>True if a Flush is found, otherwise false.</returns>
        private bool HasFlush(List<Card> cards, out PokerHand hand)
        {
            // Group the cards by suit
            var groupedBySuit = cards.GroupBy(c => c.Suit);

            foreach (var group in groupedBySuit)
            {
                // If you have 5 cards with the same suit, you have a Flush
                if (group.Count() >= 5)
                {
                    var flushCards = group.OrderByDescending(c => c.Value).Take(5).ToList();
                    var kickers = flushCards;
                    kickers = kickers.Concat(GetKickers(cards, flushCards)).ToList();
                    hand = new PokerHand(HAND.FLUSH,
                        flushCards,
                        flushCards.FirstOrDefault(),
                        null,
                        kickers);
                    return true;
                }
            }

            hand = null;
            return false;
        }

        /// <summary>
        /// Determines if the given cards form a Straight.
        /// </summary>
        /// <param name="cards">The list of cards to evaluate.</param>
        /// <param name="hand">The resulting poker hand, if found.</param>
        /// <returns>True if a Straight is found, otherwise false.</returns>
        private bool HasStraight(List<Card> cards, out PokerHand hand)
        {
            List<Card> kickers = cards.OrderByDescending(k => k.Value).ToList();
            cards = cards.OrderByDescending(c => c.Value).DistinctBy(c => c.Value).ToList();

            // The ace finalize Straight in poker
            bool hasAce = cards.Any(c => c.Value == Card.VALUE.ACE);

            for (int i = 0; i <= cards.Count - 5; i++)
            {
                bool isStraight = true;

                // Check if the next five cards are consecutive
                for (int j = i + 1; j < i + 5; j++)
                {
                    if (cards[j].Value != cards[j - 1].Value - 1)
                    {
                        isStraight = false;
                        break;
                    }
                }


                if (isStraight)
                {
                    List<Card> straightCards = cards.GetRange(i, 5);
                    hand = new PokerHand(HAND.STRAIGHT,
                        straightCards,
                        cards.First(),
                        null,
                        kickers);
                    return true;
                }
            }

            // Check if the ace finalize Straight
            if (hasAce)
            {
                var fourInitialCards = cards.OrderBy(c => c.Value).Take(4).ToList();
                var lowerStraightValues = new List<Card.VALUE>
                {
                    Card.VALUE.TWO,
                    Card.VALUE.THREE,
                    Card.VALUE.FOUR,
                    Card.VALUE.FIVE
                };

                bool isLowerStraight = fourInitialCards.Select(c => c.Value).SequenceEqual(lowerStraightValues);
                if (isLowerStraight)
                {

                    Card highcard = fourInitialCards.Last();
                    Card aceCard = cards.Where(c => c.Value == Card.VALUE.ACE).FirstOrDefault();

                    fourInitialCards.Add(aceCard);
                    List<Card> straightCards = fourInitialCards.OrderByDescending(c => c.Value).ToList();
                    hand = new PokerHand(HAND.STRAIGHT,
                        straightCards,
                        highcard,
                        null,
                        kickers);
                    return true;
                }
            }

            hand = null;
            return false;
        }

        /// <summary>
        /// Determines if the given cards form a Three of a Kind.
        /// </summary>
        /// <param name="cards">The list of cards to evaluate.</param>
        /// <param name="hand">The resulting poker hand, if found.</param>
        /// <returns>True if a Three of a Kind is found, otherwise false.</returns>
        private bool HasThreeOfAKind(List<Card> cards, out PokerHand hand)
        {
            var groupedByValue = cards.GroupBy(c => c.Value);

            // Find the group of three cards
            var threeOfAKind = groupedByValue.FirstOrDefault(group => group.Count() == 3);

            // If a group of three is found, it's Three of a Kind
            if (threeOfAKind != null)
            {
                var threeOfAKindCards = threeOfAKind.ToList();
                hand = new PokerHand(HAND.THREE_KIND,
                    threeOfAKindCards,
                    threeOfAKindCards.First(),
                    null,
                    GetKickers(cards, threeOfAKindCards));
                return true;
            }

            hand = null;
            return false;
        }

        /// <summary>
        /// Determines if the given cards form Two Pair.
        /// </summary>
        /// <param name="cards">The list of cards to evaluate.</param>
        /// <param name="hand">The resulting poker hand, if found.</param>
        /// <returns>True if Two Pair is found, otherwise false.</returns>
        private bool HasTwoPair(List<Card> cards, out PokerHand hand)
        {
            cards = cards.OrderByDescending(c => c.Value).ToList();
            var groupedByValue = cards.GroupBy(c => c.Value);

            // Find all pairs
            var pairs = groupedByValue.Where(group => group.Count() == 2).ToList();

            if (pairs.Count > 2)
            {
                // If more than two pairs are found, select the top two pairs
                var twoPairCards = pairs[0].ToList();
                twoPairCards = twoPairCards.Concat(pairs[1].ToList()).ToList();

                hand = new PokerHand(HAND.TWO_PAIR,
                    twoPairCards,
                    twoPairCards.First(),
                    twoPairCards.Last(),
                    GetKickers(cards, twoPairCards));
                return true;
            }
            else if (pairs.Count == 2)
            {
                // If exactly two pairs are found, select both pairs
                var twoPairCards = pairs.SelectMany(pair => pair.ToList()).ToList();
                twoPairCards = twoPairCards.OrderByDescending(c => c.Value).ToList();

                hand = new PokerHand(HAND.TWO_PAIR,
                    twoPairCards,
                    twoPairCards.First(),
                    twoPairCards.Last(),
                    GetKickers(cards, twoPairCards));
                return true;
            }

            hand = null;
            return false;
        }

        /// <summary>
        /// Determines if the given cards form One Pair.
        /// </summary>
        /// <param name="cards">The list of cards to evaluate.</param>
        /// <param name="hand">The resulting poker hand, if found.</param>
        /// <returns>True if One Pair is found, otherwise false.</returns>
        private bool HasOnePair(List<Card> cards, out PokerHand hand)
        {
            var groupedByValue = cards.GroupBy(c => c.Value);

            // Find a group of two cards
            var pair = groupedByValue.FirstOrDefault(group => group.Count() == 2);

            // If a pair is found, it's One Pair
            if (pair != null)
            {
                var pairCards = pair.ToList();
                hand = new PokerHand(HAND.ONE_PAIR, pairCards, pairCards.First(), null, GetKickers(cards, pairCards));
                return true;
            }

            hand = null;
            return false;
        }

        /// <summary>
        /// Determines if the given cards form a High Card.
        /// </summary>
        /// <param name="cards">The list of cards to evaluate.</param>
        /// <param name="hand">The resulting poker hand, if found.</param>
        /// <returns>True if a High Card is found, otherwise false.</returns>
        private bool HasHighCard(List<Card> cards, out PokerHand hand)
        {
            if (cards.Count > 0)
            {
                var highCard = cards.OrderByDescending(c => c.Value).First();
                hand = new PokerHand(HAND.HIGH_CARD,
                    new List<Card> { highCard },
                    highCard,
                    null,
                    cards.OrderByDescending(card => card.Value).ToList());
                return true;
            }

            hand = null;
            return false;
        }
    }
}