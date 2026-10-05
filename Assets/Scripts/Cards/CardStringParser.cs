using SimplePoker.Logic;
using SimplePoker.ScriptableObjects;
using UnityEngine;

namespace TexasHoldem
{
    public static class CardStringParser
    {
        public struct ParsedCard
        {
            public Card.VALUE Value;
            public Card.SUIT  Suit;
            public int        SpriteIndex; // 0–51 into DeckData.Sprites_CardFront
            public bool       IsHidden;
            public bool       IsValid;
        }

        // ── Parse from integer (u8, 0–51) ────────────────────────────────────
        // Used by all bridge events: HandDealt, ActionTaken, HandCompleted

        public static ParsedCard FromInt(int cardByte)
        {
            if (cardByte < 0 || cardByte > 51)
            {
                Debug.LogWarning($"[CardStringParser] Card byte {cardByte} out of range 0–51.");
                return new ParsedCard { IsValid = false };
            }

            int rankInt = cardByte / 4 + 2; // 2–14
            int suitInt = cardByte % 4;     // 0=Clubs, 1=Diamonds, 2=Hearts, 3=Spades

            Card.VALUE value = IntToValue(rankInt);
            Card.SUIT  suit  = IntToSuit(suitInt);
            int spriteIndex  = GetSpriteIndex(value, suit);

            return new ParsedCard
            {
                Value       = value,
                Suit        = suit,
                SpriteIndex = spriteIndex,
                IsHidden    = false,
                IsValid     = true
            };
        }

        // ── Parse from string ("Ah", "Kd", "??") ─────────────────────────────
        // Used by fog-of-war state JSON (my_hole_names, community_names)

        public static ParsedCard Parse(string cardString)
        {
            if (cardString == "??")
                return new ParsedCard { IsHidden = true, IsValid = true };

            if (string.IsNullOrWhiteSpace(cardString) || cardString.Length != 2)
            {
                Debug.LogWarning($"[CardStringParser] Invalid card string: '{cardString}'");
                return new ParsedCard { IsValid = false };
            }

            if (!TryParseRank(cardString[0], out Card.VALUE value))
            {
                Debug.LogWarning($"[CardStringParser] Unknown rank: '{cardString[0]}'");
                return new ParsedCard { IsValid = false };
            }

            if (!TryParseSuit(cardString[1], out Card.SUIT suit))
            {
                Debug.LogWarning($"[CardStringParser] Unknown suit: '{cardString[1]}'");
                return new ParsedCard { IsValid = false };
            }

            return new ParsedCard
            {
                Value       = value,
                Suit        = suit,
                SpriteIndex = GetSpriteIndex(value, suit),
                IsHidden    = false,
                IsValid     = true
            };
        }

        public static bool IsHidden(string cardString) => cardString == "??";

        // ── Sprite lookup ─────────────────────────────────────────────────────
        // DeckData order: Diamonds(0–12), Spades(13–25), Hearts(26–38), Clubs(39–51)

        public static Sprite GetSprite(ParsedCard card, DeckData deckData)
        {
            if (!card.IsValid || card.IsHidden || deckData == null)
                return deckData?.Sprite_CardBack;

            if (card.SpriteIndex < 0 || card.SpriteIndex >= deckData.Sprites_CardFront.Length)
            {
                Debug.LogWarning($"[CardStringParser] Sprite index {card.SpriteIndex} out of range.");
                return deckData.Sprite_CardBack;
            }

            return deckData.Sprites_CardFront[card.SpriteIndex];
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static Card.VALUE IntToValue(int rank)
        {
            switch (rank)
            {
                case 2:  return Card.VALUE.TWO;
                case 3:  return Card.VALUE.THREE;
                case 4:  return Card.VALUE.FOUR;
                case 5:  return Card.VALUE.FIVE;
                case 6:  return Card.VALUE.SIX;
                case 7:  return Card.VALUE.SEVEN;
                case 8:  return Card.VALUE.EIGHT;
                case 9:  return Card.VALUE.NINE;
                case 10: return Card.VALUE.TEN;
                case 11: return Card.VALUE.JACK;
                case 12: return Card.VALUE.QUEEN;
                case 13: return Card.VALUE.KING;
                case 14: return Card.VALUE.ACE;
                default: return Card.VALUE.NOTHING;
            }
        }

        private static Card.SUIT IntToSuit(int suit)
        {
            switch (suit)
            {
                case 0: return Card.SUIT.CLUBS;
                case 1: return Card.SUIT.DIAMONDS;
                case 2: return Card.SUIT.HEART;
                case 3: return Card.SUIT.SPADES;
                default: return Card.SUIT.CLUBS;
            }
        }

        private static bool TryParseRank(char c, out Card.VALUE value)
        {
            switch (c)
            {
                case '2': value = Card.VALUE.TWO;   return true;
                case '3': value = Card.VALUE.THREE; return true;
                case '4': value = Card.VALUE.FOUR;  return true;
                case '5': value = Card.VALUE.FIVE;  return true;
                case '6': value = Card.VALUE.SIX;   return true;
                case '7': value = Card.VALUE.SEVEN; return true;
                case '8': value = Card.VALUE.EIGHT; return true;
                case '9': value = Card.VALUE.NINE;  return true;
                case 'T': value = Card.VALUE.TEN;   return true;
                case 'J': value = Card.VALUE.JACK;  return true;
                case 'Q': value = Card.VALUE.QUEEN; return true;
                case 'K': value = Card.VALUE.KING;  return true;
                case 'A': value = Card.VALUE.ACE;   return true;
                default:  value = Card.VALUE.NOTHING; return false;
            }
        }

        private static bool TryParseSuit(char c, out Card.SUIT suit)
        {
            switch (c)
            {
                case 'c': suit = Card.SUIT.CLUBS;    return true;
                case 'd': suit = Card.SUIT.DIAMONDS; return true;
                case 'h': suit = Card.SUIT.HEART;    return true;
                case 's': suit = Card.SUIT.SPADES;   return true;
                default:  suit = Card.SUIT.CLUBS;    return false;
            }
        }

        private static int GetSpriteIndex(Card.VALUE value, Card.SUIT suit)
        {
            return GetSuitOffset(suit) + GetRankOffset(value);
        }

        private static int GetSuitOffset(Card.SUIT suit)
        {
            switch (suit)
            {
                case Card.SUIT.DIAMONDS: return 0;
                case Card.SUIT.SPADES:   return 13;
                case Card.SUIT.HEART:    return 26;
                case Card.SUIT.CLUBS:    return 39;
                default:                 return 0;
            }
        }

        private static int GetRankOffset(Card.VALUE value)
        {
            return Mathf.Clamp((int)value - 2, 0, 12);
        }
    }
}