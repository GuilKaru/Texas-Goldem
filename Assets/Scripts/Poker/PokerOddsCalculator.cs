using System.Collections.Generic;
using UnityEngine;
using TexasHoldem;

public static class PokerOddsCalculator
{
    private struct HandRank
    {
        public int Category;
        public List<int> Kickers;
        public List<int> BestFiveCards; // the 5 u8 card ints that form this rank

        public HandRank(int category, List<int> kickers, List<int> bestFive)
        {
            Category      = category;
            Kickers       = kickers;
            BestFiveCards = bestFive;
        }
    }

    // ── Public: Multi-way equity (v2 primary entry point) ────────────────────
    
    public static Dictionary<int, float> CalculateMultiWayEquity(
        List<(int seatIndex, int card0, int card1)> activeSeatHands,
        List<int> communityCards)
    {
        Dictionary<int, float> result = new Dictionary<int, float>();

        if (activeSeatHands == null || activeSeatHands.Count == 0)
            return result;

        // Single player still in — 100% equity
        if (activeSeatHands.Count == 1)
        {
            result[activeSeatHands[0].seatIndex] = 1f;
            return result;
        }

        List<int> board       = communityCards != null ? new List<int>(communityCards) : new List<int>();
        int       cardsToCome = 5 - board.Count;

        // Initialise tallies
        double[] points = new double[activeSeatHands.Count];

        if (cardsToCome <= 0)
        {
            // ── Exact: all 5 community cards known ───────────────────────────
            ScoreRunout(activeSeatHands, board, points);
        }
        else if (cardsToCome == 1)
        {
            // ── Exact: enumerate every possible river card ────────────────────
            List<int> usedCards = BuildUsedCards(activeSeatHands, board);
            List<int> deck      = BuildRemainingDeck(usedCards);

            int total = 0;
            double[] runPoints = new double[activeSeatHands.Count];

            foreach (int card in deck)
            {
                List<int> fullBoard = new List<int>(board) { card };
                ScoreRunout(activeSeatHands, fullBoard, runPoints);
                for (int i = 0; i < points.Length; i++)
                    points[i] += runPoints[i];
                total++;
            }

            if (total > 0)
                for (int i = 0; i < points.Length; i++)
                    points[i] /= total;
        }
        else
        {
            // ── Monte Carlo: preflop (3500 sims), flop (2500 sims) ───────────
            int iterations    = cardsToCome >= 4 ? 3500 : 2500;
            List<int> usedCards = BuildUsedCards(activeSeatHands, board);
            List<int> deck      = BuildRemainingDeck(usedCards);

            double[] runPoints = new double[activeSeatHands.Count];

            for (int sim = 0; sim < iterations; sim++)
            {
                List<int> sampled  = DrawRandomCards(deck, cardsToCome);
                List<int> fullBoard = new List<int>(board);
                fullBoard.AddRange(sampled);

                ScoreRunout(activeSeatHands, fullBoard, runPoints);
                for (int i = 0; i < points.Length; i++)
                    points[i] += runPoints[i];
            }

            for (int i = 0; i < points.Length; i++)
                points[i] /= iterations;
        }

        // Normalise and build result dictionary
        double sum = 0;
        for (int i = 0; i < points.Length; i++) sum += points[i];
        if (sum <= 0) sum = 1;

        for (int i = 0; i < activeSeatHands.Count; i++)
            result[activeSeatHands[i].seatIndex] = (float)(points[i] / sum);

        return result;
    }

    // ── Public: Best five cards + hand name (unchanged) ───────────────────────
    /// Given hole cards and a 5-card board, returns the 5 u8 card ints that
    /// form the best poker hand and the hand name (e.g. "Flush", "Two Pair").
    /// Returns empty list if board has fewer than 3 cards.
    public static (List<int> bestFive, string handName) GetBestFiveWithName(
        int hole0, int hole1, List<int> board)
    {
        if (board == null || board.Count < 3)
            return (new List<int>(), "");

        List<int> sevenCards = BuildSevenCards(hole0, hole1, board);
        HandRank  best       = EvaluateBestOfSeven(sevenCards);

        return (best.BestFiveCards ?? new List<int>(), HandCategoryName(best.Category));
    }

    // ── Public: Heads-up equity (kept for backwards compatibility) ────────────

    public static float CalculateHeadsUpEquity(
        int seat0Card0, int seat0Card1,
        int seat1Card0, int seat1Card1,
        List<int> board)
    {
        var hands = new List<(int, int, int)>
        {
            (0, seat0Card0, seat0Card1),
            (1, seat1Card0, seat1Card1)
        };

        var equity = CalculateMultiWayEquity(hands, board);
        return equity.TryGetValue(0, out float e) ? e : 0.5f;
    }

    public static int CompareFinalBoardResult(
        int seat0Card0, int seat0Card1,
        int seat1Card0, int seat1Card1,
        List<int> board)
    {
        return CompareHands(
            BuildSevenCards(seat0Card0, seat0Card1, board),
            BuildSevenCards(seat1Card0, seat1Card1, board)
        );
    }

    // ── Private: multi-way runout scoring ────────────────────────────────────
    
    /// Evaluates one complete board runout for all active seats.
    /// Awards 1 point to the winner, or 1/N points each on a split.
    /// Results are accumulated into the points array (caller zeroes it per sim).
    private static void ScoreRunout(
        List<(int seatIndex, int card0, int card1)> hands,
        List<int> board,
        double[] points)
    {
        // Reset output array
        for (int i = 0; i < points.Length; i++)
            points[i] = 0;

        int n = hands.Count;
        HandRank[] ranks = new HandRank[n];

        for (int i = 0; i < n; i++)
        {
            List<int> sevenCards = BuildSevenCards(hands[i].card0, hands[i].card1, board);
            ranks[i] = EvaluateBestOfSeven(sevenCards);
        }

        // Find the best rank
        HandRank bestRank = ranks[0];
        for (int i = 1; i < n; i++)
            if (CompareHandRanks(ranks[i], bestRank) > 0)
                bestRank = ranks[i];

        // Collect all seats that tie for best
        List<int> winners = new List<int>();
        for (int i = 0; i < n; i++)
            if (CompareHandRanks(ranks[i], bestRank) == 0)
                winners.Add(i);

        double share = 1.0 / winners.Count;
        foreach (int w in winners)
            points[w] = share;
    }
    
    /// Builds the set of all cards currently known (hole cards + board).
    /// Used to build the remaining deck for enumeration / Monte Carlo.
    private static List<int> BuildUsedCards(
        List<(int seatIndex, int card0, int card1)> hands,
        List<int> board)
    {
        List<int> used = new List<int>();
        foreach (var h in hands)
        {
            used.Add(h.card0);
            used.Add(h.card1);
        }
        if (board != null) used.AddRange(board);
        return used;
    }

    // ── Private: deck / random helpers ───────────────────────────────────────

    private static List<int> DrawRandomCards(List<int> sourceDeck, int count)
    {
        List<int> temp = new List<int>(sourceDeck);
        List<int> result = new List<int>(count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, temp.Count);
            result.Add(temp[index]);
            temp.RemoveAt(index);
        }

        return result;
    }

    private static List<int> BuildRemainingDeck(List<int> usedCards)
    {
        HashSet<int> used = new HashSet<int>(usedCards);
        List<int> deck = new List<int>(52);

        for (int i = 0; i < 52; i++)
        {
            if (!used.Contains(i))
                deck.Add(i);
        }

        return deck;
    }

    private static List<int> BuildSevenCards(int hole0, int hole1, List<int> board)
    {
        List<int> cards = new List<int>(7) { hole0, hole1 };
        if (board != null)
            cards.AddRange(board);
        return cards;
    }

    private static int CompareHands(List<int> sevenCardsA, List<int> sevenCardsB)
    {
        HandRank a = EvaluateBestOfSeven(sevenCardsA);
        HandRank b = EvaluateBestOfSeven(sevenCardsB);
        return CompareHandRanks(a, b);
    }

    // Returns the best HandRank out of all C(n,5) combos in sevenCards.
    // BestFiveCards stores the actual u8 card ints of that combo.
    private static HandRank EvaluateBestOfSeven(List<int> sevenCards)
    {
        if (sevenCards == null || sevenCards.Count < 5)
            return new HandRank(0, new List<int> { 0 }, new List<int>());

        HandRank best = default;
        bool hasBest = false;

        int count = sevenCards.Count;

        for (int a = 0; a < count - 4; a++)
        {
            for (int b = a + 1; b < count - 3; b++)
            {
                for (int c = b + 1; c < count - 2; c++)
                {
                    for (int d = c + 1; d < count - 1; d++)
                    {
                        for (int e = d + 1; e < count; e++)
                        {
                            List<int> fiveInts = new List<int>(5)
                            {
                                sevenCards[a],
                                sevenCards[b],
                                sevenCards[c],
                                sevenCards[d],
                                sevenCards[e]
                            };

                            HandRank rank = EvaluateFiveCardHand(fiveInts);

                            if (!hasBest || CompareHandRanks(rank, best) > 0)
                            {
                                best = rank;
                                best.BestFiveCards = fiveInts;
                                hasBest = true;
                            }
                        }
                    }
                }
            }
        }

        return best;
    }

    private static int CompareHandRanks(HandRank a, HandRank b)
    {
        if (a.Category != b.Category)
            return a.Category.CompareTo(b.Category);

        int count = Mathf.Max(a.Kickers.Count, b.Kickers.Count);

        for (int i = 0; i < count; i++)
        {
            int av = i < a.Kickers.Count ? a.Kickers[i] : 0;
            int bv = i < b.Kickers.Count ? b.Kickers[i] : 0;

            if (av != bv)
                return av.CompareTo(bv);
        }

        return 0;
    }

    private static HandRank EvaluateFiveCardHand(List<int> fiveCards)
    {
        List<int> values = new List<int>(5);
        List<int> suits  = new List<int>(5);

        for (int i = 0; i < fiveCards.Count; i++)
        {
            CardStringParser.ParsedCard parsed = CardStringParser.FromInt(fiveCards[i]);
            values.Add((int)parsed.Value);
            suits.Add((int)parsed.Suit);
        }

        values.Sort((x, y) => y.CompareTo(x));

        bool isFlush = true;
        for (int i = 1; i < suits.Count; i++)
        {
            if (suits[i] != suits[0])
            {
                isFlush = false;
                break;
            }
        }

        int straightHigh = GetStraightHigh(values);
        bool isStraight  = straightHigh > 0;

        Dictionary<int, int> counts = new Dictionary<int, int>();
        foreach (int v in values)
        {
            if (!counts.ContainsKey(v)) counts[v] = 0;
            counts[v]++;
        }

        List<KeyValuePair<int, int>> groups = new List<KeyValuePair<int, int>>(counts);
        groups.Sort((x, y) =>
        {
            int byCount = y.Value.CompareTo(x.Value);
            if (byCount != 0) return byCount;
            return y.Key.CompareTo(x.Key);
        });

        // Note: BestFiveCards is assigned by caller (EvaluateBestOfSeven)
        // We only need to return category + kickers here.

        // ✅ Royal Flush handling (Category 9)
        if (isStraight && isFlush)
        {
            // Royal Flush = A-K-Q-J-10 same suit => straight high is Ace (14)
            if (straightHigh == 14)
                return new HandRank(9, new List<int> { 14 }, fiveCards);

            return new HandRank(8, new List<int> { straightHigh }, fiveCards);
        }

        if (groups[0].Value == 4)
            return new HandRank(7, new List<int> { groups[0].Key, groups[1].Key }, fiveCards);

        if (groups[0].Value == 3 && groups[1].Value == 2)
            return new HandRank(6, new List<int> { groups[0].Key, groups[1].Key }, fiveCards);

        if (isFlush)
            return new HandRank(5, new List<int>(values), fiveCards);

        if (isStraight)
            return new HandRank(4, new List<int> { straightHigh }, fiveCards);

        if (groups[0].Value == 3)
        {
            List<int> result = new List<int> { groups[0].Key };
            List<int> kickers = new List<int>();
            for (int i = 1; i < groups.Count; i++)
                kickers.Add(groups[i].Key);
            kickers.Sort((x, y) => y.CompareTo(x));
            result.AddRange(kickers);
            return new HandRank(3, result, fiveCards);
        }

        if (groups[0].Value == 2 && groups[1].Value == 2)
        {
            int highPair = Mathf.Max(groups[0].Key, groups[1].Key);
            int lowPair  = Mathf.Min(groups[0].Key, groups[1].Key);
            int kicker   = groups.Count > 2 ? groups[2].Key : 0;
            return new HandRank(2, new List<int> { highPair, lowPair, kicker }, fiveCards);
        }

        if (groups[0].Value == 2)
        {
            List<int> result = new List<int> { groups[0].Key };
            List<int> kickers = new List<int>();
            for (int i = 1; i < groups.Count; i++)
                kickers.Add(groups[i].Key);
            kickers.Sort((x, y) => y.CompareTo(x));
            result.AddRange(kickers);
            return new HandRank(1, result, fiveCards);
        }

        return new HandRank(0, new List<int>(values), fiveCards);
    }

    private static int GetStraightHigh(List<int> values)
    {
        HashSet<int> unique = new HashSet<int>(values);
        List<int> vals = new List<int>(unique);
        vals.Sort();

        if (vals.Contains(14))
            vals.Insert(0, 1);

        int run = 1;
        int bestHigh = 0;

        for (int i = 1; i < vals.Count; i++)
        {
            if (vals[i] == vals[i - 1] + 1)
            {
                run++;
                if (run >= 5)
                    bestHigh = vals[i];
            }
            else if (vals[i] != vals[i - 1])
            {
                run = 1;
            }
        }

        return bestHigh;
    }

    // ── Hand name strings ─────────────────────────────────────────────────────

    private static string HandCategoryName(int category)
    {
        switch (category)
        {
            case 9: return "Royal Flush";
            case 8: return "Straight Flush";
            case 7: return "Four of a Kind";
            case 6: return "Full House";
            case 5: return "Flush";
            case 4: return "Straight";
            case 3: return "Three of a Kind";
            case 2: return "Two Pair";
            case 1: return "One Pair";
            case 0: return "High Card";
            default: return "";
        }
    }
}