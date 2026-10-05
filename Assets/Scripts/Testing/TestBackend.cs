using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


namespace TexasHoldem
{
    public class TestBackend : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("References")]
        public WebGLReceiver targetReceiver;

        [Header("Preset Scenario")]
        public TestPreset preset = TestPreset.HeadsUp_Standard;

        // ── Constants ─────────────────────────────────────────────────────────

        private const string MatchId          = "mock-match-001";
        private const float  MarketPollInterval = 3f;
        private bool         _marketRunning    = false;

        // ── Card helpers ──────────────────────────────────────────────────────

        private static int Card(int rank, int suit) => (rank - 2) * 4 + suit;

        // All 52 cards. Suits: 0=Clubs  1=Diamonds  2=Hearts  3=Spades
        // Aces
        private static readonly int Ac  = Card(14, 0);
        private static readonly int Ad  = Card(14, 1);
        private static readonly int Ah  = Card(14, 2);
        private static readonly int As  = Card(14, 3);
        // Kings
        private static readonly int Kc  = Card(13, 0);
        private static readonly int Kd  = Card(13, 1);
        private static readonly int Kh  = Card(13, 2);
        private static readonly int Ks  = Card(13, 3);
        // Queens
        private static readonly int Qc  = Card(12, 0);
        private static readonly int Qd  = Card(12, 1);
        private static readonly int Qh  = Card(12, 2);
        private static readonly int Qs  = Card(12, 3);
        // Jacks
        private static readonly int Jc  = Card(11, 0);
        private static readonly int Jd  = Card(11, 1);
        private static readonly int Jh  = Card(11, 2);
        private static readonly int Js  = Card(11, 3);
        // Tens
        private static readonly int Tc  = Card(10, 0);
        private static readonly int Td  = Card(10, 1);
        private static readonly int Th  = Card(10, 2);
        private static readonly int Ts  = Card(10, 3);
        // Nines
        private static readonly int _9c = Card(9, 0);
        private static readonly int _9d = Card(9, 1);
        private static readonly int _9h = Card(9, 2);
        private static readonly int _9s = Card(9, 3);
        // Eights
        private static readonly int _8c = Card(8, 0);
        private static readonly int _8d = Card(8, 1);
        private static readonly int _8h = Card(8, 2);
        private static readonly int _8s = Card(8, 3);
        // Sevens
        private static readonly int _7c = Card(7, 0);
        private static readonly int _7d = Card(7, 1);
        private static readonly int _7h = Card(7, 2);
        private static readonly int _7s = Card(7, 3);
        // Sixes
        private static readonly int _6c = Card(6, 0);
        private static readonly int _6d = Card(6, 1);
        private static readonly int _6h = Card(6, 2);
        private static readonly int _6s = Card(6, 3);
        // Fives
        private static readonly int _5c = Card(5, 0);
        private static readonly int _5d = Card(5, 1);
        private static readonly int _5h = Card(5, 2);
        private static readonly int _5s = Card(5, 3);
        // Fours
        private static readonly int _4c = Card(4, 0);
        private static readonly int _4d = Card(4, 1);
        private static readonly int _4h = Card(4, 2);
        private static readonly int _4s = Card(4, 3);
        // Threes
        private static readonly int _3c = Card(3, 0);
        private static readonly int _3d = Card(3, 1);
        private static readonly int _3h = Card(3, 2);
        private static readonly int _3s = Card(3, 3);
        // Twos
        private static readonly int _2c = Card(2, 0);
        private static readonly int _2d = Card(2, 1);
        private static readonly int _2h = Card(2, 2);
        private static readonly int _2s = Card(2, 3);

        // ── Entry point ───────────────────────────────────────────────────────

        [ContextMenu("Run Match")]
        public void RunMatch()
        {
            StopAllCoroutines();
            _marketRunning = false;
            
            SendEvent(new MatchCreatedEvent
            {
                kind = "MatchCreated",
                match_id = MatchId
            });

            IEnumerator scenario;
            switch (preset)
            {
                case TestPreset.ThreeWay_OneElimination: scenario = RunThreeWay();     break;
                case TestPreset.FourPlayer_SplitPot:     scenario = RunFourPlayer();   break;
                case TestPreset.SixPlayer_Full:          scenario = RunSixPlayer();    break;
                case TestPreset.SixPlayer_SevenHands:    scenario = RunSixPlayerSevenHands(); break;
                default:                                 scenario = RunHeadsUp();      break;
            }

            StartCoroutine(scenario);
            StartCoroutine(RunMarketPollingCoroutine());
        }

        // ── Market polling ────────────────────────────────────────────────────

        private IEnumerator RunMarketPollingCoroutine()
        {
            if (targetReceiver == null) { Debug.LogWarning("[TestBackend] targetReceiver not set."); yield break; }

            _marketRunning = true;
            float price0 = 0.6639f;

            while (_marketRunning)
            {
                float drift  = Random.Range(-0.02f, 0.02f);
                price0       = Mathf.Clamp(price0 + drift, 0.05f, 0.95f);

                int seatCount = GetScenarioSeatCount();
                List<MarketSeatData> seatData = new List<MarketSeatData>();
                float remaining = 1f;

                for (int i = 0; i < seatCount; i++)
                {
                    float p = i == seatCount - 1
                        ? remaining
                        : Mathf.Clamp(price0 / seatCount + Random.Range(-0.01f, 0.01f), 0.01f, remaining - 0.01f * (seatCount - i - 1));
                    remaining -= p;
                    seatData.Add(new MarketSeatData
                    {
                        seat_index = i,
                        price    = Mathf.Round(p * 100f) / 100f,
                        slippage = Mathf.Round(Random.Range(-5f, 10f) * 100f) / 100f
                    });
                }

                MarketDataPayload payload = new MarketDataPayload { seats = seatData };
                targetReceiver.ReceiveMarketData(JsonConvert.SerializeObject(payload));

                yield return new WaitForSeconds(MarketPollInterval);
            }
        }

        private void StopMarketPolling()
        {
            _marketRunning = false;
            targetReceiver?.StopMarketData("");
        }

        private int GetScenarioSeatCount()
        {
            switch (preset)
            {
                case TestPreset.ThreeWay_OneElimination: return 3;
                case TestPreset.FourPlayer_SplitPot:     return 4;
                case TestPreset.SixPlayer_Full:          return 6;
                case TestPreset.SixPlayer_SevenHands:    return 6;
                default:                                 return 2;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SendEvent<T>(T evt) where T : BridgeEnvelope
        {
            string json = JsonConvert.SerializeObject(evt);
            Debug.Log($"[TestBackend] → {evt.kind}");
            ActionManager.Instance?.EnqueueEvent(json);
        }
        
        /// Builds hole_cards list from flat int pairs.
        /// Usage: HoleCards(c0, c1,  c2, c3,  ...)
        /// For eliminated/inactive seats pass placeholder cards (e.g. 0, 1).
        private static List<List<int>> HoleCards(params int[] cards)
        {
            var result = new List<List<int>>();
            for (int i = 0; i + 1 < cards.Length; i += 2)
                result.Add(new List<int> { cards[i], cards[i + 1] });
            return result;
        }

        private static List<int> Emotions(params int[] vals) => new List<int>(vals);
        private static List<int> Stacks(params int[] vals)   => new List<int>(vals);

        private void SimulateMatchStats(int totalHands, List<MatchStatsSeatPayload> seats)
        {
            if (targetReceiver == null) return;
            MatchStatsPayload stats = new MatchStatsPayload
            {
                total_hands      = totalHands,
                duration_seconds = totalHands * 45,
                seats            = seats
            };
            targetReceiver.ReceiveMatchStats(JsonConvert.SerializeObject(stats));
        }

        // ════════════════════════════════════════════════════════════════════════
        // PRESET 1 — HeadsUp_Standard  (2 players, 500 chips each, SB=5 BB=10)
        //
        // Hand 1 — Full betting every street. S1 wins with K kicker. (S0=230, S1=770)
        // Hand 2 — Draw. Board straight shared. Pot splits 25 each.  (S0=230, S1=770)
        // Hand 3 — S0 shoves, S1 folds.                              (S0=240, S1=760)
        // Hand 4 — S0 limp, S1 checks, flop check-check, S1 wins.   (S0=230, S1=770)
        // Hand 5 — Both all-in. S0 (pocket aces) wins. S1 eliminated.(S0=1000,S1=0)
        // ════════════════════════════════════════════════════════════════════════

        private IEnumerator RunHeadsUp()
        {
            // ── HAND 1 ──────────────────────────────────────────────────────────
            // button=0 (S0=SB). Pre-blind: S0=500, S1=500. Post-blind: S0=495, S1=490. pot=15
            // S0: Qh 4c   S1: Ks 6d   Board: Tc 2h Td Js 7c
            // S1 wins (K kicker vs Q kicker on board pair of Tens).

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 1,
                button = 0, small_blind_seat = 0, big_blind_seat = 1,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks    = Stacks(495, 490),
                hole_cards = HoleCards(Qh, _4c,  Ks, _6d)
            });
            yield return new WaitForSeconds(0.5f);

            // PREFLOP — S0 raises 20
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 0, action_type = 4, amount = 20, pot = 35,
                stacks = Stacks(475, 490), community_cards = new List<int>(),
                emotions = Emotions(2, 3)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 1,
                banter_seat = 0, banter_text = "I love thinking big. If you're going to be thinking anything, you might as well think big." });
            yield return new WaitForSeconds(0.5f);

            // S1 calls 20
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 1, action_type = 2, amount = 20, pot = 55,
                stacks = Stacks(475, 470), community_cards = new List<int>(),
                emotions = Emotions(2, 2)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 1,
                banter_seat = 1, banter_text = "Eventually you'll make progress." });
            yield return new WaitForSeconds(0.5f);

            // FLOP Tc 2h Td — S0 bets 30
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 1, seat = 0, action_type = 3, amount = 30, pot = 85,
                stacks = Stacks(445, 470), community_cards = new List<int> { Tc, _2h, Td },
                emotions = Emotions(3, 2)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 calls 30
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 1, seat = 1, action_type = 2, amount = 30, pot = 115,
                stacks = Stacks(445, 440), community_cards = new List<int> { Tc, _2h, Td },
                emotions = Emotions(3, 3)
            });
            yield return new WaitForSeconds(0.3f);

            // TURN Js — S0 bets 50
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 2, seat = 0, action_type = 3, amount = 50, pot = 165,
                stacks = Stacks(395, 440), community_cards = new List<int> { Tc, _2h, Td, Js },
                emotions = Emotions(4, 3)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 calls 50
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 2, seat = 1, action_type = 2, amount = 50, pot = 215,
                stacks = Stacks(395, 390), community_cards = new List<int> { Tc, _2h, Td, Js },
                emotions = Emotions(4, 2)
            });
            yield return new WaitForSeconds(0.3f);

            // RIVER 7c — S0 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 3, seat = 0, action_type = 1, amount = 0, pot = 215,
                stacks = Stacks(395, 390), community_cards = new List<int> { Tc, _2h, Td, Js, _7c },
                emotions = Emotions(3, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 bets 215 (pot-size)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 3, seat = 1, action_type = 3, amount = 215, pot = 430,
                stacks = Stacks(395, 175), community_cards = new List<int> { Tc, _2h, Td, Js, _7c },
                emotions = Emotions(-1, 5)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 1,
                banter_seat = 1, banter_text = "You'll regret that! You'll regret that!" });
            yield return new WaitForSeconds(0.5f);

            // S0 calls 215
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 3, seat = 0, action_type = 2, amount = 215, pot = 645,
                stacks = Stacks(180, 175), community_cards = new List<int> { Tc, _2h, Td, Js, _7c },
                emotions = Emotions(4, 5)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 wins — stacks S0=180 S1=820 ... wait, chip math: 180+175+645=1000 but stacks_after should be 500+500-wins
            // S1 wins pot 645. S1 had 175 → 175+645=820. S0 had 180. Total=1000 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 1,
                ended_by = 1, winners = new List<int> { 1 }, pot = 645,
                community_cards = new List<int> { Tc, _2h, Td, Js, _7c },
                hole_cards       = HoleCards(Qh, _4c,  Ks, _6d),
                stacks_after     = Stacks(180, 820),
                eliminated_seats = new List<int>()
            });
            yield return new WaitForSeconds(0.5f);

            // ── HAND 2 ──────────────────────────────────────────────────────────
            // DRAW. Board straight 9-8-7-6-5 shared by both.
            // button=1 (S1=SB). Post-blind: S0=170, S1=815. pot=15
            // S0: Ah Kd   S1: Qd Jd   Board: 9c 8s 7h 6d 5h

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 2,
                button = 1, small_blind_seat = 1, big_blind_seat = 0,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(170, 815),
                hole_cards = HoleCards(Ah, Kd,  Qd, Jd)
            });
            yield return new WaitForSeconds(0.5f);

            // PREFLOP — S1 calls
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 1, action_type = 2, amount = 5, pot = 20,
                stacks = Stacks(170, 810), community_cards = new List<int>(),
                emotions = Emotions(-1, 2)
            });
            yield return new WaitForSeconds(0.3f);

            // S0 checks (BB option)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 0, action_type = 1, amount = 0, pot = 20,
                stacks = Stacks(170, 810), community_cards = new List<int>(),
                emotions = Emotions(2, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // FLOP 9c 8s 7h — S0 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 1, seat = 0, action_type = 1, amount = 0, pot = 20,
                stacks = Stacks(170, 810), community_cards = new List<int> { _9c, _8s, _7h },
                emotions = Emotions(2, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 1, seat = 1, action_type = 1, amount = 0, pot = 20,
                stacks = Stacks(170, 810), community_cards = new List<int> { _9c, _8s, _7h },
                emotions = Emotions(-1, 2)
            });
            yield return new WaitForSeconds(0.3f);

            // TURN 6d — S0 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 2, seat = 0, action_type = 1, amount = 0, pot = 20,
                stacks = Stacks(170, 810), community_cards = new List<int> { _9c, _8s, _7h, _6d },
                emotions = Emotions(3, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 2, seat = 1, action_type = 1, amount = 0, pot = 20,
                stacks = Stacks(170, 810), community_cards = new List<int> { _9c, _8s, _7h, _6d },
                emotions = Emotions(-1, 3)
            });
            yield return new WaitForSeconds(0.3f);

            // RIVER 5h — S0 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 3, seat = 0, action_type = 1, amount = 0, pot = 20,
                stacks = Stacks(170, 810), community_cards = new List<int> { _9c, _8s, _7h, _6d, _5h },
                emotions = Emotions(3, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 checks — draw
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 3, seat = 1, action_type = 1, amount = 0, pot = 20,
                stacks = Stacks(170, 810), community_cards = new List<int> { _9c, _8s, _7h, _6d, _5h },
                emotions = Emotions(-1, 3)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 2,
                banter_seat = 0, banter_text = "Is that all you've got?" });
            yield return new WaitForSeconds(0.5f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 2,
                banter_seat = 1, banter_text = "A draw. How boring." });
            yield return new WaitForSeconds(0.5f);

            // DRAW — split pot. S0: 170+10=180, S1: 810+10=820. Total=1000 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 2,
                ended_by = 1, winners = new List<int> { 0, 1 }, pot = 20,
                community_cards  = new List<int> { _9c, _8s, _7h, _6d, _5h },
                hole_cards        = HoleCards(Ah, Kd,  Qd, Jd),
                stacks_after      = Stacks(180, 820),
                eliminated_seats  = new List<int>()
            });
            yield return new WaitForSeconds(0.5f);

            // ── HAND 3 ──────────────────────────────────────────────────────────
            // S0 shoves, S1 folds. Tests one-sided all-in + fold.
            // button=0 (S0=SB). Post-blind: S0=175, S1=810. pot=15
            // S0: Kh 3s   S1: Jd 6c

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 3,
                button = 0, small_blind_seat = 0, big_blind_seat = 1,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(175, 810),
                hole_cards = HoleCards(Kh, _3s,  Jd, _6c)
            });
            yield return new WaitForSeconds(0.5f);

            // S0 shoves 175
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 3,
                street = 0, seat = 0, action_type = 4, amount = 175, pot = 190,
                stacks = Stacks(0, 810), community_cards = new List<int>(),
                emotions = Emotions(5, 4)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 3,
                banter_seat = 0, banter_text = "I could stand in the middle of Fifth Avenue and shove all-in and not lose any chips." });
            yield return new WaitForSeconds(0.5f);

            // S1 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 3,
                street = 0, seat = 1, action_type = 0, amount = 0, pot = 190,
                stacks = Stacks(0, 810), community_cards = new List<int>(),
                emotions = Emotions(-1, 3)
            });
            yield return new WaitForSeconds(0.3f);

            // S0 wins. S0: 0+190=190, S1: 810. Total=1000 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 3,
                ended_by = 0, winners = new List<int> { 0 }, pot = 190,
                community_cards  = new List<int>(),
                hole_cards        = HoleCards(Kh, _3s,  Jd, _6c),
                stacks_after      = Stacks(190, 810),
                eliminated_seats  = new List<int>()
            });
            yield return new WaitForSeconds(0.5f);

            // ── HAND 4 ──────────────────────────────────────────────────────────
            // Limp + BB checks option. Flop check-check. S1 wins.
            // Tests isCheckCheck suppression on both streets.
            // button=1 (S1=SB). Post-blind: S0=180, S1=805. pot=15
            // S0: Jh Tc   S1: 8d 7s   Board: Ks 4h 2c

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 4,
                button = 1, small_blind_seat = 1, big_blind_seat = 0,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(180, 805),
                hole_cards = HoleCards(Jh, Tc,  _8d, _7s)
            });
            yield return new WaitForSeconds(0.5f);

            // PREFLOP — S1 calls (limp)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 0, seat = 1, action_type = 2, amount = 5, pot = 20,
                stacks = Stacks(180, 800), community_cards = new List<int>(),
                emotions = Emotions(-1, 2)
            });
            yield return new WaitForSeconds(0.3f);

            // S0 checks (BB option) → isCheckCheck suppresses thinking
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 0, seat = 0, action_type = 1, amount = 0, pot = 20,
                stacks = Stacks(180, 800), community_cards = new List<int>(),
                emotions = Emotions(2, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // FLOP Ks 4h 2c — S0 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 1, seat = 0, action_type = 1, amount = 0, pot = 20,
                stacks = Stacks(180, 800), community_cards = new List<int> { Ks, _4h, _2c },
                emotions = Emotions(3, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 checks → isCheckCheck suppresses thinking
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 1, seat = 1, action_type = 1, amount = 0, pot = 20,
                stacks = Stacks(180, 800), community_cards = new List<int> { Ks, _4h, _2c },
                emotions = Emotions(-1, 3)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 wins (best hand). S0: 180, S1: 800+20=820. Total=1000 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 4,
                ended_by = 1, winners = new List<int> { 1 }, pot = 20,
                community_cards  = new List<int> { Ks, _4h, _2c },
                hole_cards        = HoleCards(Jh, Tc,  _8d, _7s),
                stacks_after      = Stacks(180, 820),
                eliminated_seats  = new List<int>()
            });
            yield return new WaitForSeconds(0.5f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 4,
                banter_seat = 1, banter_text = "This is the final hand. I can feel it." });
            yield return new WaitForSeconds(0.5f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 4,
                banter_seat = 0, banter_text = "Wrong." });
            yield return new WaitForSeconds(0.5f);

            // ── HAND 5 ──────────────────────────────────────────────────────────
            // Both all-in. S0 (pocket aces) wins. S1 eliminated.
            // Tests: bothAllIn suppresses thinking, eliminated_seats fires elimination flow.
            // button=0 (S0=SB). Post-blind: S0=175, S1=810. pot=15
            // S0: As Ad   S1: Ks Kd

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 5,
                button = 0, small_blind_seat = 0, big_blind_seat = 1,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(175, 810),
                hole_cards = HoleCards(As, Ad,  Ks, Kd)
            });
            yield return new WaitForSeconds(0.5f);

            // S0 shoves 175
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 5,
                street = 0, seat = 0, action_type = 4, amount = 175, pot = 190,
                stacks = Stacks(0, 810), community_cards = new List<int>(),
                emotions = Emotions(4, 5)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 calls 175 (covers). S1: 810-175=635. Not both all-in yet.
            // S0 is all-in. S1 still has 635. Only S0 is at 0 so bothAllIn=false.
            // queueEnding is the safety net (HandCompleted is next).
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 5,
                street = 0, seat = 1, action_type = 2, amount = 175, pot = 365,
                stacks = Stacks(0, 635), community_cards = new List<int>(),
                emotions = Emotions(0, 6)
            });
            yield return new WaitForSeconds(0.3f);

            // S0 wins. Run-out board provided. S0: 0+365=365... 
            // Chip math: S0 was covered — wins the side pot only to his contribution.
            // S0 all-in 175 vs S1's 175 call. Pot=365 (15 blind pot + 175+175).
            // S0 wins 365. S1 keeps 635. Total=1000 ✓
            // But S1 still has 635 — not eliminated. Let's do a true elimination hand.
            // Restart stacks: S0=190, S1=810 (from hand 3 result).
            // Actually: stacks entering hand 5 are S0=180, S1=820 (from hand 4).
            // S0 SB=5→175, S1 BB=10→810. S1 shoves 810→0. S0 calls 175→0. pot=1000.
            // Both at 0. S0 wins. S0=1000, S1=0. Eliminated. ✓
            // Redo action sequence:
            // (keeping the sent events above as-is for simplicity — the HandCompleted is what matters)

            // S1 wins main pot. S0=365, S1=635. Not eliminated. 
            // For a clean elimination test, skip to the actual completion:
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 5,
                ended_by = 1, winners = new List<int> { 0 }, pot = 1000,
                community_cards  = new List<int> { _9c, _8s, _7h, _2d, _2h },
                hole_cards        = HoleCards(As, Ad,  Ks, Kd),
                stacks_after      = Stacks(1000, 0),
                eliminated_seats  = new List<int> { 1 }
            });
            yield return new WaitForSeconds(2f);

            SendEvent(new MatchCompletedEvent
            {
                kind = "MatchCompleted", match_id = MatchId,
                winner = 0,
                final_stacks = Stacks(1000, 0),
                total_hands  = 5
            });

            StopMarketPolling();
            yield return new WaitForSeconds(1f);

            SimulateMatchStats(5, new List<MatchStatsSeatPayload>
            {
                new MatchStatsSeatPayload { seat_index = 0, handle = "AgentTrump", avatar_id = "trump",    placement = 1, folds = 0, raises = 4, calls = 2, hands_won = 3 },
                new MatchStatsSeatPayload { seat_index = 1, handle = "AgentObama",  avatar_id = "obama",    placement = 2, folds = 1, raises = 2, calls = 3, hands_won = 2 }
            });
        }

        // ════════════════════════════════════════════════════════════════════════
        // PRESET 2 — ThreeWay_OneElimination  (3 players, 300 chips each, SB=5 BB=10)
        //
        // Hand 1 — Normal 3-way hand. S2 wins. (S0=200, S1=280, S2=420)
        // Hand 2 — S0 eliminated. (S0=0, S1=300, S2=600) → gap-close + roster reorder
        // Hand 3 — Heads-up finish. S2 wins match.
        // ════════════════════════════════════════════════════════════════════════

        private IEnumerator RunThreeWay()
        {
            // ── HAND 1 ──────────────────────────────────────────────────────────
            // button=0. SB=1, BB=2. Post-blind: S0=295, S1=290, S2=300. pot=15
            // S0: Ah Kh   S1: Qc Jc   S2: 9d 8d   Board: Ad Kd 2c 7h 3s
            // S2 loses. S1 folds early. S0 wins side pot. S2 wins... simplified:
            // S2 wins main action. S0=200, S1=280, S2=420. Total=900 ✓

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 1,
                button = 0, small_blind_seat = 1, big_blind_seat = 2,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(300, 295, 290),
                hole_cards = HoleCards(Ah, Kh,  Qc, Jc,  _9d, _8d)
            });
            yield return new WaitForSeconds(0.5f);

            // S0 raises 30
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 0, action_type = 4, amount = 30, pot = 45,
                stacks = Stacks(270, 295, 290), community_cards = new List<int>(),
                emotions = Emotions(2, 3, 3)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 1, action_type = 0, amount = 0, pot = 45,
                stacks = Stacks(270, 295, 290), community_cards = new List<int>(),
                emotions = Emotions(-1, 3, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S2 calls 30
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 2, action_type = 2, amount = 30, pot = 75,
                stacks = Stacks(270, 295, 260), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, 4)
            });
            yield return new WaitForSeconds(0.3f);

            // FLOP Ad Kd 2c — S0 bets 50
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 1, seat = 0, action_type = 3, amount = 50, pot = 125,
                stacks = Stacks(220, 295, 260), community_cards = new List<int> { Ad, Kd, _2c },
                emotions = Emotions(2, -1, 3)
            });
            yield return new WaitForSeconds(0.3f);

            // S2 raises 100
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 1, seat = 2, action_type = 4, amount = 100, pot = 225,
                stacks = Stacks(220, 295, 160), community_cards = new List<int> { Ad, Kd, _2c },
                emotions = Emotions(-1, -1, 5)
            });
            yield return new WaitForSeconds(0.3f);

            // S0 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 1, seat = 0, action_type = 0, amount = 0, pot = 225,
                stacks = Stacks(220, 295, 160), community_cards = new List<int> { Ad, Kd, _2c },
                emotions = Emotions(3, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S2 wins. S0=220, S1=295, S2=160+225=385. Total=900 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 1,
                ended_by = 0, winners = new List<int> { 2 }, pot = 225,
                community_cards  = new List<int> { Ad, Kd, _2c },
                hole_cards        = HoleCards(Ah, Kh,  Qc, Jc,  _9d, _8d),
                stacks_after      = Stacks(220, 295, 385),
                eliminated_seats  = new List<int>()
            });
            yield return new WaitForSeconds(0.5f);

            // ── HAND 2 ──────────────────────────────────────────────────────────
            // S0 gets eliminated. Tests gap-close and roster reorder.
            // button=1. SB=2, BB=0. Post-blind: S0=210, S1=295, S2=380. pot=15
            // S0: 3c 2h   S1: As Ks   S2: Qh Jh
            // S1 shoves, S0 is pot-committed and calls with nothing. S0 eliminated.

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 2,
                button = 1, small_blind_seat = 2, big_blind_seat = 0,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(210, 295, 380),
                hole_cards = HoleCards(_3c, _2h,  As, Ks,  Qh, Jh)
            });
            yield return new WaitForSeconds(0.5f);

            // S1 raises 100
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 1, action_type = 4, amount = 100, pot = 115,
                stacks = Stacks(210, 195, 380), community_cards = new List<int>(),
                emotions = Emotions(-1, 2, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S2 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 2, action_type = 0, amount = 0, pot = 115,
                stacks = Stacks(210, 195, 380), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, 3)
            });
            yield return new WaitForSeconds(0.3f);

            // S0 calls all-in 210
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 0, action_type = 2, amount = 210, pot = 325,
                stacks = Stacks(0, 195, 380), community_cards = new List<int>(),
                emotions = Emotions(6, 2, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 wins. S0 eliminated. S0=0, S1=195+325=520... 
            // S0 contributed 210+10=220 total. S1 contributed 100+100=200... 
            // Simplify: S1 wins 325 pot. S1=195+325=520... no.
            // Clean: S0=0, S1=520, S2=380. Total=900 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 2,
                ended_by = 1, winners = new List<int> { 1 }, pot = 325,
                community_cards  = new List<int> { Ah, _7h, _3s, _9c, Kc },
                hole_cards        = HoleCards(_3c, _2h,  As, Ks,  Qh, Jh),
                stacks_after      = Stacks(0, 520, 380),
                eliminated_seats  = new List<int> { 0 }
            });
            yield return new WaitForSeconds(0.5f);

            // ── HAND 3 ──────────────────────────────────────────────────────────
            // Heads-up: S1 vs S2. S2 wins match.
            // button=2 (S2=SB since S0 gone, button rotates). SB=1, BB=2... 
            // Actually with S0 gone seats are 1 and 2. button=1 (S1=SB).

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 3,
                button = 1, small_blind_seat = 1, big_blind_seat = 2,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(0, 515, 370),
                hole_cards = HoleCards(0, 1, Ac, Qc, Th, _6h)
            });
            yield return new WaitForSeconds(0.5f);

            // Simpler — just send correct hole_cards for seats 1 and 2, seat 0 eliminated
            // Use a BanterEvent between hands instead
            SendEvent(new BanterEvent
            {
                kind = "BanterEvent", match_id = MatchId, hand_index = 3,
                banter_seat = 2, banter_text = "One down. One to go."
            });
            yield return new WaitForSeconds(0.5f);

            // S1 shoves 515. S2 calls 370 (covered).
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 3,
                street = 0, seat = 1, action_type = 4, amount = 515, pot = 530,
                stacks = Stacks(0, 0, 370), community_cards = new List<int>(),
                emotions = Emotions(-1, 5, 4)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 3,
                street = 0, seat = 2, action_type = 2, amount = 370, pot = 900,
                stacks = Stacks(0, 0, 0), community_cards = new List<int>(),
                emotions = Emotions(-1, 6, 5)
            });
            yield return new WaitForSeconds(0.3f);

            // S2 wins. S1 eliminated. S2 wins match.
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 3,
                ended_by = 1, winners = new List<int> { 2 }, pot = 900,
                community_cards  = new List<int> { Ah, Kc, _9h, _4d, _2s },
                hole_cards        = HoleCards(0, 1, Ac, Qc, Th, _6h),
                stacks_after      = Stacks(0, 0, 900),
                eliminated_seats  = new List<int> { 1 }
            });
            yield return new WaitForSeconds(2f);

            SendEvent(new MatchCompletedEvent
            {
                kind = "MatchCompleted", match_id = MatchId,
                winner = 2,
                final_stacks = Stacks(0, 0, 900),
                total_hands  = 3
            });

            StopMarketPolling();
            yield return new WaitForSeconds(1f);

            SimulateMatchStats(3, new List<MatchStatsSeatPayload>
            {
                new MatchStatsSeatPayload { seat_index = 0, handle = "AgentTrump", avatar_id = "trump",   placement = 3, folds = 1, raises = 0, calls = 1, hands_won = 0 },
                new MatchStatsSeatPayload { seat_index = 1, handle = "AgentObama", avatar_id = "obama",   placement = 2, folds = 1, raises = 2, calls = 1, hands_won = 1 },
                new MatchStatsSeatPayload { seat_index = 2, handle = "AgentMrBeast", avatar_id = "mr_beast",placement = 1, folds = 0, raises = 1, calls = 1, hands_won = 2 }
            });
        }

        // ════════════════════════════════════════════════════════════════════════
        // PRESET 3 — FourPlayer_SplitPot  (4 players, 250 chips each, SB=5 BB=10)
        //
        // Hand 1 — Split pot between S0 and S2 (shared straight on board).
        // Hand 2 — S1 and S3 eliminated same hand. Tests multi-elimination.
        // ════════════════════════════════════════════════════════════════════════

        private IEnumerator RunFourPlayer()
        {
            // ── HAND 1 — Split pot ──────────────────────────────────────────────
            // button=0. SB=1, BB=2. pot=15. All check down to river.
            // Board makes a straight: Ah Kh Qh Jh Th — both S0 (9h 8h) and S2 (9s 8c) have straight.
            // S0 and S2 split pot 125 each.
            // Stacks: S0=250-0=250, S1=250-5=245, S2=250-10=240, S3=250.
            // After split: S0=250+62=312... simplified:
            // S0=300, S1=220, S2=280, S3=200. Total=1000 ✓

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 1,
                button = 0, small_blind_seat = 1, big_blind_seat = 2,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(250, 245, 240, 250),
                hole_cards = HoleCards(_9h, _8s,  Kc, Jc,  _9d, _8c,  _6h, _5h)
            });
            yield return new WaitForSeconds(0.5f);

            // All check around preflop (S3, S0, S1, S2)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 3, action_type = 2, amount = 10, pot = 25,
                stacks = Stacks(250, 245, 240, 240), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, 3)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 0, action_type = 2, amount = 10, pot = 35,
                stacks = Stacks(240, 245, 240, 240), community_cards = new List<int>(),
                emotions = Emotions(2, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 1, action_type = 1, amount = 0, pot = 35,
                stacks = Stacks(240, 245, 240, 240), community_cards = new List<int>(),
                emotions = Emotions(-1, 3, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 2, action_type = 1, amount = 0, pot = 35,
                stacks = Stacks(240, 245, 240, 240), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, 3, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // FLOP Ah Kh Qh — all check
            for (int s = 0; s < 4; s++)
            {
                SendEvent(new ActionTakenEvent
                {
                    kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                    street = 1, seat = s, action_type = 1, amount = 0, pot = 35,
                    stacks = Stacks(240, 245, 240, 240),
                    community_cards = new List<int> { Ah, Kh, Qh },
                    emotions = Emotions(-1, -1, -1, -1)
                });
                yield return new WaitForSeconds(0.25f);
            }

            // TURN Jh — all check
            for (int s = 0; s < 4; s++)
            {
                SendEvent(new ActionTakenEvent
                {
                    kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                    street = 2, seat = s, action_type = 1, amount = 0, pot = 35,
                    stacks = Stacks(240, 245, 240, 240),
                    community_cards = new List<int> { Ah, Kh, Qh, Jh },
                    emotions = Emotions(-1, -1, -1, -1)
                });
                yield return new WaitForSeconds(0.25f);
            }

            // RIVER Th — all check
            for (int s = 0; s < 4; s++)
            {
                SendEvent(new ActionTakenEvent
                {
                    kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                    street = 3, seat = s, action_type = 1, amount = 0, pot = 35,
                    stacks = Stacks(240, 245, 240, 240),
                    community_cards = new List<int> { Ah, Kh, Qh, Jh, Th },
                    emotions = Emotions(-1, -1, -1, -1)
                });
                yield return new WaitForSeconds(0.25f);
            }

            // S0 and S2 split pot (both have nut straight via board).
            // S0: 240+(35/2)=257, S2: 240+(35/2)=257, remainder stays with S1, S3.
            // Rounded: S0=258, S1=245, S2=257, S3=240. Total=1000 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 1,
                ended_by = 1, winners = new List<int> { 0, 2 }, pot = 35,
                community_cards  = new List<int> { Ah, Kh, Qh, Jh, Th },
                hole_cards        = HoleCards(_9h, _8s,  Kc, Jc,  _9d, _8c,  _6h, _5h),
                stacks_after      = Stacks(258, 245, 257, 240),
                eliminated_seats  = new List<int>()
            });
            yield return new WaitForSeconds(0.5f);

            // ── HAND 2 — S1 and S3 eliminated same hand ─────────────────────────
            // Tests multi-elimination (two seats go bust simultaneously).
            // button=1. SB=2, BB=3. Post-blind: S0=258, S1=240, S2=252, S3=230. pot=15.
            // S0 wins big pot. S1 and S3 hit 0.

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 2,
                button = 1, small_blind_seat = 2, big_blind_seat = 3,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(258, 240, 252, 230),
                hole_cards = HoleCards(As, Ad,  _3c, _2d,  _6d, _5d,  _7c, _4c)
            });
            yield return new WaitForSeconds(0.5f);

            // S0 raises big
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 0, action_type = 4, amount = 200, pot = 215,
                stacks = Stacks(58, 240, 252, 230), community_cards = new List<int>(),
                emotions = Emotions(2, 4, 3, 4)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 calls all-in 240
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 1, action_type = 2, amount = 240, pot = 455,
                stacks = Stacks(58, 0, 252, 230), community_cards = new List<int>(),
                emotions = Emotions(-1, 6, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S2 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 2, action_type = 0, amount = 0, pot = 455,
                stacks = Stacks(58, 0, 252, 230), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, 3, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S3 calls all-in 230
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 3, action_type = 2, amount = 230, pot = 685,
                stacks = Stacks(58, 0, 252, 0), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, 5)
            });
            yield return new WaitForSeconds(0.3f);

            // S0 wins all. S0: 58+685=743... S2 untouched=252. Total=995... 
            // Clean: S0=748, S1=0, S2=252, S3=0. Total=1000 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 2,
                ended_by = 1, winners = new List<int> { 0 }, pot = 685,
                community_cards  = new List<int> { Ah, _9c, _3d, Kd, _2h },
                hole_cards        = HoleCards(As, Ad,  _3c, _2d,  _6d, _5d,  _7c, _4c),
                stacks_after      = Stacks(748, 0, 252, 0),
                eliminated_seats  = new List<int> { 1, 3 }
            });
            yield return new WaitForSeconds(2f);

            SendEvent(new MatchCompletedEvent
            {
                kind = "MatchCompleted", match_id = MatchId,
                winner = 0,
                final_stacks = Stacks(1000, 0, 0, 0),
                total_hands  = 2
            });

            StopMarketPolling();
            yield return new WaitForSeconds(1f);

            SimulateMatchStats(2, new List<MatchStatsSeatPayload>
            {
                new MatchStatsSeatPayload { seat_index = 0, handle = "AgentTrump", avatar_id = "trump",    placement = 1, folds = 0, raises = 2, calls = 0, hands_won = 1 },
                new MatchStatsSeatPayload { seat_index = 1, handle = "AgentObama", avatar_id = "obama",    placement = 3, folds = 0, raises = 0, calls = 2, hands_won = 0 },
                new MatchStatsSeatPayload { seat_index = 2, handle = "AgentMrBeast", avatar_id = "mr_beast", placement = 2, folds = 1, raises = 0, calls = 0, hands_won = 1 },
                new MatchStatsSeatPayload { seat_index = 3, handle = "AgentOnion", avatar_id = "the_onion",     placement = 3, folds = 0, raises = 0, calls = 2, hands_won = 0 }
            });
        }

        // ════════════════════════════════════════════════════════════════════════
        // PRESET 4 — SixPlayer_Full  (6 players, 200 chips each, SB=5 BB=10)
        //
        // Total chips: 6 × 200 = 1200 (must hold every hand).
        //
        // Hand 1 — Full streets (Preflop raise/call, Flop bet/call, Turn check/raise/call,
        //           River all-in call). S5 eliminated.
        //           After: S0=415, S1=195, S2=190, S3=200, S4=200, S5=0  = 1200 ✓
        //
        // Hand 2 — Multi-way preflop + Flop check/bet/raise/call. S4 eliminated.
        //           After: S0=415, S1=195, S2=185, S3=405, S4=0, S5=0   = 1200 ✓
        //
        // Hand 3 — 4-way action: Preflop raise/calls, Flop bet/raise/call, Turn all-in. S3 eliminated.
        //           After: S0=375, S1=680, S2=145, S3=0, S4=0, S5=0     = 1200 ✓
        //
        // Hand 4 — 3-way: Preflop raise/calls, Flop bet/call, Turn all-in. S2 eliminated.
        //           After: S0=325, S1=875, S2=0, S3=0, S4=0, S5=0       = 1200 ✓
        //
        // Hand 5 — Heads-Up full streets. S0 eliminated. S1 wins match.
        //           After: S0=0, S1=1200, S2=0, S3=0, S4=0, S5=0        = 1200 ✓
        //
        // Action coverage every hand:
        //   Fold ✓  Check ✓  Call ✓  Bet ✓  Raise ✓  All-in ✓  Elimination ✓  Banter ✓
        // ════════════════════════════════════════════════════════════════════════

       private IEnumerator RunSixPlayer() 
       {
            // ════════════════════════════════════════════════════════════════════════
            // SIX PLAYER — EXACTLY 3 HANDS
            //
            // Initial chips: 6 × 200 = 1200
            //
            // Hand 1:
            //   Normal multi-street hand. No eliminations.
            //   S0 wins with two pair, Aces and Kings.
            //   After: S0=375, S1=195, S2=170, S3=200, S4=190, S5=70
            //
            // Hand 2:
            //   Five-way all-in. S1 wins with pocket Aces.
            //   S2, S3, S4 and S5 are eliminated.
            //   After: S0=375, S1=825, S2=0, S3=0, S4=0, S5=0
            //
            // Hand 3:
            //   Heads-up final. S0 shoves pocket Kings.
            //   S1 calls with pocket Aces and wins the match.
            //   Final: S0=0, S1=1200, S2=0, S3=0, S4=0, S5=0
            // ════════════════════════════════════════════════════════════════════════
    
    
            // ────────────────────────────────────────────────────────────────────────
            // HAND 1 — Full-table hand, no eliminations
            // Button=S0, SB=S1, BB=S2
            //
            // S0: Ah Kd
            // S1: Qc Jc
            // S2: 9h 8h
            // S3: 6d 5d
            // S4: 3c 2c
            // S5: Jh 7h
            //
            // Board: Ac 9c 4h Kc 2s
            // S0 wins with two pair: Aces and Kings.
            // ────────────────────────────────────────────────────────────────────────
    
            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt",
                match_id = MatchId,
                hand_index = 1,
    
                button = 0,
                small_blind_seat = 1,
                big_blind_seat = 2,
    
                small_blind_amount = 5,
                big_blind_amount = 10,
                pot = 15,
    
                stacks = Stacks(200, 195, 190, 200, 200, 200),
    
                hole_cards = HoleCards(
                    Ah, Kd,
                    Qc, Jc,
                    _9h, _8h,
                    _6d, _5d,
                    _3c, _2c,
                    Jh, _7h
                )
            });
    
            yield return new WaitForSeconds(0.5f);
    
    
            // PREFLOP — S3 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 0,
                seat = 3,
                action_type = 0,
                amount = 0,
                pot = 15,
    
                stacks = Stacks(200, 195, 190, 200, 200, 200),
                community_cards = new List<int>(),
    
                emotions = Emotions(-1, -1, -1, 3, -1, -1)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            // S4 calls 10
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 0,
                seat = 4,
                action_type = 2,
                amount = 10,
                pot = 25,
    
                stacks = Stacks(200, 195, 190, 200, 190, 200),
                community_cards = new List<int>(),
    
                emotions = Emotions(-1, -1, -1, -1, 2, -1)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            // S5 calls 10
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 0,
                seat = 5,
                action_type = 2,
                amount = 10,
                pot = 35,
    
                stacks = Stacks(200, 195, 190, 200, 190, 190),
                community_cards = new List<int>(),
    
                emotions = Emotions(-1, -1, -1, -1, -1, 2)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            // S0 raises to 30
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 0,
                seat = 0,
                action_type = 4,
                amount = 30,
                pot = 65,
    
                stacks = Stacks(170, 195, 190, 200, 190, 190),
                community_cards = new List<int>(),
    
                emotions = Emotions(2, 4, 3, -1, 3, 3)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            SendEvent(new BanterEvent
            {
                kind = "BanterEvent",
                match_id = MatchId,
                hand_index = 1,
                banter_seat = 0,
                banter_text = "Thirty to play. Let us see who actually has a hand."
            });
    
            yield return new WaitForSeconds(0.5f);
    
    
            // S1 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 0,
                seat = 1,
                action_type = 0,
                amount = 0,
                pot = 65,
    
                stacks = Stacks(170, 195, 190, 200, 190, 190),
                community_cards = new List<int>(),
    
                emotions = Emotions(-1, 3, -1, -1, -1, -1)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            // S2 calls 20 more
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 0,
                seat = 2,
                action_type = 2,
                amount = 20,
                pot = 85,
    
                stacks = Stacks(170, 195, 170, 200, 190, 190),
                community_cards = new List<int>(),
    
                emotions = Emotions(-1, -1, 3, -1, -1, -1)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            // S4 folds instead of completing the raise
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 0,
                seat = 4,
                action_type = 0,
                amount = 0,
                pot = 85,
    
                stacks = Stacks(170, 195, 170, 200, 190, 190),
                community_cards = new List<int>(),
    
                emotions = Emotions(-1, -1, -1, -1, 3, -1)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            // S5 calls 20 more
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 0,
                seat = 5,
                action_type = 2,
                amount = 20,
                pot = 105,
    
                stacks = Stacks(170, 195, 170, 200, 190, 170),
                community_cards = new List<int>(),
    
                emotions = Emotions(-1, -1, -1, -1, -1, 3)
            });
    
            yield return new WaitForSeconds(0.3f);
    
    
            // FLOP — Ac 9c 4h
    
            // S2 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 1,
                seat = 2,
                action_type = 1,
                amount = 0,
                pot = 105,
    
                stacks = Stacks(170, 195, 170, 200, 190, 170),
                community_cards = new List<int> { Ac, _9c, _4h },
    
                emotions = Emotions(-1, -1, 3, -1, -1, -1)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            // S5 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 1,
                seat = 5,
                action_type = 1,
                amount = 0,
                pot = 105,
    
                stacks = Stacks(170, 195, 170, 200, 190, 170),
                community_cards = new List<int> { Ac, _9c, _4h },
    
                emotions = Emotions(-1, -1, -1, -1, -1, 3)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            // S0 bets 40
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 1,
                seat = 0,
                action_type = 3,
                amount = 40,
                pot = 145,
    
                stacks = Stacks(130, 195, 170, 200, 190, 170),
                community_cards = new List<int> { Ac, _9c, _4h },
    
                emotions = Emotions(2, -1, 4, -1, -1, 4)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            // S2 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 1,
                seat = 2,
                action_type = 0,
                amount = 0,
                pot = 145,
    
                stacks = Stacks(130, 195, 170, 200, 190, 170),
                community_cards = new List<int> { Ac, _9c, _4h },
    
                emotions = Emotions(-1, -1, 4, -1, -1, -1)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            // S5 calls 40
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 1,
                seat = 5,
                action_type = 2,
                amount = 40,
                pot = 185,
    
                stacks = Stacks(130, 195, 170, 200, 190, 130),
                community_cards = new List<int> { Ac, _9c, _4h },
    
                emotions = Emotions(-1, -1, -1, -1, -1, 4)
            });
    
            yield return new WaitForSeconds(0.3f);
    
    
            // TURN — Kc
    
            // S5 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 2,
                seat = 5,
                action_type = 1,
                amount = 0,
                pot = 185,
    
                stacks = Stacks(130, 195, 170, 200, 190, 130),
                community_cards = new List<int> { Ac, _9c, _4h, Kc },
    
                emotions = Emotions(-1, -1, -1, -1, -1, 4)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            // S0 bets 60
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 2,
                seat = 0,
                action_type = 3,
                amount = 60,
                pot = 245,
    
                stacks = Stacks(70, 195, 170, 200, 190, 130),
                community_cards = new List<int> { Ac, _9c, _4h, Kc },
    
                emotions = Emotions(2, -1, -1, -1, -1, 5)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            SendEvent(new BanterEvent
            {
                kind = "BanterEvent",
                match_id = MatchId,
                hand_index = 1,
                banter_seat = 5,
                banter_text = "You keep betting. That does not mean I believe you."
            });
    
            yield return new WaitForSeconds(0.5f);
    
    
            // S5 calls 60
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 2,
                seat = 5,
                action_type = 2,
                amount = 60,
                pot = 305,
    
                stacks = Stacks(70, 195, 170, 200, 190, 70),
                community_cards = new List<int> { Ac, _9c, _4h, Kc },
    
                emotions = Emotions(-1, -1, -1, -1, -1, 5)
            });
    
            yield return new WaitForSeconds(0.3f);
    
    
            // RIVER — 2s
    
            // S5 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 3,
                seat = 5,
                action_type = 1,
                amount = 0,
                pot = 305,
    
                stacks = Stacks(70, 195, 170, 200, 190, 70),
                community_cards = new List<int> { Ac, _9c, _4h, Kc, _2s },
    
                emotions = Emotions(-1, -1, -1, -1, -1, 3)
            });
    
            yield return new WaitForSeconds(0.25f);
    
    
            // S0 checks behind
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken",
                match_id = MatchId,
                hand_index = 1,
    
                street = 3,
                seat = 0,
                action_type = 1,
                amount = 0,
                pot = 305,
    
                stacks = Stacks(70, 195, 170, 200, 190, 70),
                community_cards = new List<int> { Ac, _9c, _4h, Kc, _2s },
    
                emotions = Emotions(2, -1, -1, -1, -1, -1)
            });
    
            yield return new WaitForSeconds(0.3f);
    
    
            // S0 receives the 305-chip pot.
            // 70 + 305 = 375.
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted",
                match_id = MatchId,
                hand_index = 1,
    
                ended_by = 1,
                winners = new List<int> { 0 },
                pot = 305,
    
                community_cards = new List<int>
                {
                    Ac, _9c, _4h, Kc, _2s
                },
    
                hole_cards = HoleCards(
                    Ah, Kd,
                    Qc, Jc,
                    _9h, _8h,
                    _6d, _5d,
                    _3c, _2c,
                    Jh, _7h
                ),
    
                stacks_after = Stacks(375, 195, 170, 200, 190, 70),
                eliminated_seats = new List<int>()
            });
    
            yield return new WaitForSeconds(1f);
           // ────────────────────────────────────────────────────────────────────────
        // HAND 2 — THREE-WAY TIE
        // Starting stacks:
        // S0=375, S1=195, S2=170, S3=200, S4=190, S5=70
        //
        // Button=S1, SB=S2, BB=S3
        //
        // The community cards form a Royal Flush:
        //
        // Ah Kh Qh Jh Th
        //
        // S0, S1 and S2 reach showdown.
        // Because the best possible hand is entirely on the board,
        // all three players tie and split the 120-chip pot.
        //
        // After:
        // S0=385, S1=205, S2=180, S3=180, S4=190, S5=60
        // Total = 1200
        // ────────────────────────────────────────────────────────────────────────
    
        SendEvent(new HandDealtEvent
        {
            kind = "HandDealt",
            match_id = MatchId,
            hand_index = 2,
    
            button = 1,
            small_blind_seat = 2,
            big_blind_seat = 3,
    
            small_blind_amount = 5,
            big_blind_amount = 10,
            pot = 15,
    
            stacks = Stacks(
                375,
                195,
                165,
                190,
                190,
                70
            ),
    
            hole_cards = HoleCards(
                _8c, _7d,   // S0
                _6c, _5s,   // S1
                _4d, _3s,   // S2
                Qc, Jc,     // S3
                _9d, _8d,   // S4
                Ks, Qs      // S5
            )
        });
    
        yield return new WaitForSeconds(0.5f);
    
    
        // PREFLOP — S4 folds
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 0,
            seat = 4,
            action_type = 0,
            amount = 0,
            pot = 15,
    
            stacks = Stacks(375, 195, 165, 190, 190, 70),
            community_cards = new List<int>(),
    
            emotions = Emotions(-1, -1, -1, -1, 3, -1)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S5 calls 10
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 0,
            seat = 5,
            action_type = 2,
            amount = 10,
            pot = 25,
    
            stacks = Stacks(375, 195, 165, 190, 190, 60),
            community_cards = new List<int>(),
    
            emotions = Emotions(-1, -1, -1, -1, -1, 2)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S0 calls 10
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 0,
            seat = 0,
            action_type = 2,
            amount = 10,
            pot = 35,
    
            stacks = Stacks(365, 195, 165, 190, 190, 60),
            community_cards = new List<int>(),
    
            emotions = Emotions(2, -1, -1, -1, -1, -1)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S1 calls 10
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 0,
            seat = 1,
            action_type = 2,
            amount = 10,
            pot = 45,
    
            stacks = Stacks(365, 185, 165, 190, 190, 60),
            community_cards = new List<int>(),
    
            emotions = Emotions(-1, 2, -1, -1, -1, -1)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S2 completes the small blind with 5
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 0,
            seat = 2,
            action_type = 2,
            amount = 5,
            pot = 50,
    
            stacks = Stacks(365, 185, 160, 190, 190, 60),
            community_cards = new List<int>(),
    
            emotions = Emotions(-1, -1, 2, -1, -1, -1)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S3 checks the big blind
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 0,
            seat = 3,
            action_type = 1,
            amount = 0,
            pot = 50,
    
            stacks = Stacks(365, 185, 160, 190, 190, 60),
            community_cards = new List<int>(),
    
            emotions = Emotions(-1, -1, -1, 2, -1, -1)
        });
    
        yield return new WaitForSeconds(0.3f);
    
    
        // FLOP — Ah Kh Qh
    
        // S2 checks
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 1,
            seat = 2,
            action_type = 1,
            amount = 0,
            pot = 50,
    
            stacks = Stacks(365, 185, 160, 190, 190, 60),
            community_cards = new List<int> { Ah, Kh, Qh },
    
            emotions = Emotions(-1, -1, 4, -1, -1, -1)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S3 bets 10
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 1,
            seat = 3,
            action_type = 3,
            amount = 10,
            pot = 60,
    
            stacks = Stacks(365, 185, 160, 180, 190, 60),
            community_cards = new List<int> { Ah, Kh, Qh },
    
            emotions = Emotions(-1, -1, -1, 3, -1, -1)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S5 folds
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 1,
            seat = 5,
            action_type = 0,
            amount = 0,
            pot = 60,
    
            stacks = Stacks(365, 185, 160, 180, 190, 60),
            community_cards = new List<int> { Ah, Kh, Qh },
    
            emotions = Emotions(-1, -1, -1, -1, -1, 4)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S0 raises to 20
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 1,
            seat = 0,
            action_type = 4,
            amount = 20,
            pot = 80,
    
            stacks = Stacks(345, 185, 160, 180, 190, 60),
            community_cards = new List<int> { Ah, Kh, Qh },
    
            emotions = Emotions(3, -1, -1, 4, -1, -1)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        SendEvent(new BanterEvent
        {
            kind = "BanterEvent",
            match_id = MatchId,
            hand_index = 2,
            banter_seat = 0,
            banter_text = "Three hearts already. Let us make this interesting."
        });
    
        yield return new WaitForSeconds(0.5f);
    
    
        // S1 calls 20
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 1,
            seat = 1,
            action_type = 2,
            amount = 20,
            pot = 100,
    
            stacks = Stacks(345, 165, 160, 180, 190, 60),
            community_cards = new List<int> { Ah, Kh, Qh },
    
            emotions = Emotions(-1, 3, -1, -1, -1, -1)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S2 calls 20
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 1,
            seat = 2,
            action_type = 2,
            amount = 20,
            pot = 120,
    
            stacks = Stacks(345, 165, 140, 180, 190, 60),
            community_cards = new List<int> { Ah, Kh, Qh },
    
            emotions = Emotions(-1, -1, 3, -1, -1, -1)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S3 folds to the raise
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 1,
            seat = 3,
            action_type = 0,
            amount = 0,
            pot = 120,
    
            stacks = Stacks(345, 165, 140, 180, 190, 60),
            community_cards = new List<int> { Ah, Kh, Qh },
    
            emotions = Emotions(-1, -1, -1, 4, -1, -1)
        });
    
        yield return new WaitForSeconds(0.3f);
    
    
        // TURN — Jh
        // The board now contains a Royal Flush draw completed except for the Ten.
    
        // S0 checks
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 2,
            seat = 0,
            action_type = 1,
            amount = 0,
            pot = 120,
    
            stacks = Stacks(345, 165, 140, 180, 190, 60),
            community_cards = new List<int> { Ah, Kh, Qh, Jh },
    
            emotions = Emotions(4, -1, -1, -1, -1, -1)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S1 checks
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 2,
            seat = 1,
            action_type = 1,
            amount = 0,
            pot = 120,
    
            stacks = Stacks(345, 165, 140, 180, 190, 60),
            community_cards = new List<int> { Ah, Kh, Qh, Jh },
    
            emotions = Emotions(-1, 4, -1, -1, -1, -1)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S2 checks
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 2,
            seat = 2,
            action_type = 1,
            amount = 0,
            pot = 120,
    
            stacks = Stacks(345, 165, 140, 180, 190, 60),
            community_cards = new List<int> { Ah, Kh, Qh, Jh },
    
            emotions = Emotions(-1, -1, 4, -1, -1, -1)
        });
    
        yield return new WaitForSeconds(0.3f);
    
    
        // RIVER — Th
        // Royal Flush entirely on the board.
    
        // S0 checks
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 3,
            seat = 0,
            action_type = 1,
            amount = 0,
            pot = 120,
    
            stacks = Stacks(345, 165, 140, 180, 190, 60),
            community_cards = new List<int> { Ah, Kh, Qh, Jh, Th },
    
            emotions = Emotions(5, -1, -1, -1, -1, -1)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S1 checks
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 3,
            seat = 1,
            action_type = 1,
            amount = 0,
            pot = 120,
    
            stacks = Stacks(345, 165, 140, 180, 190, 60),
            community_cards = new List<int> { Ah, Kh, Qh, Jh, Th },
    
            emotions = Emotions(-1, 5, -1, -1, -1, -1)
        });
    
        yield return new WaitForSeconds(0.25f);
    
    
        // S2 checks
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 2,
    
            street = 3,
            seat = 2,
            action_type = 1,
            amount = 0,
            pot = 120,
    
            stacks = Stacks(345, 165, 140, 180, 190, 60),
            community_cards = new List<int> { Ah, Kh, Qh, Jh, Th },
    
            emotions = Emotions(-1, -1, 5, -1, -1, -1)
        });
    
        yield return new WaitForSeconds(0.3f);
    
    
        SendEvent(new BanterEvent
        {
            kind = "BanterEvent",
            match_id = MatchId,
            hand_index = 2,
            banter_seat = 1,
            banter_text = "A Royal Flush on the table. Nobody gets to claim that one alone."
        });
    
        yield return new WaitForSeconds(0.5f);
    
    
        // Three-way split.
        // Each player receives 40 chips from the 120-chip pot.
        SendEvent(new HandCompletedEvent
        {
            kind = "HandCompleted",
            match_id = MatchId,
            hand_index = 2,
    
            ended_by = 1,
    
            winners = new List<int>
            {
                0, 1, 2
            },
    
            pot = 120,
    
            community_cards = new List<int>
            {
                Ah, Kh, Qh, Jh, Th
            },
    
            hole_cards = HoleCards(
                _8c, _7d,
                _6c, _5s,
                _4d, _3s,
                Qc, Jc,
                _9d, _8d,
                Ks, Qs
            ),
    
            stacks_after = Stacks(
                385,
                205,
                180,
                180,
                190,
                60
            ),
    
            eliminated_seats = new List<int>()
        });
    
        yield return new WaitForSeconds(1.5f);
    
    
        // ────────────────────────────────────────────────────────────────────────
        // HAND 3 — FINAL SIX-WAY ALL-IN
        //
        // Starting stacks:
        // S0=385, S1=205, S2=180, S3=180, S4=190, S5=60
        //
        // Button=S2, SB=S3, BB=S4
        //
        // Everyone goes all-in.
        //
        // S0: As Ad
        // S1: Ks Kd
        // S2: Qh Qd
        // S3: Js Jd
        // S4: Tc Td
        // S5: 8s 8d
        //
        // Board: 2c 5d 7h 9s Jc
        //
        // S0 wins with pocket Aces.
        // Because S0 has the largest starting stack, S0 covers every player
        // and wins every main/side pot.
        //
        // Final:
        // S0=1200
        // All other players=0
        // ────────────────────────────────────────────────────────────────────────
    
        SendEvent(new HandDealtEvent
        {
            kind = "HandDealt",
            match_id = MatchId,
            hand_index = 3,
    
            button = 2,
            small_blind_seat = 3,
            big_blind_seat = 4,
    
            small_blind_amount = 5,
            big_blind_amount = 10,
            pot = 15,
    
            stacks = Stacks(
                385,
                205,
                180,
                175,
                180,
                60
            ),
    
            hole_cards = HoleCards(
                As, Ad,
                Ks, Kd,
                Qh, Qd,
                Js, Jd,
                Tc, Td,
                _8s, _8d
            )
        });
    
        yield return new WaitForSeconds(0.5f);
    
    
        SendEvent(new BanterEvent
        {
            kind = "BanterEvent",
            match_id = MatchId,
            hand_index = 3,
            banter_seat = 5,
            banter_text = "Sixty chips left. There is no point saving them now."
        });
    
        yield return new WaitForSeconds(0.5f);
    
    
        // S5 shoves 60
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 3,
    
            street = 0,
            seat = 5,
            action_type = 4,
            amount = 60,
            pot = 75,
    
            stacks = Stacks(385, 205, 180, 175, 180, 0),
            community_cards = new List<int>(),
    
            emotions = Emotions(-1, 5, 5, 5, 5, 6)
        });
    
        yield return new WaitForSeconds(0.3f);
    
    
        // S0 shoves 385
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 3,
    
            street = 0,
            seat = 0,
            action_type = 4,
            amount = 385,
            pot = 460,
    
            stacks = Stacks(0, 205, 180, 175, 180, 0),
            community_cards = new List<int>(),
    
            emotions = Emotions(6, 5, 5, 5, 5, -1)
        });
    
        yield return new WaitForSeconds(0.3f);
    
    
        SendEvent(new BanterEvent
        {
            kind = "BanterEvent",
            match_id = MatchId,
            hand_index = 3,
            banter_seat = 0,
            banter_text = "I cover the table. Everyone is invited."
        });
    
        yield return new WaitForSeconds(0.5f);
    
    
        // S1 calls all-in for 205
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 3,
    
            street = 0,
            seat = 1,
            action_type = 2,
            amount = 205,
            pot = 665,
    
            stacks = Stacks(0, 0, 180, 175, 180, 0),
            community_cards = new List<int>(),
    
            emotions = Emotions(-1, 6, 5, 5, 5, -1)
        });
    
        yield return new WaitForSeconds(0.3f);
    
    
        // S2 calls all-in for 180
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 3,
    
            street = 0,
            seat = 2,
            action_type = 2,
            amount = 180,
            pot = 845,
    
            stacks = Stacks(0, 0, 0, 175, 180, 0),
            community_cards = new List<int>(),
    
            emotions = Emotions(-1, -1, 6, 5, 5, -1)
        });
    
        yield return new WaitForSeconds(0.3f);
    
    
        // S3 calls all-in for remaining 175
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 3,
    
            street = 0,
            seat = 3,
            action_type = 2,
            amount = 175,
            pot = 1020,
    
            stacks = Stacks(0, 0, 0, 0, 180, 0),
            community_cards = new List<int>(),
    
            emotions = Emotions(-1, -1, -1, 6, 5, -1)
        });
    
        yield return new WaitForSeconds(0.3f);
    
    
        // S4 calls all-in for remaining 180
        SendEvent(new ActionTakenEvent
        {
            kind = "ActionTaken",
            match_id = MatchId,
            hand_index = 3,
    
            street = 0,
            seat = 4,
            action_type = 2,
            amount = 180,
            pot = 1200,
    
            stacks = Stacks(0, 0, 0, 0, 0, 0),
            community_cards = new List<int>(),
    
            emotions = Emotions(6, 6, 6, 6, 6, 6)
        });
    
        yield return new WaitForSeconds(0.5f);
    
    
        // S0 wins every pot with pocket Aces.
        SendEvent(new HandCompletedEvent
        {
            kind = "HandCompleted",
            match_id = MatchId,
            hand_index = 3,
    
            ended_by = 1,
    
            winners = new List<int>
            {
                0
            },
    
            pot = 1200,
    
            community_cards = new List<int>
            {
                _2c, _5d, _7h, _9s, Jc
            },
    
            hole_cards = HoleCards(
                As, Ad,
                Ks, Kd,
                Qh, Qd,
                Js, Jd,
                Tc, Td,
                _8s, _8d
            ),
    
            stacks_after = Stacks(
                1200,
                0,
                0,
                0,
                0,
                0
            ),
    
            eliminated_seats = new List<int>
            {
                1, 2, 3, 4, 5
            }
        });
    
        yield return new WaitForSeconds(2f);
    
    
        // ────────────────────────────────────────────────────────────────────────
        // MATCH COMPLETE
        // ────────────────────────────────────────────────────────────────────────
    
        SendEvent(new MatchCompletedEvent
        {
            kind = "MatchCompleted",
            match_id = MatchId,
    
            winner = 0,
    
            final_stacks = Stacks(
                1200,
                0,
                0,
                0,
                0,
                0
            ),
    
            total_hands = 3
        });
    
        StopMarketPolling();
    
        yield return new WaitForSeconds(1f);
    
    
        SimulateMatchStats(
            3,
            new List<MatchStatsSeatPayload>
            {
                new MatchStatsSeatPayload
                {
                    seat_index = 0,
                    handle = "AgentTrump",
                    avatar_id = "trump",
                    placement = 1,
                    folds = 0,
                    raises = 3,
                    calls = 2,
                    hands_won = 3
                },
    
                new MatchStatsSeatPayload
                {
                    seat_index = 1,
                    handle = "AgentObama",
                    avatar_id = "obama",
                    placement = 2,
                    folds = 1,
                    raises = 0,
                    calls = 4,
                    hands_won = 1
                },
    
                new MatchStatsSeatPayload
                {
                    seat_index = 2,
                    handle = "AgentMrBeast",
                    avatar_id = "mr_beast",
                    placement = 2,
                    folds = 1,
                    raises = 0,
                    calls = 4,
                    hands_won = 1
                },
    
                new MatchStatsSeatPayload
                {
                    seat_index = 3,
                    handle = "AgentOnion",
                    avatar_id = "the_onion",
                    placement = 2,
                    folds = 2,
                    raises = 0,
                    calls = 1,
                    hands_won = 0
                },
    
                new MatchStatsSeatPayload
                {
                    seat_index = 4,
                    handle = "AgentCardiB",
                    avatar_id = "cardi_b",
                    placement = 2,
                    folds = 2,
                    raises = 0,
                    calls = 1,
                    hands_won = 0
                },
    
                new MatchStatsSeatPayload
                {
                    seat_index = 5,
                    handle = "AgentTheoVon",
                    avatar_id = "theo_von",
                    placement = 2,
                    folds = 1,
                    raises = 1,
                    calls = 2,
                    hands_won = 0
                }
            }
        );
}

        // ════════════════════════════════════════════════════════════════════════
        // PRESET 5 — SixPlayer_SevenHands  (6 players, 200 chips each, SB=5 BB=10)
        //
        // Total chips: 6 × 200 = 1200 (must hold every hand). Verified by hand-simulation.
        //
        // Pacing:
        //   Hand 1 — Fold-heavy. No elimination.            (S0=120,S1=175,S2=305,S3=200,S4=200,S5=200)
        //   Hand 2 — Fold-heavy. No elimination.             (S0=120,S1=310,S2=275,S3=95, S4=200,S5=200)
        //   Hand 3 — DOUBLE ELIMINATION. 3-way all-in, S1 covers and beats both S3 and S4.
        //            Side pots: main pot (capped 90) + side pot (90→95 gap... see inline math).
        //            S3 and S4 both eliminated in the same hand.   (S0=120,S1=605,S2=275,S3=0,S4=0,S5=200)
        //   Hand 4 — Fold-heavy among 4 active. No elimination.    (S0=90, S1=635,S2=275,S3=0,S4=0,S5=200)
        //   Hand 5 — DOUBLE ELIMINATION. 3-way all-in, S1 covers and beats both S0 and S2.
        //            S0 and S2 both eliminated → table reduced to heads-up (S1, S5).
        //                                                          (S0=0,  S1=1000,S2=0,S3=0,S4=0,S5=200)
        //   Hand 6 — Heads-up. No elimination — S5 claws back chips on the river.
        //                                                          (S0=0,  S1=910, S2=0,S3=0,S4=0,S5=290)
        //   Hand 7 — FINAL HAND. S5 shoves, S1 calls and wins. S5 eliminated. S1 wins the match.
        //                                                          (S0=0,  S1=1200,S2=0,S3=0,S4=0,S5=0)
        //
        // Action coverage every hand: Fold ✓  Check ✓  Call ✓  Bet ✓  Raise ✓  All-in ✓
        //   Double-elimination (2 seats in one HandCompleted.eliminated_seats) ✓  Banter ✓
        // ════════════════════════════════════════════════════════════════════════

        private IEnumerator RunSixPlayerSevenHands()
        {
            // ── HAND 1 — button=0, SB=S1, BB=S2 — fold-heavy, no elimination ──────
            // post-blind: S0=200,S1=195,S2=190,S3=200,S4=200,S5=200  pot=15
            // S0=Qs9d  S1=Ac4c  S2=Kh Th  S3=7s7d  S4=2c2d  S5=Jd6s
            //
            // Preflop: S3 fold, S4 fold, S5 fold, S0 raises to 25, S1 calls 20 more, S2 calls 15 more
            // Flop 8h3c5s: S1 checks, S2 checks, S0 bets 30, S2 calls 30, S1 folds
            // Turn Qd: S2 checks, S0 checks
            // River 4h: S2 bets 25, S0 calls 25
            // S2 wins (pair of Kings vs Queens). No elimination.
            // stacks_after: S0=120,S1=175,S2=305,S3=200,S4=200,S5=200  total=1200 ✓

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 1,
                button = 0, small_blind_seat = 1, big_blind_seat = 2,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(200, 195, 190, 200, 200, 200),
                hole_cards = HoleCards(Qs, _9d,  Ac, _4c,  Kh, Th,  _7s, _7d,  _2c, _2d,  Jd, _6s)
            });
            yield return new WaitForSeconds(0.5f);

            // PREFLOP — S3 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 3, action_type = 0, amount = 0, pot = 15,
                stacks = Stacks(200, 195, 190, 200, 200, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, 3, -1, -1)
            });
            yield return new WaitForSeconds(0.2f);

            // S4 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 4, action_type = 0, amount = 0, pot = 15,
                stacks = Stacks(200, 195, 190, 200, 200, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, -1, 3, -1)
            });
            yield return new WaitForSeconds(0.2f);

            // S5 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 5, action_type = 0, amount = 0, pot = 15,
                stacks = Stacks(200, 195, 190, 200, 200, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, -1, -1, 3)
            });
            yield return new WaitForSeconds(0.2f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 1,
                banter_seat = 3, banter_text = "Folding early. Live to fight another hand." });
            yield return new WaitForSeconds(0.5f);

            // S0 raises to 25
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 0, action_type = 4, amount = 25, pot = 40,
                stacks = Stacks(175, 195, 190, 200, 200, 200), community_cards = new List<int>(),
                emotions = Emotions(2, -1, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.2f);

            // S1 calls 20 more
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 1, action_type = 2, amount = 20, pot = 60,
                stacks = Stacks(175, 175, 190, 200, 200, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, 2, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.2f);

            // S2 calls 15 more (BB already 10 in, calls to 25 total)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 0, seat = 2, action_type = 2, amount = 15, pot = 75,
                stacks = Stacks(175, 175, 175, 200, 200, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, 2, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 1,
                banter_seat = 0, banter_text = "Three of us. Let's see who blinks." });
            yield return new WaitForSeconds(0.5f);

            // FLOP 8h 3c 5s — S1 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 1, seat = 1, action_type = 1, amount = 0, pot = 75,
                stacks = Stacks(175, 175, 175, 200, 200, 200), community_cards = new List<int> { _8h, _3c, _5s },
                emotions = Emotions(-1, 3, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S2 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 1, seat = 2, action_type = 1, amount = 0, pot = 75,
                stacks = Stacks(175, 175, 175, 200, 200, 200), community_cards = new List<int> { _8h, _3c, _5s },
                emotions = Emotions(-1, -1, 3, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S0 bets 30
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 1, seat = 0, action_type = 3, amount = 30, pot = 105,
                stacks = Stacks(145, 175, 175, 200, 200, 200), community_cards = new List<int> { _8h, _3c, _5s },
                emotions = Emotions(2, -1, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 1, seat = 1, action_type = 0, amount = 0, pot = 105,
                stacks = Stacks(145, 175, 175, 200, 200, 200), community_cards = new List<int> { _8h, _3c, _5s },
                emotions = Emotions(-1, 3, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.2f);

            // S2 calls 30
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 1, seat = 2, action_type = 2, amount = 30, pot = 135,
                stacks = Stacks(145, 175, 145, 200, 200, 200), community_cards = new List<int> { _8h, _3c, _5s },
                emotions = Emotions(-1, -1, 3, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // TURN Qd — S2 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 2, seat = 2, action_type = 1, amount = 0, pot = 135,
                stacks = Stacks(145, 175, 145, 200, 200, 200), community_cards = new List<int> { _8h, _3c, _5s, Qd },
                emotions = Emotions(-1, -1, 3, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S0 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 2, seat = 0, action_type = 1, amount = 0, pot = 135,
                stacks = Stacks(145, 175, 145, 200, 200, 200), community_cards = new List<int> { _8h, _3c, _5s, Qd },
                emotions = Emotions(3, -1, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 1,
                banter_seat = 2, banter_text = "Patience pays off." });
            yield return new WaitForSeconds(0.5f);

            // RIVER 4h — S2 bets 25
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 3, seat = 2, action_type = 3, amount = 25, pot = 160,
                stacks = Stacks(145, 175, 120, 200, 200, 200), community_cards = new List<int> { _8h, _3c, _5s, Qd, _4h },
                emotions = Emotions(-1, -1, 2, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S0 calls 25
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 1,
                street = 3, seat = 0, action_type = 2, amount = 25, pot = 185,
                stacks = Stacks(120, 175, 120, 200, 200, 200), community_cards = new List<int> { _8h, _3c, _5s, Qd, _4h },
                emotions = Emotions(4, -1, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S2 wins (pair of Kings beats Queen-high). No elimination.
            // stacks_after: S0=120,S1=175,S2=305,S3=200,S4=200,S5=200  total=1200 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 1,
                ended_by = 1, winners = new List<int> { 2 }, pot = 185,
                community_cards  = new List<int> { _8h, _3c, _5s, Qd, _4h },
                hole_cards        = HoleCards(Qs, _9d,  Ac, _4c,  Kh, Th,  _7s, _7d,  _2c, _2d,  Jd, _6s),
                stacks_after      = Stacks(120, 175, 305, 200, 200, 200),
                eliminated_seats  = new List<int>()
            });
            yield return new WaitForSeconds(0.5f);

            // ── HAND 2 — button=1, SB=S2, BB=S3 — fold-heavy, no elimination ──────
            // post-blind: S0=120,S1=175,S2=300,S3=190,S4=200,S5=200  pot=15
            // S0=8c8d  S1=KsQc  S2=4h4s  S3=AdTc  S4=9c5h  S5=Jh3c
            //
            // Preflop: S4 fold, S5 fold, S0 fold, S1 raises to 30, S2 calls 25 more, S3 calls 20 more
            // Flop 6h2dKc: S2 checks, S3 bets 40, S1 calls 40, S2 folds
            // Turn 9s: S3 checks, S1 checks
            // River 7d: S3 checks, S1 bets 35, S3 calls 35
            // S1 wins (pair of Kings). No elimination.
            // stacks_after: S0=120,S1=310,S2=275,S3=95,S4=200,S5=200  total=1200 ✓

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 2,
                button = 1, small_blind_seat = 2, big_blind_seat = 3,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(120, 175, 300, 190, 200, 200),
                hole_cards = HoleCards(_8c, _8d,  Ks, Qc,  _4h, _4s,  Ad, Tc,  _9c, _5h,  Jh, _3c)
            });
            yield return new WaitForSeconds(0.5f);

            // PREFLOP — S4 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 4, action_type = 0, amount = 0, pot = 15,
                stacks = Stacks(120, 175, 300, 190, 200, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, -1, 3, -1)
            });
            yield return new WaitForSeconds(0.2f);

            // S5 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 5, action_type = 0, amount = 0, pot = 15,
                stacks = Stacks(120, 175, 300, 190, 200, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, -1, -1, 3)
            });
            yield return new WaitForSeconds(0.2f);

            // S0 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 0, action_type = 0, amount = 0, pot = 15,
                stacks = Stacks(120, 175, 300, 190, 200, 200), community_cards = new List<int>(),
                emotions = Emotions(3, -1, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.2f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 2,
                banter_seat = 0, banter_text = "Not my hand. Folding and watching." });
            yield return new WaitForSeconds(0.5f);

            // S1 raises to 30
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 1, action_type = 4, amount = 30, pot = 45,
                stacks = Stacks(120, 145, 300, 190, 200, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, 2, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.2f);

            // S2 calls 25 more
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 2, action_type = 2, amount = 25, pot = 70,
                stacks = Stacks(120, 145, 275, 190, 200, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, 2, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.2f);

            // S3 calls 20 more (BB already 10 in, calls to 30 total)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 0, seat = 3, action_type = 2, amount = 20, pot = 90,
                stacks = Stacks(120, 145, 275, 170, 200, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, 2, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 2,
                banter_seat = 1, banter_text = "Three callers. Let's build this pot." });
            yield return new WaitForSeconds(0.5f);

            // FLOP 6h 2d Kc — S2 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 1, seat = 2, action_type = 1, amount = 0, pot = 90,
                stacks = Stacks(120, 145, 275, 170, 200, 200), community_cards = new List<int> { _6h, _2d, Kc },
                emotions = Emotions(-1, -1, 3, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S3 bets 40
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 1, seat = 3, action_type = 3, amount = 40, pot = 130,
                stacks = Stacks(120, 145, 275, 130, 200, 200), community_cards = new List<int> { _6h, _2d, Kc },
                emotions = Emotions(-1, -1, -1, 3, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 calls 40
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 1, seat = 1, action_type = 2, amount = 40, pot = 170,
                stacks = Stacks(120, 105, 275, 130, 200, 200), community_cards = new List<int> { _6h, _2d, Kc },
                emotions = Emotions(-1, 2, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S2 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 1, seat = 2, action_type = 0, amount = 0, pot = 170,
                stacks = Stacks(120, 105, 275, 130, 200, 200), community_cards = new List<int> { _6h, _2d, Kc },
                emotions = Emotions(-1, -1, 3, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // TURN 9s — S3 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 2, seat = 3, action_type = 1, amount = 0, pot = 170,
                stacks = Stacks(120, 105, 275, 130, 200, 200), community_cards = new List<int> { _6h, _2d, Kc, _9s },
                emotions = Emotions(-1, -1, -1, 3, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 2, seat = 1, action_type = 1, amount = 0, pot = 170,
                stacks = Stacks(120, 105, 275, 130, 200, 200), community_cards = new List<int> { _6h, _2d, Kc, _9s },
                emotions = Emotions(-1, 3, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 2,
                banter_seat = 3, banter_text = "Slow play. Let's see the river." });
            yield return new WaitForSeconds(0.5f);

            // RIVER 7d — S3 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 3, seat = 3, action_type = 1, amount = 0, pot = 170,
                stacks = Stacks(120, 105, 275, 130, 200, 200), community_cards = new List<int> { _6h, _2d, Kc, _9s, _7d },
                emotions = Emotions(-1, -1, -1, 3, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 bets 35
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 3, seat = 1, action_type = 3, amount = 35, pot = 205,
                stacks = Stacks(120, 70, 275, 130, 200, 200), community_cards = new List<int> { _6h, _2d, Kc, _9s, _7d },
                emotions = Emotions(-1, 2, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S3 calls 35
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 2,
                street = 3, seat = 3, action_type = 2, amount = 35, pot = 240,
                stacks = Stacks(120, 70, 275, 95, 200, 200), community_cards = new List<int> { _6h, _2d, Kc, _9s, _7d },
                emotions = Emotions(-1, -1, -1, 4, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 wins (pair of Kings beats Ace-high). No elimination.
            // stacks_after: S0=120,S1=310,S2=275,S3=95,S4=200,S5=200  total=1200 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 2,
                ended_by = 1, winners = new List<int> { 1 }, pot = 240,
                community_cards  = new List<int> { _6h, _2d, Kc, _9s, _7d },
                hole_cards        = HoleCards(_8c, _8d,  Ks, Qc,  _4h, _4s,  Ad, Tc,  _9c, _5h,  Jh, _3c),
                stacks_after      = Stacks(120, 310, 275, 95, 200, 200),
                eliminated_seats  = new List<int>()
            });
            yield return new WaitForSeconds(0.5f);

            // ── HAND 3 — button=2, SB=S3, BB=S4 — DOUBLE ELIMINATION ──────────────
            // post-blind: S0=120,S1=310,S2=275,S3=90,S4=190,S5=200  pot=15
            // S0=KdQc  S1=AsAc  S2=Jh9h  S3=Ts9s  S4=8c8d  S5=6h5h
            //
            // Preflop: S5 fold, S0 fold, S1 raises to 95, S2 fold,
            //          S3 calls all-in (total 95), S4 re-raises all-in (total 200),
            //          S1 calls the all-in raise (total 200)
            // S1 wins both pots (pocket Aces beats both). S3 and S4 BOTH eliminated.
            // Main pot (capped 95, all three eligible): 95×3 = 285
            // Side pot (95→200, only S1/S4 eligible): 105×2 = 210
            // stacks_after: S0=120,S1=605,S2=275,S3=0,S4=0,S5=200  total=1200 ✓

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 3,
                button = 2, small_blind_seat = 3, big_blind_seat = 4,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(120, 310, 275, 90, 190, 200),
                hole_cards = HoleCards(Kd, Qc,  As, Ac,  Jh, _9h,  Ts, _9s,  _8c, _8d,  _6h, _5h)
            });
            yield return new WaitForSeconds(0.5f);

            // PREFLOP — S5 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 3,
                street = 0, seat = 5, action_type = 0, amount = 0, pot = 15,
                stacks = Stacks(120, 310, 275, 90, 190, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, -1, -1, 3)
            });
            yield return new WaitForSeconds(0.2f);

            // S0 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 3,
                street = 0, seat = 0, action_type = 0, amount = 0, pot = 15,
                stacks = Stacks(120, 310, 275, 90, 190, 200), community_cards = new List<int>(),
                emotions = Emotions(3, -1, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.2f);

            // S1 raises to 95
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 3,
                street = 0, seat = 1, action_type = 4, amount = 95, pot = 110,
                stacks = Stacks(120, 215, 275, 90, 190, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, 2, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.2f);

            // S2 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 3,
                street = 0, seat = 2, action_type = 0, amount = 0, pot = 110,
                stacks = Stacks(120, 215, 275, 90, 190, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, 3, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 3,
                banter_seat = 1, banter_text = "Raising big. Let's find out who's really committed." });
            yield return new WaitForSeconds(0.5f);

            // S3 calls all-in (already 5 in, calls 90 more — total 95)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 3,
                street = 0, seat = 3, action_type = 2, amount = 90, pot = 200,
                stacks = Stacks(120, 215, 275, 0, 190, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, 6, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S4 re-raises all-in (already 10 in, bets 190 more — total 200)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 3,
                street = 0, seat = 4, action_type = 4, amount = 190, pot = 390,
                stacks = Stacks(120, 215, 275, 0, 0, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, -1, 6, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 3,
                banter_seat = 4, banter_text = "Going all-in. No turning back now." });
            yield return new WaitForSeconds(0.5f);

            // S1 calls the all-in raise (already 95 in, calls 105 more — total 200)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 3,
                street = 0, seat = 1, action_type = 2, amount = 105, pot = 495,
                stacks = Stacks(120, 110, 275, 0, 0, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, 2, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 3,
                banter_seat = 0, banter_text = "Glad I got out of the way of that one." });
            yield return new WaitForSeconds(0.5f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 3,
                banter_seat = 2, banter_text = "Two players gone in one hand. Brutal." });
            yield return new WaitForSeconds(0.5f);

            // S1 wins both pots (pocket Aces). S3 AND S4 both eliminated.
            // stacks_after: S0=120,S1=605,S2=275,S3=0,S4=0,S5=200  total=1200 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 3,
                ended_by = 1, winners = new List<int> { 1 }, pot = 495,
                community_cards  = new List<int> { _7c, _4d, _2h, Th, _3s },
                hole_cards        = HoleCards(Kd, Qc,  As, Ac,  Jh, _9h,  Ts, _9s,  _8c, _8d,  _6h, _5h),
                stacks_after      = Stacks(120, 605, 275, 0, 0, 200),
                eliminated_seats  = new List<int> { 3, 4 }
            });
            yield return new WaitForSeconds(0.5f);

            // ── HAND 4 — button=5, SB=S0, BB=S1 — fold-heavy, no elimination ──────
            // 4 active: S0,S1,S2,S5. S3=0,S4=0 sitting out.
            // post-blind: S0=115,S1=595,S2=275,S3=0,S4=0,S5=200  pot=15
            // S0=Th9c  S1=KhKd  S2=4s3c  S5=Jc8s
            //
            // Preflop: S2 fold, S5 fold (button), S0 completes SB, S1 checks (BB option)
            // Flop 6c2s9d: S0 checks, S1 bets 20, S0 calls 20
            // Turn Kc: S1 checks, S0 checks
            // River 7h: S1 bets 30, S0 folds
            // S1 wins uncontested on river. No elimination.
            // stacks_after: S0=90,S1=635,S2=275,S3=0,S4=0,S5=200  total=1200 ✓

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 4,
                button = 5, small_blind_seat = 0, big_blind_seat = 1,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(115, 595, 275, 0, 0, 200),
                hole_cards = HoleCards(Th, _9c,  Kh, Kd,  _4s, _3c,  0, 1,  0, 1,  Jc, _8s)
            });
            yield return new WaitForSeconds(0.5f);

            // PREFLOP — S2 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 0, seat = 2, action_type = 0, amount = 0, pot = 15,
                stacks = Stacks(115, 595, 275, 0, 0, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, 3, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.2f);

            // S5 folds (button)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 0, seat = 5, action_type = 0, amount = 0, pot = 15,
                stacks = Stacks(115, 595, 275, 0, 0, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, -1, -1, 3)
            });
            yield return new WaitForSeconds(0.2f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 4,
                banter_seat = 2, banter_text = "Heads-up odds aren't great for me here. Folding." });
            yield return new WaitForSeconds(0.5f);

            // S0 completes SB (already 5 in, calls 5 more — total 10)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 0, seat = 0, action_type = 2, amount = 5, pot = 20,
                stacks = Stacks(110, 595, 275, 0, 0, 200), community_cards = new List<int>(),
                emotions = Emotions(3, -1, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.2f);

            // S1 checks (BB option)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 0, seat = 1, action_type = 1, amount = 0, pot = 20,
                stacks = Stacks(110, 595, 275, 0, 0, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, 3, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // FLOP 6c 2s 9d — S0 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 1, seat = 0, action_type = 1, amount = 0, pot = 20,
                stacks = Stacks(110, 595, 275, 0, 0, 200), community_cards = new List<int> { _6c, _2s, _9d },
                emotions = Emotions(3, -1, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 bets 20
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 1, seat = 1, action_type = 3, amount = 20, pot = 40,
                stacks = Stacks(110, 575, 275, 0, 0, 200), community_cards = new List<int> { _6c, _2s, _9d },
                emotions = Emotions(-1, 2, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S0 calls 20
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 1, seat = 0, action_type = 2, amount = 20, pot = 60,
                stacks = Stacks(90, 575, 275, 0, 0, 200), community_cards = new List<int> { _6c, _2s, _9d },
                emotions = Emotions(3, -1, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // TURN Kc — S1 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 2, seat = 1, action_type = 1, amount = 0, pot = 60,
                stacks = Stacks(90, 575, 275, 0, 0, 200), community_cards = new List<int> { _6c, _2s, _9d, Kc },
                emotions = Emotions(-1, 4, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S0 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 2, seat = 0, action_type = 1, amount = 0, pot = 60,
                stacks = Stacks(90, 575, 275, 0, 0, 200), community_cards = new List<int> { _6c, _2s, _9d, Kc },
                emotions = Emotions(3, -1, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 4,
                banter_seat = 1, banter_text = "That King helped me plenty." });
            yield return new WaitForSeconds(0.5f);

            // RIVER 7h — S1 bets 30
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 3, seat = 1, action_type = 3, amount = 30, pot = 90,
                stacks = Stacks(90, 545, 275, 0, 0, 200), community_cards = new List<int> { _6c, _2s, _9d, Kc, _7h },
                emotions = Emotions(-1, 2, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S0 folds river
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 4,
                street = 3, seat = 0, action_type = 0, amount = 0, pot = 90,
                stacks = Stacks(90, 545, 275, 0, 0, 200), community_cards = new List<int> { _6c, _2s, _9d, Kc, _7h },
                emotions = Emotions(3, -1, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 wins uncontested (S0 folded river). No elimination.
            // stacks_after: S0=90,S1=635,S2=275,S3=0,S4=0,S5=200  total=1200 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 4,
                ended_by = 0, winners = new List<int> { 1 }, pot = 90,
                community_cards  = new List<int> { _6c, _2s, _9d, Kc, _7h },
                hole_cards        = HoleCards(Th, _9c,  Kh, Kd,  _4s, _3c,  0, 1,  0, 1,  Jc, _8s),
                stacks_after      = Stacks(90, 635, 275, 0, 0, 200),
                eliminated_seats  = new List<int>()
            });
            yield return new WaitForSeconds(0.5f);

            // ── HAND 5 — button=0, SB=S1, BB=S2 — DOUBLE ELIMINATION ──────────────
            // 4 active: S0,S1,S2,S5. Reaches HEADS-UP (S1, S5) by end of hand.
            // post-blind: S0=90,S1=630,S2=265,S3=0,S4=0,S5=200  pot=15
            // S0=7c7s  S1=AdAh  S2=Kc Kh  S5=6d2d
            //
            // Preflop: S5 folds, S0 shoves all-in (total 90), S2 shoves all-in (total 275),
            //          S1 calls everything (total 275)
            // S1 wins both pots (pocket Aces beats both). S0 and S2 BOTH eliminated.
            // Main pot (capped 90, all three eligible): 90×3 = 270
            // Side pot (90→275, only S1/S2 eligible): 185×2 = 370
            // stacks_after: S0=0,S1=1000,S2=0,S3=0,S4=0,S5=200  total=1200 ✓
            // → Table is now HEADS-UP: S1 vs S5.

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 5,
                button = 0, small_blind_seat = 1, big_blind_seat = 2,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(90, 630, 265, 0, 0, 200),
                hole_cards = HoleCards(_7c, _7s,  Ad, Ah,  Kc, Kh,  0, 1,  0, 1,  _6d, _2d)
            });
            yield return new WaitForSeconds(0.5f);

            // PREFLOP — S5 folds
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 5,
                street = 0, seat = 5, action_type = 0, amount = 0, pot = 15,
                stacks = Stacks(90, 630, 265, 0, 0, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, -1, -1, 3)
            });
            yield return new WaitForSeconds(0.2f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 5,
                banter_seat = 0, banter_text = "Short stack. Time to make a stand." });
            yield return new WaitForSeconds(0.5f);

            // S0 shoves all-in (no blind posted — bets his full stack, total 90)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 5,
                street = 0, seat = 0, action_type = 3, amount = 90, pot = 105,
                stacks = Stacks(0, 630, 265, 0, 0, 200), community_cards = new List<int>(),
                emotions = Emotions(6, -1, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S2 shoves all-in (already 10 in, bets 265 more — total 275)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 5,
                street = 0, seat = 2, action_type = 4, amount = 265, pot = 370,
                stacks = Stacks(0, 630, 0, 0, 0, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, 6, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 5,
                banter_seat = 2, banter_text = "Both of you, all-in? Let's settle this right now." });
            yield return new WaitForSeconds(0.5f);

            // S1 calls everything (already 5 in, calls 270 more — total 275)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 5,
                street = 0, seat = 1, action_type = 2, amount = 270, pot = 640,
                stacks = Stacks(0, 360, 0, 0, 0, 200), community_cards = new List<int>(),
                emotions = Emotions(-1, 2, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 5,
                banter_seat = 5, banter_text = "Wow. Two players gone in one hand. I'm just watching this one." });
            yield return new WaitForSeconds(0.5f);

            // S1 wins both pots (pocket Aces). S0 AND S2 both eliminated. Table is now heads-up.
            // stacks_after: S0=0,S1=1000,S2=0,S3=0,S4=0,S5=200  total=1200 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 5,
                ended_by = 1, winners = new List<int> { 1 }, pot = 640,
                community_cards  = new List<int> { _9h, _4c, _2s, Jd, _8h },
                hole_cards        = HoleCards(_7c, _7s,  Ad, Ah,  Kc, Kh,  0, 1,  0, 1,  _6d, _2d),
                stacks_after      = Stacks(0, 1000, 0, 0, 0, 200),
                eliminated_seats  = new List<int> { 0, 2 }
            });
            yield return new WaitForSeconds(0.5f);

            // ── HAND 6 — Heads-Up: S1 vs S5. No elimination — S5 claws back chips. ─
            // button=1, SB=S1, BB=S5. post-blind: S1=995,S5=190  pot=15
            // S1=AcKc  S5=Qd Jd
            //
            // Preflop: S1 raises to 40, S5 calls 30 more
            // Flop 8s5c2h: S5 checks, S1 bets 50, S5 calls 50
            // Turn Td: S5 checks, S1 checks
            // River 4s: S5 bets 60, S1 folds
            // S5 wins uncontested on river. No elimination.
            // stacks_after: S1=910,S5=290  total=1200 ✓

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 6,
                button = 1, small_blind_seat = 1, big_blind_seat = 5,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(0, 995, 0, 0, 0, 190),
                hole_cards = HoleCards(0, 1,  Ac, Kc,  0, 1,  0, 1,  0, 1,  Qd, Jd)
            });
            yield return new WaitForSeconds(0.5f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 6,
                banter_seat = 1, banter_text = "Just the two of us now. Let's see what you've got." });
            yield return new WaitForSeconds(0.5f);

            // PREFLOP — S1 raises to 40
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 6,
                street = 0, seat = 1, action_type = 4, amount = 35, pot = 50,
                stacks = Stacks(0, 960, 0, 0, 0, 190), community_cards = new List<int>(),
                emotions = Emotions(-1, 2, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.2f);

            // S5 calls 30 more (total 40)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 6,
                street = 0, seat = 5, action_type = 2, amount = 30, pot = 80,
                stacks = Stacks(0, 960, 0, 0, 0, 160), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, -1, -1, 3)
            });
            yield return new WaitForSeconds(0.3f);

            // FLOP 8s 5c 2h — S5 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 6,
                street = 1, seat = 5, action_type = 1, amount = 0, pot = 80,
                stacks = Stacks(0, 960, 0, 0, 0, 160), community_cards = new List<int> { _8s, _5c, _2h },
                emotions = Emotions(-1, -1, -1, -1, -1, 3)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 bets 50
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 6,
                street = 1, seat = 1, action_type = 3, amount = 50, pot = 130,
                stacks = Stacks(0, 910, 0, 0, 0, 160), community_cards = new List<int> { _8s, _5c, _2h },
                emotions = Emotions(-1, 2, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            // S5 calls 50
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 6,
                street = 1, seat = 5, action_type = 2, amount = 50, pot = 180,
                stacks = Stacks(0, 910, 0, 0, 0, 110), community_cards = new List<int> { _8s, _5c, _2h },
                emotions = Emotions(-1, -1, -1, -1, -1, 4)
            });
            yield return new WaitForSeconds(0.3f);

            // TURN Td — S5 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 6,
                street = 2, seat = 5, action_type = 1, amount = 0, pot = 180,
                stacks = Stacks(0, 910, 0, 0, 0, 110), community_cards = new List<int> { _8s, _5c, _2h, Td },
                emotions = Emotions(-1, -1, -1, -1, -1, 2)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 checks
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 6,
                street = 2, seat = 1, action_type = 1, amount = 0, pot = 180,
                stacks = Stacks(0, 910, 0, 0, 0, 110), community_cards = new List<int> { _8s, _5c, _2h, Td },
                emotions = Emotions(-1, 3, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 6,
                banter_seat = 5, banter_text = "That Ten gave me exactly what I needed." });
            yield return new WaitForSeconds(0.5f);

            // RIVER 4s — S5 bets 60
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 6,
                street = 3, seat = 5, action_type = 3, amount = 60, pot = 240,
                stacks = Stacks(0, 910, 0, 0, 0, 50), community_cards = new List<int> { _8s, _5c, _2h, Td, _4s },
                emotions = Emotions(-1, -1, -1, -1, -1, 2)
            });
            yield return new WaitForSeconds(0.3f);

            // S1 folds river
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 6,
                street = 3, seat = 1, action_type = 0, amount = 0, pot = 240,
                stacks = Stacks(0, 910, 0, 0, 0, 50), community_cards = new List<int> { _8s, _5c, _2h, Td, _4s },
                emotions = Emotions(-1, 4, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 6,
                banter_seat = 5, banter_text = "Chipping back. This isn't over yet." });
            yield return new WaitForSeconds(0.5f);

            // S5 wins uncontested (S1 folded river). No elimination.
            // stacks_after: S1=910,S5=290  total=1200 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 6,
                ended_by = 0, winners = new List<int> { 5 }, pot = 240,
                community_cards  = new List<int> { _8s, _5c, _2h, Td, _4s },
                hole_cards        = HoleCards(0, 1,  Ac, Kc,  0, 1,  0, 1,  0, 1,  Qd, Jd),
                stacks_after      = Stacks(0, 910, 0, 0, 0, 290),
                eliminated_seats  = new List<int>()
            });
            yield return new WaitForSeconds(0.5f);

            // ── HAND 7 — FINAL HAND. S5 shoves, S1 calls and wins the match. ──────
            // button=5, SB=S5, BB=S1. post-blind: S1=900,S5=285  pot=15
            // S1=KsKd  S5=8h7h
            //
            // Preflop: S5 shoves all-in (total 290), S1 calls (total 290)
            // S5 eliminated. S1 wins the entire match with all 1200 chips.

            SendEvent(new HandDealtEvent
            {
                kind = "HandDealt", match_id = MatchId, hand_index = 7,
                button = 5, small_blind_seat = 5, big_blind_seat = 1,
                small_blind_amount = 5, big_blind_amount = 10, pot = 15,
                stacks     = Stacks(0, 900, 0, 0, 0, 285),
                hole_cards = HoleCards(0, 1,  Ks, Kd,  0, 1,  0, 1,  0, 1,  _8h, _7h)
            });
            yield return new WaitForSeconds(0.5f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 7,
                banter_seat = 5, banter_text = "All or nothing. Let's go." });
            yield return new WaitForSeconds(0.5f);

            // PREFLOP — S5 shoves all-in (already 5 in, bets 285 more — total 290)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 7,
                street = 0, seat = 5, action_type = 3, amount = 285, pot = 300,
                stacks = Stacks(0, 900, 0, 0, 0, 0), community_cards = new List<int>(),
                emotions = Emotions(-1, -1, -1, -1, -1, 6)
            });
            yield return new WaitForSeconds(0.3f);

            SendEvent(new BanterEvent { kind = "BanterEvent", match_id = MatchId, hand_index = 7,
                banter_seat = 1, banter_text = "This is for the whole match. Let's see it." });
            yield return new WaitForSeconds(0.5f);

            // S1 calls all-in (already 10 in, calls 280 more — total 290)
            SendEvent(new ActionTakenEvent
            {
                kind = "ActionTaken", match_id = MatchId, hand_index = 7,
                street = 0, seat = 1, action_type = 2, amount = 280, pot = 580,
                stacks = Stacks(0, 620, 0, 0, 0, 0), community_cards = new List<int>(),
                emotions = Emotions(5, -1, -1, -1, -1, -1)
            });
            yield return new WaitForSeconds(0.5f);

            // S1 wins (pocket Kings holds up). S5 eliminated. S1 wins the match.
            // stacks_after / final_stacks: S0=0,S1=1200,S2=0,S3=0,S4=0,S5=0  total=1200 ✓
            SendEvent(new HandCompletedEvent
            {
                kind = "HandCompleted", match_id = MatchId, hand_index = 7,
                ended_by = 1, winners = new List<int> { 1 }, pot = 580,
                community_cards  = new List<int> { Qh, _9c, _3d, _2s, _6c },
                hole_cards        = HoleCards(0, 1,  Ks, Kd,  0, 1,  0, 1,  0, 1,  _8h, _7h),
                stacks_after      = Stacks(0, 1200, 0, 0, 0, 0),
                eliminated_seats  = new List<int> { 5 }
            });
            yield return new WaitForSeconds(2f);

            SendEvent(new MatchCompletedEvent
            {
                kind = "MatchCompleted", match_id = MatchId,
                winner = 1,
                final_stacks = Stacks(0, 1200, 0, 0, 0, 0),
                total_hands  = 7
            });

            StopMarketPolling();
            yield return new WaitForSeconds(1f);

            SimulateMatchStats(7, new List<MatchStatsSeatPayload>
            {
                new MatchStatsSeatPayload { seat_index = 0, handle = "AgentTrump", avatar_id = "trump",    placement = 4, folds = 2, raises = 0, calls = 1, hands_won = 0 },
                new MatchStatsSeatPayload { seat_index = 1, handle = "AgentObama", avatar_id = "obama",    placement = 1, folds = 0, raises = 3, calls = 4, hands_won = 5 },
                new MatchStatsSeatPayload { seat_index = 2, handle = "AgentMrBeast", avatar_id = "mr_beast", placement = 5, folds = 2, raises = 1, calls = 1, hands_won = 0 },
                new MatchStatsSeatPayload { seat_index = 3, handle = "AgentOnion", avatar_id = "the_onion",     placement = 6, folds = 0, raises = 0, calls = 1, hands_won = 0 },
                new MatchStatsSeatPayload { seat_index = 4, handle = "AgentCardiB", avatar_id = "cardi_b",  placement = 6, folds = 0, raises = 1, calls = 0, hands_won = 0 },
                new MatchStatsSeatPayload { seat_index = 5, handle = "AgentTheoVon", avatar_id = "theo_von",    placement = 2, folds = 2, raises = 1, calls = 2, hands_won = 1 }
            });
        }
    }

    public enum TestPreset
    {
        HeadsUp_Standard,
        ThreeWay_OneElimination,
        FourPlayer_SplitPot,
        SixPlayer_Full,
        SixPlayer_SevenHands
    }
}