using System;
using System.Collections.Generic;

namespace TexasHoldem
{
    public static class PokerTableEvents
    {
        // ── Match lifecycle ───────────────────────────────────────────────────

        public static Action             OnMatchInitialized;
        public static Action<string>     OnMatchOver;                   // winnerName

        // ── Hand lifecycle ────────────────────────────────────────────────────

        public static Action<int>              OnNewHand;               // hand_index
        public static Action<int> OnDealerButtonChanged;

        // winners:   list of winning seat indices (multiple = split pot).
        // pot:       total pot amount awarded this hand.
        public static Action<List<int>, int>   OnHandResult;            // winners, pot

        // ── Winning hand highlight ────────────────────────────────────────────
        // winnerSeats:     list of winning seat indices.
        // bestFivePerSeat: parallel list — each entry is the 5-card int list for
        //                  the corresponding winner in winnerSeats.
        // handName:        human-readable hand rank string (e.g. "Full House").
        //                  On a split pot all winners share the same handName.

        public static Action<List<int>, List<List<int>>, string> OnWinningHandRevealed;

        // ── Staged cleanup ────────────────────────────────────────────────────

        public static Action OnClearActionLabels;
        public static Action OnClearBoardAndPot;
        public static Action OnClearHoleCards;
        public static Action OnClearActionLog;

        // ── Street ───────────────────────────────────────────────────────────

        public static Action<int>        OnStreetChanged;               // street int (0–4)

        // ── Pot ──────────────────────────────────────────────────────────────

        public static Action<int, int>   OnPotChanged;                  // prev, next

        // ── Chip animations ───────────────────────────────────────────────────

        public static Action<int, int>   OnChipsToPot;                  // seatIndex, amount
        public static Action<Action> OnCollectBetsToPot;                        // all betting boxes -> pot
        // winnerSeats: todos los asientos que reciben una parte del bote.
        // pot: bote total de la mano.
        public static Action<List<int>, int> OnPotAwarded;         // seatIndex, amount

        // ── Action seat ──────────────────────────────────────────────────────
        // null = no seat currently highlighted (between streets or between hands).
        // Drives active-seat highlighting (ring, roster row, turn queue, log).
        // NOTE: does NOT by itself mean "thinking" — see OnSeatThinkingChanged.

        public static Action<int?>       OnActionSeatChanged;           // seatIndex; null = none

        // ── Seat thinking (speculative) ───────────────────────────────────────
        // Fired ONLY when Unity is guessing ahead of the backend which seat will
        // act next, before the real ActionTaken/EngineActionTaken event for it
        // has arrived — i.e. a genuine "waiting on backend data" loading state.
        // null = not speculating on anyone (blinds posting, community cards
        // revealing, re-displaying an already-resolved action, etc.).
        // Drives the "Thinking..." dot animation in UIPlayer and FeaturedSeatDisplay.

        public static Action<int?>       OnSeatThinkingChanged;         // seatIndex; null = none

        // ── Community cards ───────────────────────────────────────────────────

        public static Action<List<int>>  OnCommunityCardsChanged;       // u8 card ints

        // ── Emotion ──────────────────────────────────────────────────────────
        // Fired once per seat per ActionTaken that has an emotion update.
        // emotionId is always 0–6 here — ActionManager never fires this with -1.

        public static Action<int, int>   OnEmotionChanged;              // seatIndex, emotionId

        // ── Action played ─────────────────────────────────────────────────────
        // Drives the LastActionLabel on each compact table node.
        // actionName: human-readable string from ActionNames.FromInt().
        // amount:     chip amount (0 for check/fold).

        public static Action<int, string, int> OnActionPlayed;          // seatIndex, actionName, amount

        // ── Banter ────────────────────────────────────────────────────────────

        public static Action<int, string> OnBanterReceived;             // seatIndex, text

        // ── Elimination ───────────────────────────────────────────────────────
        // OnSeatEliminated:    fired immediately when a seat hits 0 chips (end of hand).
        //                      Compact node deactivates. RosterRow switches to eliminated state.
        // OnActiveSeatsChanged: fired after gap-close animation is complete and roster
        //                       has reordered. Listeners receive the full list of seat
        //                       indices still active in the match.

        public static Action<int>         OnSeatEliminated;             // seatIndex
        public static Action<List<int>>   OnActiveSeatsChanged;         // remaining active seat indices

        // ── Table node fade ───────────────────────────────────────────────────
        // OnFadeOutTableNodes: fired after hole cards clear — compact nodes fade to 0.
        //   UIPlayer fades seatCanvasGroup out. Node stays invisible through reset.
        // OnFadeInTableNodes:  fired after Phase-0 clean-slate sync in next HandDealt,
        //   once positions and UI are fully reset. Nodes fade back to alpha 1.

        public static Action             OnFadeOutTableNodes;
        public static Action             OnFadeInTableNodes;
    }
}