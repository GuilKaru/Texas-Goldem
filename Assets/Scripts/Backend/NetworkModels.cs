using System;
using System.Collections.Generic;

namespace TexasHoldem
{
    // ── Top-level envelope ────────────────────────────────────────────────────

    [Serializable]
    public class BridgeEnvelope
    {
        public string kind;
        public string schema_version;
        public string tx_digest;
        public string checkpoint_seq;
    }

    // ── Seat setup entry — used inside MatchSetupPayload ──────────────────────

    [Serializable]
    public class SeatSetupData
    {
        public int    seat_index;
        public string avatar_id;    // slug, e.g. "trump", "mr_beast"
        public string agent_name;   // display name shown in seat UI
    }

    // ── MatchCreated ──────────────────────────────────────────────────────────

    [Serializable]
    public class MatchCreatedEvent : BridgeEnvelope
    {
        public string match_id;
    }

    // ── HandDealt ─────────────────────────────────────────────────────────────
    // stacks:     one entry per seat, post-blind stacks.
    // hole_cards: one List<int> per seat, each inner list is [card0, card1].
    //             All cards sent face-up — this is a spectator game.
    // small_blind_seat / big_blind_seat: explicit seat indices, not amounts.
    // small_blind_amount / big_blind_amount: chip amounts for the blind intro sequence.

    [Serializable]
    public class HandDealtEvent : BridgeEnvelope
    {
        public string          match_id;
        public int             hand_index;
        public int             button;
        public int             small_blind_seat;
        public int             big_blind_seat;
        public int             small_blind_amount;
        public int             big_blind_amount;
        public int             pot;
        public List<int>       stacks;       // length = seat_count
        public List<List<int>> hole_cards;   // length = seat_count, each entry = [card0, card1]
    }

    // ── ActionTaken ───────────────────────────────────────────────────────────
    // stacks:   full list of all seat stacks at this moment (length = seat_count).
    // emotions: emotion id per seat (length = seat_count).
    //           -1 = no emotion update for that seat this action.

    [Serializable]
    public class ActionTakenEvent : BridgeEnvelope
    {
        public string    match_id;
        public int       hand_index;
        public int       street;
        public int       seat;
        public int       action_type;
        public int       amount;
        public int       pot;
        public List<int> stacks;           // length = seat_count
        public List<int> community_cards;
        public List<int> emotions;         // length = seat_count, -1 = no update
    }

    // ── EngineActionTaken — AdvanceStreet only ────────────────────────────────
    // Fired by React when the engine advances the street.
    // winners: list of winning seat indices (empty if hand still ongoing).
    // hole_cards: all seats' cards at this point (may be [[-1,-1],...] if not revealed yet).
    // small_blind_seat / big_blind_seat: explicit seat indices.

    [Serializable]
    public class EngineActionTakenEvent : BridgeEnvelope
    {
        public string          match_id;
        public int             hand_index;
        public int             action_type;
        public int             street;
        public int             button;
        public int             small_blind_seat;
        public int             big_blind_seat;
        public int             small_blind_amount;
        public int             big_blind_amount;
        public int             pot;
        public int             awarded_pot;
        public List<int>       stacks;           // length = seat_count
        public int             ended_by;
        public List<int>       winners;          // seat indices; empty = hand still running
        public int             total_hands;
        public List<List<int>> hole_cards;       // length = seat_count, each = [card0, card1]
        public List<int>       community_cards;
    }

    // ── BanterEvent ───────────────────────────────────────────────────────────
    // banter_seat: 0–5 — which seat is speaking.
    // banter_text: the line to display.
    // Unity does NOT know the receiver. The right-side avatar in BanterBar only
    // appears if a second BanterEvent fires while the first is still displayed.

    [Serializable]
    public class BanterEvent : BridgeEnvelope
    {
        public string match_id;
        public int    hand_index;
        public int    banter_seat;
        public string banter_text;
    }

    // ── HandCompleted ─────────────────────────────────────────────────────────
    // winners:          list of winning seat indices (multiple = split pot).
    // stacks_after:     one entry per seat, post-hand stacks (length = seat_count).
    // hole_cards:       all seats revealed face-up (length = seat_count).
    // eliminated_seats: seat indices that hit 0 chips this hand (may be empty).

    [Serializable]
    public class HandCompletedEvent : BridgeEnvelope
    {
        public string          match_id;
        public int             hand_index;
        public int             ended_by;
        public List<int>       winners;           // seat indices
        public int             pot;
        public List<int>       community_cards;
        public List<List<int>> hole_cards;        // length = seat_count, each = [card0, card1]
        public List<int>       stacks_after;      // length = seat_count
        public List<int>       eliminated_seats;  // seat indices at 0 chips; may be empty
    }

    // ── MatchCompleted ────────────────────────────────────────────────────────
    // winner:       seat index of the match winner.
    // final_stacks: one entry per seat (length = seat_count). Winner has all chips, others 0.

    [Serializable]
    public class MatchCompletedEvent : BridgeEnvelope
    {
        public string    match_id;
        public int       winner;
        public List<int> final_stacks;   // length = seat_count
        public int       total_hands;
    }

    // ── Internal display state — not a wire format ────────────────────────────
    // Built and maintained by ActionManager. Represents the current visual state
    // of the table. Dynamically sized to seat_count at match start.

    [Serializable]
    public class MatchDisplayState
    {
        public string             match_id;
        public int                hand_index;
        public int                button;
        public int                small_blind_seat;    // explicit seat index
        public int                big_blind_seat;      // explicit seat index
        public int                small_blind_amount;
        public int                big_blind_amount;
        public int                pot;
        public int                street;
        public List<int>          community_cards  = new List<int>();
        public int                to_act           = -1;  // -1 = nobody (between streets)
        public bool               match_over;
        public int                match_winner     = -1;
        public SeatDisplayState[] seats;                  // length = seat_count, set at init
    }

    [Serializable]
    public class SeatDisplayState
    {
        public int  seat_index;
        public int  stack;
        public int  current_bet;
        public bool is_dealer;
        public bool is_small_blind;
        public bool is_big_blind;
        public bool is_folded;
        public bool is_all_in;
        public bool is_eliminated;
        public int  hole_card_0 = -1;
        public int  hole_card_1 = -1;
        public int  emotion     = -1;   // 0–6 (-1 = none received yet)
    }

    // ── Action log entry (unchanged) ──────────────────────────────────────────

    [Serializable]
    public class ActionLogEntry
    {
        public int street;
        public int seat;
        public int action_type;
        public int amount;
    }

    // ── Setup payload — sent by React via InitializeMatch ─────────────────────
    // seat_count: number of players in this match (2–6).
    // seats:      one SeatSetupData per player, in seat index order.

    [Serializable]
    public class MatchSetupPayload
    {
        public string             match_id;
        public int                seat_count;
        public int                starting_stack;
        public int                small_blind;
        public int                big_blind;
        public int                hands_per_level = 4;
        public List<SeatSetupData> seats;          // length = seat_count
    }

    // ── Match stats ───────────────────────────────────────────────────────────
    // Sent by React via WebGLReceiver.ReceiveMatchStats(json).
    // React derives all fields from full blockchain event history.
    // placement: 1 = match winner, N = first eliminated.
    //   React sends this pre-sorted; Unity sorts leaderboard by placement ascending.

    [Serializable]
    public class MatchStatsSeatPayload
    {
        public int    seat_index;
        public string handle;
        public string avatar_id;
        public int    placement;    // 1 = winner, N = first eliminated
        public int    folds;
        public int    raises;
        public int    calls;
        public int    hands_won;
    }

    [Serializable]
    public class MatchStatsPayload
    {
        public int                         total_hands;
        public int                         duration_seconds;
        public List<MatchStatsSeatPayload> seats;
    }

    // ── Market data — pushed by React on a polling interval ──────────────────
    // React calls: SendMessage("WebGLReceiver", "ReceiveMarketData", json)
    // React calls: SendMessage("WebGLReceiver", "StopMarketData",   "")
    //
    // Only active (non-eliminated) seats are included in the seats list.
    // Eliminated seats are absent — RosterRow shows "--" for both fields.
    //
    // price:    outcome probability from Kash API. Range: 0 < price < 1.
    //           Displayed as: {value}%
    // slippage: $100 buy slippage %; can be negative.
    //           Displayed as: To win +${value}

    [Serializable]
    public class MarketSeatData
    {
        public int   seat_index;
        public float price;       // 0 < x < 1 — displayed as {value}%
        public float slippage;    // can be negative — displayed as To win +${value}
    }

    [Serializable]
    public class MarketDataPayload
    {
        public List<MarketSeatData> seats;   // active seats only; eliminated seats absent
    }

    // ── Static helpers ────────────────────────────────────────────────────────

    public static class StreetNames
    {
        public static string FromInt(int street)
        {
            switch (street)
            {
                case 0:  return "Preflop";
                case 1:  return "Flop";
                case 2:  return "Turn";
                case 3:  return "River";
                case 4:  return "Showdown";
                default: return "";
            }
        }
    }

    public static class ActionNames
    {
        public static string FromInt(int actionType)
        {
            switch (actionType)
            {
                case 0:  return "Fold";
                case 1:  return "Check";
                case 2:  return "Call";
                case 3:  return "Bet";
                case 4:  return "Raise";
                default: return "Unknown";
            }
        }
    }
}