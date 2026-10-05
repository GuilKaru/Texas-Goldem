using UnityEngine;

namespace TexasHoldem
{
    public class MarketDataDisplay : MonoBehaviour
    {
        public static MarketDataDisplay Instance { get; private set; }

        // ── State ─────────────────────────────────────────────────────────────

        private bool _stopped = false;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            PokerTableEvents.OnMatchOver += HandleMatchOver;
        }

        private void OnDisable()
        {
            PokerTableEvents.OnMatchOver -= HandleMatchOver;
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void ResetDisplay()
        {
            _stopped = false;
            Debug.Log("[MarketDataDisplay] Reset.");
        }

        /// <summary>
        /// Routes v2 market data to each seat's RosterRow.
        /// Per-seat price/slippage display lives in RosterRow.SetMarketData().
        /// </summary>
        public void UpdateMarketData(MarketDataPayload payload)
        {
            if (_stopped || payload?.seats == null) return;

            foreach (MarketSeatData entry in payload.seats)
            {
                RosterRow row = RosterPanel.Instance?.GetRow(entry.seat_index);
                row?.SetMarketData(entry.price, entry.slippage);
            }
        }

        public void StopUpdates()
        {
            _stopped = true;
            Debug.Log("[MarketDataDisplay] Market data updates stopped.");
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void HandleMatchOver(string winnerName)
        {
            StopUpdates();
        }
    }
}