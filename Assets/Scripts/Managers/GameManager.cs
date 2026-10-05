using System.Collections.Generic;
using UnityEngine;

namespace TexasHoldem
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private SpawnManager   spawnManager;
        [SerializeField] private AvatarRegistry avatarRegistry;

        private List<SeatController> _seats = new List<SeatController>();

        // ── Properties ────────────────────────────────────────────────────────

        public int                   SeatCount      => _seats.Count;
        public List<SeatController>  AllSeats       => _seats;
        public AvatarRegistry        AvatarRegistry => avatarRegistry;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Seat access ───────────────────────────────────────────────────────
        
        /// Returns the SeatController for the given seat index.
        /// Returns null if the index is out of range.
        public SeatController GetSeatController(int seatIndex)
        {
            if (seatIndex >= 0 && seatIndex < _seats.Count)
                return _seats[seatIndex];
            return null;
        }

        /// <summary>Alias kept for backwards compatibility.</summary>
        public SeatController GetSeat(int seatIndex) => GetSeatController(seatIndex);

        // ── Match lifecycle ───────────────────────────────────────────────────
        
        /// Called by WebGLReceiver.InitializeMatch().
        /// Spawns seat_count compact node prefabs, initializes each SeatController,
        /// wires RosterPanel and TableNodeLayoutManager.
        public void InitializeMatch(MatchSetupPayload setup)
        {
            if (setup == null)
            {
                Debug.LogError("[GameManager] InitializeMatch called with null setup.");
                return;
            }

            if (setup.seats == null || setup.seats.Count == 0)
            {
                Debug.LogError("[GameManager] InitializeMatch — setup.seats is empty.");
                return;
            }

            int seatCount = Mathf.Clamp(setup.seat_count, 1, 6);
            Debug.Log($"[GameManager] Initializing match '{setup.match_id}' — {seatCount} seats.");

            ClearSeats();

            // Spawn compact node prefabs and register them
            _seats = spawnManager.SpawnSeats(seatCount);

            // Build parallel lists for TableNodeLayoutManager
            List<int>            activeSeatIndices = new List<int>(seatCount);
            List<RectTransform>  nodeTransforms    = new List<RectTransform>(seatCount);

            foreach (SeatController seat in _seats)
            {
                if (!seat) continue;

                seat.SetAvatarRegistry(avatarRegistry);

                // Find setup data for this seat index
                SeatSetupData seatData = setup.seats.Find(s => s.seat_index == seat.seatIndex);
                string slug = seatData?.avatar_id  ?? "";
                string name = seatData?.agent_name ?? $"Agent {seat.seatIndex}";

                seat.Initialize(seat.seatIndex, slug, name);

                activeSeatIndices.Add(seat.seatIndex);

                RectTransform rt = seat.GetComponent<RectTransform>();
                nodeTransforms.Add(rt);

                Debug.Log($"[GameManager] Seat {seat.seatIndex}: avatar={slug}, name={name}");
            }

            // Wire TableNodeLayoutManager — positions compact nodes on oval
            TableNodeLayoutManager.Instance?.InitializeLayout(activeSeatIndices, nodeTransforms);

            // Wire RosterPanel — activates and binds N roster rows
            RosterPanel.Instance?.Initialize(setup.seats, _seats);

            // Wire UIManager
            UIManager.Instance?.InitializeTable(setup);

            // Notify FeaturedSeatDisplay to reset
            PokerTableEvents.OnMatchInitialized?.Invoke();

            Debug.Log("[GameManager] Match initialized. Awaiting events.");
        }

        /// <summary>
        /// Resets all match state. Called between matches or on error.
        /// </summary>
        public void ResetMatch()
        {
            Debug.Log("[GameManager] Resetting match.");
            ActionManager.Instance?.ResetState();
            UIManager.Instance?.ResetTable();
            AudioManager.Instance?.StopAllAudio();
            ClearSeats();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void ClearSeats()
        {
            foreach (SeatController seat in _seats)
                if (seat) Destroy(seat.gameObject);
            _seats.Clear();
        }
    }
}