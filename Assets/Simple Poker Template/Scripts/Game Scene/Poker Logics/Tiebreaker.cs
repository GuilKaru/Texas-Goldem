using System.Collections.Generic;
using System.Linq;

namespace SimplePoker.Logic
{
    /// <summary>
    /// Manages tiebreakers in a poker game, determining the winners in case of tied hands or kickers.
    /// </summary>
    public class Tiebreaker
    {
        class SubKicker
        {
            public List<PlayerKicker> Kickers = new List<PlayerKicker>();
        }

        class PlayerKicker
        {
            public PlayerBase Player;
            public Card Kicker;
            public int KickerValue;

            public PlayerKicker(PlayerBase player, Card kicker, int kickerValue)
            {
                Player = player;
                Kicker = kicker;
                KickerValue = kickerValue;
            }
        }

        /// <summary>
        /// Determines the winners in case of a tie based on the type of hand.
        /// </summary>
        /// <param name="players">The list of tied players.</param>
        /// <param name="hand">The type of hand for which tiebreaking is required.</param>
        /// <returns>The list of winners after tiebreaking.</returns>
        public List<PlayerBase> PokerHandsTiebreaker(List<PlayerBase> players, HAND hand)
        {
            switch (hand)
            {
                case HAND.HIGH_CARD:
                    return HighCardTiebreaker(players);
                case HAND.ONE_PAIR:
                    return HighCardTiebreaker(players);
                case HAND.TWO_PAIR:
                    return TwoHighCardTiebreaker(players);
                case HAND.THREE_KIND:
                    return HighCardTiebreaker(players);
                case HAND.STRAIGHT:
                    return HighCardTiebreaker(players);
                case HAND.FLUSH:
                    return HighCardTiebreaker(players);
                case HAND.FULL_HOUSE:
                    return TwoHighCardTiebreaker(players);
                case HAND.FOUR_KIND:
                    return HighCardTiebreaker(players);
                case HAND.STRAIGHT_FLUSH:
                    return HighCardTiebreaker(players);
                case HAND.ROYAL_STRAIGHT_FLUSH:
                    return players;
            }
            return players;
        }

        /// <summary>
        /// Tiebreaker for hands where high card determines the winner.
        /// </summary>
        /// <param name="tiedPlayers">The list of tied players.</param>
        /// <returns>The list of winners after tiebreaking.</returns>
        private List<PlayerBase> HighCardTiebreaker(List<PlayerBase> tiedPlayers)
        {
            List<PlayerBase> winners = new List<PlayerBase>();

            PlayerBase winner = tiedPlayers[0];
            winners.Add(winner);
            for (int i = 1; i < tiedPlayers.Count; i++)
            {
                Card currentWinnerCard = winner.PokerHand.FirstHighCardHand;
                Card compareCard = tiedPlayers[i].PokerHand.FirstHighCardHand;

                if (currentWinnerCard.Value < compareCard.Value)
                {
                    winners.Clear();
                    winners.Add(tiedPlayers[i]);
                    winner = tiedPlayers[i];
                }
                else if (currentWinnerCard.Value == compareCard.Value)
                {
                    winners.Add(tiedPlayers[i]);
                }
            }
            return winners;
        }

        /// <summary>
        /// Tiebreaker for hands where two high cards determine the winner, like two pairs and full house.
        /// </summary>
        /// <param name="tiedPlayers">The list of tied players.</param>
        /// <returns>The list of winners after tiebreaking.</returns>
        private List<PlayerBase> TwoHighCardTiebreaker(List<PlayerBase> tiedPlayers)
        {
            List<PlayerBase> winners = new List<PlayerBase>();

            PlayerBase winner = tiedPlayers[0];
            winners.Add(winner);
            for (int i = 1; i < tiedPlayers.Count; i++)
            {
                Card firstCurrentWinnerCard = winner.PokerHand.FirstHighCardHand;
                Card secondCurrentWinnerCard = winner.PokerHand.SecondHighCardHand;
                Card firstCompareCard = tiedPlayers[i].PokerHand.FirstHighCardHand;
                Card secondCompareCard = tiedPlayers[i].PokerHand.SecondHighCardHand;

                if (firstCurrentWinnerCard.Value < firstCompareCard.Value)
                {
                    winners.Clear();
                    winners.Add(tiedPlayers[i]);
                    winner = tiedPlayers[i];
                }
                else if (firstCurrentWinnerCard.Value == firstCompareCard.Value)
                {
                    if (secondCurrentWinnerCard.Value < secondCompareCard.Value)
                    {
                        winners.Clear();
                        winners.Add(tiedPlayers[i]);
                        winner = tiedPlayers[i];
                    }
                    else if (secondCurrentWinnerCard.Value == secondCompareCard.Value)
                    {
                        winners.Add(tiedPlayers[i]);
                    }
                }
            }
            return winners;
        }

        /// <summary>
        /// Determines the winners in case of a tie based on the kickers.
        /// </summary>
        /// <param name="tiedPlayers">The list of tied players.</param>
        /// <returns>The list of winners after tiebreaking.</returns>
        public List<PlayerBase> PokerKickersTiebreaker(List<PlayerBase> tiedPlayers)
        {
            int numKickers = tiedPlayers.First().PokerHand.Kickers.Count;

            List<SubKicker> subKickers = new List<SubKicker>();

            for (int k = 0; k < numKickers; k++)
            {
                SubKicker subKicker = new SubKicker();
                for (int i = 0; i < tiedPlayers.Count; i++)
                {
                    PlayerBase player = tiedPlayers[i];
                    Card kicker = tiedPlayers[i].PokerHand.Kickers[k];
                    int kickerValue = (int)kicker.Value;

                    PlayerKicker playerKicker = new PlayerKicker(player, kicker, kickerValue);

                    subKicker.Kickers.Add(playerKicker);
                }
                subKickers.Add(subKicker);
            }

            for (int i = 0; i < subKickers.Count; i++)
            {
                int highCardValue = HighCardValue(subKickers[i].Kickers);
                if (!AllIsEqual(subKickers[i].Kickers, highCardValue))
                {
                    List<PlayerBase> winners = GetAllPlayerWinner(subKickers[i].Kickers, highCardValue);
                    if (winners.Count > 0)
                        return winners;
                    else
                        return tiedPlayers;
                }
            }
            return tiedPlayers;
        }

        /// <summary>
        /// Checks if all kicker values in a list are equal to a specified value.
        /// </summary>
        /// <param name="playerKickers">The list of player kickers.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>True if all kicker values are equal to the specified value; otherwise, false.</returns>
        bool AllIsEqual(List<PlayerKicker> playerKickers, int value)
        {
            return playerKickers.All(valor => valor.KickerValue == value);
        }

        /// <summary>
        /// Gets the highest kicker value from a list of player kickers.
        /// </summary>
        /// <param name="playerKickers">The list of player kickers.</param>
        /// <returns>The highest kicker value.</returns>
        int HighCardValue(List<PlayerKicker> playerKickers)
        {
            int maxValue = -1;

            for (int i = 0; i < playerKickers.Count; i++)
            {
                if (playerKickers[i].KickerValue >= maxValue)
                    maxValue = playerKickers[i].KickerValue;
            }

            return maxValue;
        }

        /// <summary>
        /// Gets all players with a kicker value equal to a specified value.
        /// </summary>
        /// <param name="playerKickers">The list of player kickers.</param>
        /// <param name="value">The kicker value.</param>
        /// <returns>The list of players with a kicker value equal to the specified card value.</returns>
        List<PlayerBase> GetAllPlayerWinner(List<PlayerKicker> playerKickers, int value)
        {
            List<PlayerKicker> playersKickersWinner = playerKickers.Where(p => p.KickerValue == value).ToList();

            List<PlayerBase> winners = new List<PlayerBase>();
            for (int i = 0; i < playersKickersWinner.Count; i++)
            {
                playersKickersWinner[i].Player.PokerHand.KickerHighCard = playersKickersWinner[i].Kicker;
                winners.Add(playersKickersWinner[i].Player);
            }
            return winners;
        }

    }
}