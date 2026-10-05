using Newtonsoft.Json;
using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
// EDITOR ONLY. Press SPACE to start a mock match.
// Sends InitializeMatch with v2 seats[] list, then triggers TestBackend.RunMatch().
// Supports 2–6 players configured in the Inspector.
// ──────────────────────────────────────────────────────────────────────────────

namespace TexasHoldem
{
    public class MockFrontendTester : MonoBehaviour
    {
        [Header("References")]
        public WebGLReceiver targetReceiver;
        public TestBackend   testBackend;

        [Header("Match Config")]
        public int startingStack  = 200;
        public int smallBlind     = 1;
        public int bigBlind       = 2;
        public int handsPerLevel  = 4;

        [Header("Seats (2–6) — avatar slugs must match AvatarRegistry")]
        [Tooltip("One entry per player. Seat index = array index.")]
        public SeatSetupData[] seats = new SeatSetupData[]
        {
            new SeatSetupData { seat_index = 0, avatar_id = "trump",    agent_name = "AgentAlpha"  },
            new SeatSetupData { seat_index = 1, avatar_id = "obama",    agent_name = "AgentBeta"   },
            new SeatSetupData { seat_index = 2, avatar_id = "mr_beast", agent_name = "AgentCharlie"},
            new SeatSetupData { seat_index = 3, avatar_id = "elon",     agent_name = "AgentDelta"  },
            new SeatSetupData { seat_index = 4, avatar_id = "cardi_b",  agent_name = "AgentEcho"   },
            new SeatSetupData { seat_index = 5, avatar_id = "bezos",    agent_name = "AgentFoxtrot"},
        };

        private void Update()
        {
#if UNITY_EDITOR
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space))
                SimulateMatchInitialization();
#endif
        }

        [ContextMenu("Simulate Match Initialization")]
        public void SimulateMatchInitialization()
        {
            if (targetReceiver == null)
            {
                Debug.LogError("[MockFrontendTester] WebGLReceiver not assigned.");
                return;
            }

            if (seats == null || seats.Length < 2)
            {
                Debug.LogError("[MockFrontendTester] At least 2 seats required.");
                return;
            }

            GameManager.Instance?.ResetMatch();
            MarketDataDisplay.Instance?.ResetDisplay();

            if (testBackend != null)
                testBackend.targetReceiver = targetReceiver;

            // Normalise seat indices to match array position
            System.Collections.Generic.List<SeatSetupData> seatList
                = new System.Collections.Generic.List<SeatSetupData>();

            for (int i = 0; i < seats.Length; i++)
            {
                SeatSetupData s = seats[i];
                s.seat_index = i;
                seatList.Add(s);
            }

            MatchSetupPayload setup = new MatchSetupPayload
            {
                match_id        = "mock-match-001",
                seat_count      = seatList.Count,
                starting_stack  = startingStack,
                small_blind     = smallBlind,
                big_blind       = bigBlind,
                hands_per_level = handsPerLevel,
                seats           = seatList
            };

            string setupJson = JsonConvert.SerializeObject(setup);
            Debug.Log($"[MockFrontendTester] Sending InitializeMatch — {seatList.Count} seats.");
            targetReceiver.InitializeMatch(setupJson);

            if (testBackend != null)
                testBackend.RunMatch();
            else
                Debug.LogWarning("[MockFrontendTester] TestBackend not assigned.");
        }
    }
}