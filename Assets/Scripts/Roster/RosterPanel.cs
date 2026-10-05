using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TexasHoldem
{
    public class RosterPanel : MonoBehaviour
    {
        public static RosterPanel Instance { get; private set; }

        // ── Inspector refs ────────────────────────────────────────────────────

        [Header("Row Slots (assign all 6 in order, top to bottom)")]
        [SerializeField] private RosterRow[] rowSlots = new RosterRow[6];

        [FormerlySerializedAs("activerowSpacing")]
        [FormerlySerializedAs("rowSpacing")]
        [Header("Layout")]
        [Tooltip("Vertical spacing between rows in local canvas units.")]
        [SerializeField] private float activeRowSpacing   = 160f;

        [Tooltip("Y position of the topmost row (anchored position).")]
        [SerializeField] private float topRowY      = -80f;
        
        [Tooltip("Spacing between the last active row and the first eliminated row.")]
        [SerializeField] private float activeToEliminatedSpacing = 95f;

        [Tooltip("Spacing between eliminated compact rows.")]
        [SerializeField] private float eliminatedRowSpacing = 70f;

        // ── Runtime ───────────────────────────────────────────────────────────

        // seatIndex → RosterRow
        private readonly Dictionary<int, RosterRow> _rowBySeat = new Dictionary<int, RosterRow>();

        // Current ordered seat lists — updated by RefreshOrder()
        private readonly List<int> _activeSeats     = new List<int>();
        private readonly List<int> _eliminatedSeats = new List<int>();

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // All slots hidden until match start
            for (int i = 0; i < rowSlots.Length; i++)
                if (rowSlots[i] != null)
                    rowSlots[i].gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Called by GameManager at match start.
        /// Activates exactly seat_count rows and binds each to its SeatController.
        /// seats must be ordered by seat_index ascending.
        /// </summary>
        public void Initialize(List<SeatSetupData> seats, List<SeatController> controllers)
        {
            _rowBySeat.Clear();
            _activeSeats.Clear();
            _eliminatedSeats.Clear();

            int count = Mathf.Min(seats.Count, rowSlots.Length);

            for (int i = 0; i < count; i++)
            {
                SeatSetupData setup      = seats[i];
                SeatController controller = controllers[i];
                RosterRow row             = rowSlots[i];

                if (!row || !controller) continue;

                row.gameObject.SetActive(true);
                row.Bind(controller);

                _rowBySeat[setup.seat_index] = row;
                _activeSeats.Add(setup.seat_index);
            }

            // Hide unused slots
            for (int i = count; i < rowSlots.Length; i++)
                if (rowSlots[i] != null)
                    rowSlots[i].gameObject.SetActive(false);

            // Set initial positions instantly (no animation at match start)
            ApplyPositionsInstant();

            Debug.Log($"[RosterPanel] Initialized with {count} rows.");
        }
        
        /// Returns the RosterRow for the given seat index.
        /// Returns null if the seat index is not found.
        public RosterRow GetRow(int seatIndex)
        {
            _rowBySeat.TryGetValue(seatIndex, out RosterRow row);
            return row;
        }
        
        /// Called by ActionManager between hands after an elimination.
        /// Moves newly eliminated seat indices from active to eliminated list,
        /// then animates all rows to their new sorted positions.
        public void RefreshOrder(List<int> activeSeatIndices, List<int> eliminatedSeatIndices)
        {
            _activeSeats.Clear();
            _eliminatedSeats.Clear();

            if (activeSeatIndices != null)
                _activeSeats.AddRange(activeSeatIndices);

            if (eliminatedSeatIndices != null)
                _eliminatedSeats.AddRange(eliminatedSeatIndices);

            AnimatePositions();

            Debug.Log($"[RosterPanel] RefreshOrder — active: [{string.Join(",", _activeSeats)}] " +
                      $"eliminated: [{string.Join(",", _eliminatedSeats)}]");
        }

        // ── Position helpers ──────────────────────────────────────────────────
        
        /// Applies row positions instantly (no animation). Used at match start.
        private void ApplyPositionsInstant()
        {
            List<int> order = BuildDisplayOrder();

            for (int i = 0; i < order.Count; i++)
            {
                int seatIndex = order[i];
                if (!_rowBySeat.TryGetValue(seatIndex, out RosterRow row)) continue;

                RectTransform rt = row.GetComponent<RectTransform>();
                if (!rt) continue;

                rt.DOKill();
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, RowYForIndex(i));
            }
        }
        
        /// Animates all rows to their new positions after a reorder.
        private void AnimatePositions()
        {
            List<int> order = BuildDisplayOrder();

            for (int i = 0; i < order.Count; i++)
            {
                int seatIndex = order[i];
                if (!_rowBySeat.TryGetValue(seatIndex, out RosterRow row)) continue;

                RectTransform rt = row.GetComponent<RectTransform>();
                if (!rt) continue;

                Vector2 target = new Vector2(rt.anchoredPosition.x, RowYForIndex(i));
                row.AnimateToPosition(target);
            }
        }
        
        /// Returns the full display order: active seats first (ascending seat index),
        /// then eliminated seats (most recently eliminated first).
        private List<int> BuildDisplayOrder()
        {
            List<int> order = new List<int>(_activeSeats.Count + _eliminatedSeats.Count);
            order.AddRange(_activeSeats);
            order.AddRange(_eliminatedSeats);
            return order;
        }
        
        /// Converts a display-order index (0 = top) to a canvas anchoredPosition Y.
        private float RowYForIndex(int index)
        {
            int activeCount = _activeSeats.Count;

            // Active rows keep the normal big spacing.
            if (index < activeCount)
            {
                return topRowY - index * activeRowSpacing;
            }

            // Eliminated rows use compact spacing.
            int eliminatedIndex = index - activeCount;

            // If everyone somehow is eliminated or no active rows exist,
            // start eliminated rows from the top.
            if (activeCount <= 0)
            {
                return topRowY - eliminatedIndex * eliminatedRowSpacing;
            }

            // First eliminated row starts below the last active row,
            // using a smaller custom gap.
            float lastActiveY = topRowY - (activeCount - 1) * activeRowSpacing;

            return lastActiveY
                   - activeToEliminatedSpacing
                   - eliminatedIndex * eliminatedRowSpacing;
        }
    }
}