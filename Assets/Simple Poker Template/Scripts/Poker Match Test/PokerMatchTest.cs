using SimplePoker.Logic;
using System.Collections.Generic;

namespace SimplePoker.MatchTest
{
    /// <summary>
    /// Class for testing different scenarios in a poker match, including high card matches and split pot outcomes.
    /// </summary>
    public class PokerMatchTest
    {

        #region HIGHCARD

        /// <summary>
        /// Determines the winning hand in a high card match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> WinnerHighCardMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.QUEEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a high card match with kicker tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> WinnerHighCardKickersTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.QUEEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the outcome of a split pot in a high card match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The cards for a split pot outcome.</returns>
        public List<Card> SplitpotHighCardMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        #endregion

        #region ONE_PAIR

        /// <summary>
        /// Determines the winning hand in a one pair match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> OnePairWinnerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a one pair match with high card tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> OnePairWinnerHighcardTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a one pair match based on player cards.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> OnePairWinnerOnHandMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }


        /// <summary>
        /// Determines the winning hand in a one pair match with kicker tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> OnePairWinnerKickersTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.QUEEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the outcome of a split pot in a one pair match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The cards for a split pot outcome.</returns>
        public List<Card> OnePairSplitpotMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        #endregion

        #region TWO_PAIR

        /// <summary>
        /// Determines the winning hand in a two pair match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> TwoPairWinnerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a two pair match with high card tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> TwoPairWinnerHighcardTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a two pair match with kicker tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> TwoPairWinnerKickersTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.QUEEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the outcome of a split pot in a two pair match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The cards for a split pot outcome.</returns>
        public List<Card> TwoPairSplitpotMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        #endregion

        #region THREE_KIND

        /// <summary>
        /// Determines the winning hand in a three of a kind match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> ThreeKindWinnerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a three of a kind match with high card tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> ThreeKindWinnerHighcardTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a three of a kind match with kicker tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> ThreeKindWinnerKickersTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
          // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the outcome of a split pot in a three of a kind match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The cards for a split pot outcome.</returns>
        public List<Card> ThreeKindSplitpotMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        #endregion

        #region STRAIGHT

        /// <summary>
        /// Determines the winning hand in a straight match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> StraightWinnerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a straight match with high card tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> StraightWinnerHighcardTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a straight match with kicker tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> StraightWinnerKickersTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the outcome of a split pot in a straight match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The cards for a split pot outcome.</returns>
        public List<Card> StraightSplitpotMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a lower straight match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> LowerStraightWinnerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a lower straight match with high card tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> LowerStraightWinnerHighcardTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a lower straight match with kicker tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> LowerStraightWinnerKickersTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the outcome of a split pot in a lower straight match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The cards for a split pot outcome.</returns>
        public List<Card> LowerStraightSplitpotMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        #endregion

        #region FLUSH

        /// <summary>
        /// Determines the winning hand in a flush match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> FlushWinnerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.CLUBS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a flush match with high card tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> FlushWinnerHighcardTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.CLUBS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a flush match with kicker tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> FlushWinnerKickersTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the outcome of a split pot in a flush match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The cards for a split pot outcome.</returns>
        public List<Card> FlushSplitpotMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        #endregion

        #region FULL_HOUSE

        /// <summary>
        /// Determines the winning hand in a full house match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> FullHouseWinnerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a full house match with high card tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> FullHouseWinnerHighcardTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }


        /// <summary>
        /// Determines the winning hand in a full house match with kicker tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> FullHouseWinnerKickersTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the outcome of a split pot in a full house match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The cards for a split pot outcome.</returns>
        public List<Card> FullHouseSplitpotMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        #endregion

        #region FOUR_KIND

        /// <summary>
        /// Determines the winning hand in a four of a kind match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> FourKindWinnerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a four of a kind match with high card tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> FourKindWinnerHighcardTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a four of a kind match with kicker tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> FourKindWinnerKickersTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }


        /// <summary>
        /// Determines the outcome of a split pot in a four of a kind match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The cards for a split pot outcome.</returns>
        public List<Card> FourKindSplitpotMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        #endregion

        #region STRAIGHT_FLUSH

        /// <summary>
        /// Determines the winning hand in a straight flush match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> StraightFlushWinnerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a straight flush match with high card tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> StraightFlushWinnerHighcardTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a straight flush match with kicker tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> StraightFlushWinnerKickersTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.HEART),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the result of a split pot in a straight flush match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The cards resulting in a split pot.</returns>
        public List<Card> StraightFlushSplitpotMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
        };

            deck.Cards.Clear();
            return simulateCards;
        }


        /// <summary>
        /// Determines the winning hand in a lower straight flush match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> LowerStraightFlushWinnerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a lower straight flush match with high card tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> LowerStraightFlushWinnerHighcardTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a lower straight flush match with kicker tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> LowerStraightFlushWinnerKickersTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the result of a split pot in a lower straight flush match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The cards resulting in a split pot.</returns>
        public List<Card> LowerStraightFlushSplitpotMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        #endregion

        #region ROYAL_STRAIGHT_FLUSH

        /// <summary>
        /// Determines the winning hand in a royal straight flush match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> RoyalStraightFlushWinnerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.QUEEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a royal straight flush match with high card tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> RoyalStraightFlushWinnerHighcardTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.SEVEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.THREE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FOUR, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.SIX, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.DIAMONDS),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the winning hand in a royal straight flush match with kicker tiebreaker.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The winning hand's cards.</returns>
        public List<Card> RoyalStraightFlushWinnerKickersTiebreakerMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.EIGHT, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.DIAMONDS),
            deck.GetCardByValueAndSuit(Card.VALUE.FIVE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.QUEEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.HEART),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        /// <summary>
        /// Determines the result of a split pot in a royal straight flush match.
        /// </summary>
        /// <param name="deck">The deck containing player cards and table cards.</param>
        /// <returns>The cards resulting in a split pot.</returns>
        public List<Card> RoyalStraightFlushSplitpotMatch(Deck deck)
        {
            List<Card> simulateCards = new List<Card>{
            // Player cards
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.CLUBS),
            deck.GetCardByValueAndSuit(Card.VALUE.TWO, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.SPADES),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.NINE, Card.SUIT.CLUBS),

            // Table cards
            deck.GetCardByValueAndSuit(Card.VALUE.TEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.JACK, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.KING, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.QUEEN, Card.SUIT.HEART),
            deck.GetCardByValueAndSuit(Card.VALUE.ACE, Card.SUIT.HEART),
        };

            deck.Cards.Clear();
            return simulateCards;
        }

        #endregion

        // Match Tests, include this to IEnumerator on Game Manager, after EnablePlayersInMatchCoroutine() method

        // HIGH CARD Tests
        //Deck.cards = new PokerMatchTest().WinnerHighCardMatch(Deck); //Winner Highcard
        //Deck.cards = new PokerMatchTest().WinnerHighCardKickersTiebreakerMatch(Deck); //Winner Highcard Kickers
        //Deck.cards = new PokerMatchTest().SplitpotHighCardMatch(Deck); //Tie Highcard

        // ONE PAIR Tests
        //Deck.cards = new PokerMatchTest().OnePairWinnerMatch(Deck); //Winner Onepair
        //Deck.cards = new PokerMatchTest().OnePairWinnerHighcardTiebreakerMatch(Deck); //Winner Highcard Onepair
        //Deck.cards = new PokerMatchTest().OnePairWinnerOnHandMatch(Deck); //Winner Direct on hand Onepair
        //Deck.cards = new PokerMatchTest().OnePairWinnerKickersTiebreakerMatch(Deck); //Winner Kickers Onepair
        //Deck.cards = new PokerMatchTest().OnePairSplitpotMatch(Deck); //Tie Onepair

        // TWO PAIR Tests
        //Deck.cards = new PokerMatchTest().TwoPairWinnerMatch(Deck); //Winner TwoPair
        //Deck.cards = new PokerMatchTest().TwoPairWinnerHighcardTiebreakerMatch(Deck); //Winner Highcard TwoPair
        //Deck.cards = new PokerMatchTest().TwoPairWinnerKickersTiebreakerMatch(Deck); //Winner Kickers TwoPair
        //Deck.cards = new PokerMatchTest().TwoPairSplitpotMatch(Deck); //Tie TwoPair

        // THREE OF KIND Tests
        //Deck.cards = new PokerMatchTest().ThreeKindWinnerMatch(Deck); //Winner ThreeKind
        //Deck.cards = new PokerMatchTest().ThreeKindWinnerHighcardTiebreakerMatch(Deck); //Winner Highcard ThreeKind
        //Deck.cards = new PokerMatchTest().ThreeKindWinnerKickersTiebreakerMatch(Deck); //Winner Kickers ThreeKind
        //Deck.cards = new PokerMatchTest().ThreeKindSplitpotMatch(Deck); //Tie ThreeKind

        // STRAIGHT Tests
        //Deck.cards = new PokerMatchTest().StraightWinnerMatch(Deck); //Winner Straight
        //Deck.cards = new PokerMatchTest().StraightWinnerHighcardTiebreakerMatch(Deck); //Winner Highcard Straight
        //Deck.cards = new PokerMatchTest().StraightWinnerKickersTiebreakerMatch(Deck); //Winner Kickers Straight
        //Deck.cards = new PokerMatchTest().StraightSplitpotMatch(Deck); //Tie Straight

        //LOWER STRAIGHT Tests
        //Deck.cards = new PokerMatchTest().LowerStraightWinnerMatch(Deck); //Winner Straight
        //Deck.cards = new PokerMatchTest().LowerStraightWinnerHighcardTiebreakerMatch(Deck); //Winner Straight
        //Deck.cards = new PokerMatchTest().LowerStraightWinnerKickersTiebreakerMatch(Deck); //Winner Straight
        //Deck.cards = new PokerMatchTest().LowerStraightSplitpotMatch(Deck); //Winner Straight


        // FLUSH Tests
        //Deck.cards = new PokerMatchTest().FlushWinnerMatch(Deck); //Winner Flush
        //Deck.cards = new PokerMatchTest().FlushWinnerHighcardTiebreakerMatch(Deck); //Winner Highcard Flush
        //Deck.cards = new PokerMatchTest().FlushWinnerKickersTiebreakerMatch(Deck); //Winner Kickers Flush
        //Deck.cards = new PokerMatchTest().FlushSplitpotMatch(Deck); //Tie Flush

        // FULL HOUSE Tests
        //Deck.cards = new PokerMatchTest().FullHouseWinnerMatch(Deck); //Winner FullHouse
        //Deck.cards = new PokerMatchTest().FullHouseWinnerHighcardTiebreakerMatch(Deck); //Winner Highcard FullHouse
        //Deck.cards = new PokerMatchTest().FullHouseWinnerKickersTiebreakerMatch(Deck); //Winner Kickers FullHouse
        //Deck.cards = new PokerMatchTest().FullHouseSplitpotMatch(Deck); //Tie FullHouse

        // FOUR_KIND Tests
        //Deck.cards = new PokerMatchTest().FourKindWinnerMatch(Deck); //Winner FourKind
        //Deck.cards = new PokerMatchTest().FourKindWinnerHighcardTiebreakerMatch(Deck); //Winner Highcard FourKind
        //Deck.cards = new PokerMatchTest().FourKindWinnerKickersTiebreakerMatch(Deck); //Winner Kickers FourKind
        //Deck.cards = new PokerMatchTest().FourKindSplitpotMatch(Deck); //Tie FourKind

        // STRAIGHT_FLUSH Tests
        //Deck.cards = new PokerMatchTest().StraightFlushWinnerMatch(Deck); //Winner StraightFlush
        //Deck.cards = new PokerMatchTest().StraightFlushWinnerHighcardTiebreakerMatch(Deck); //Winner Highcard StraightFlush
        //Deck.cards = new PokerMatchTest().StraightFlushWinnerKickersTiebreakerMatch(Deck); //Winner Kickers StraightFlush
        //Deck.cards = new PokerMatchTest().StraightFlushSplitpotMatch(Deck); //Tie StraightFlush

        // LOWER STRAIGHT_FLUSH Tests
        //Deck.cards = new PokerMatchTest().LowerStraightFlushWinnerMatch(Deck); //Winner StraightFlush
        //Deck.cards = new PokerMatchTest().LowerStraightFlushWinnerHighcardTiebreakerMatch(Deck); //Winner StraightFlush
        //Deck.cards = new PokerMatchTest().LowerStraightFlushWinnerKickersTiebreakerMatch(Deck); //Winner StraightFlush
        //Deck.cards = new PokerMatchTest().LowerStraightFlushSplitpotMatch(Deck); //Winner StraightFlush


        // ROYAL_STRAIGHT_FLUSH Tests
        //Deck.cards = new PokerMatchTest().RoyalStraightFlushWinnerMatch(Deck); //Winner RoyalStraightFlush
        //Deck.cards = new PokerMatchTest().RoyalStraightFlushWinnerKickersTiebreakerMatch(Deck); //Winner Kickers RoyalStraightFlush
        //Deck.cards = new PokerMatchTest().RoyalStraightFlushSplitpotMatch(Deck); //Tie RoyalStraightFlush
    }
}