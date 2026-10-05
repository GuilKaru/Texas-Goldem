using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TexasHoldem
{
    public class TableNodeLayoutManager : MonoBehaviour
    {
        public static TableNodeLayoutManager Instance { get; private set; }

        // ── Inspector refs ────────────────────────────────────────────────────

        [Header("Compact Node Anchors (up to 5, designer-placed on oval)")]
        [SerializeField] private Transform[] compactAnchors = new Transform[5];

        [Header("Gap-Close Animation")]
        [SerializeField] private float gapCloseDuration = 0.5f;
        [SerializeField] private Ease  gapCloseEase     = Ease.OutQuad;

        // ── Runtime ───────────────────────────────────────────────────────────

        // seatIndex → compact node RectTransform
        private readonly Dictionary<int, RectTransform> _nodeBySeats
            = new Dictionary<int, RectTransform>();

        // Current anchor assignment: seatIndex → anchor index
        private readonly Dictionary<int, int> _anchorBySeat
            = new Dictionary<int, int>();

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── Public API ────────────────────────────────────────────────────────
        
        /// Called by SpawnManager immediately after spawning all compact node prefabs.
        /// Registers each node's RectTransform and positions it instantly at its anchor.
        /// activeSeatIndices: seat indices in seat order (ascending).
        /// nodeTransforms: parallel list — nodeTransforms[i] belongs to activeSeatIndices[i].
        public void InitializeLayout(List<int> activeSeatIndices,
                                     List<RectTransform> nodeTransforms)
        {
            _nodeBySeats.Clear();
            _anchorBySeat.Clear();

            if (activeSeatIndices == null || nodeTransforms == null) return;

            int count = Mathf.Min(activeSeatIndices.Count, nodeTransforms.Count);

            for (int i = 0; i < count; i++)
            {
                int seatIndex = activeSeatIndices[i];
                RectTransform rt = nodeTransforms[i];
                if (rt == null) continue;
                _nodeBySeats[seatIndex] = rt;
            }

            // Assign anchors and snap instantly
            AssignAnchors(activeSeatIndices);
            ApplyPositionsInstant(activeSeatIndices);

            Debug.Log($"[TableNodeLayoutManager] Initialized {count} nodes.");
        }
        
        /// Called by ActionManager between hands after a seat is eliminated.
        /// Deactivates the eliminated node, recalculates anchor assignments,
        /// and DOTweens remaining nodes to close the gap.
        /// activeSeatIndices: seat indices still active (ascending seat order).
        /// eliminatedSeatIndices: all seats eliminated so far.
        public void AnimateGapClose(List<int> activeSeatIndices,
                                    List<int> eliminatedSeatIndices)
        {
            // Deactivate eliminated nodes
            if (eliminatedSeatIndices != null)
            {
                foreach (int seat in eliminatedSeatIndices)
                {
                    if (_nodeBySeats.TryGetValue(seat, out RectTransform rt))
                    {
                        if (rt != null)
                            rt.gameObject.SetActive(false);
                    }
                }
            }

            if (activeSeatIndices == null || activeSeatIndices.Count == 0)
            {
                Debug.Log("[TableNodeLayoutManager] No active seats remaining.");
                return;
            }

            // Reassign anchors for remaining active seats
            AssignAnchors(activeSeatIndices);

            // Animate to new positions
            foreach (int seat in activeSeatIndices)
            {
                if (!_nodeBySeats.TryGetValue(seat, out RectTransform rt)) continue;
                if (rt == null) continue;

                if (!_anchorBySeat.TryGetValue(seat, out int anchorIndex)) continue;

                Transform anchor = GetAnchor(anchorIndex);
                if (anchor == null) continue;

                rt.DOKill();
                rt.DOMove(anchor.position, gapCloseDuration)
                  .SetEase(gapCloseEase);
            }

            Debug.Log($"[TableNodeLayoutManager] AnimateGapClose — active: " +
                      $"[{string.Join(",", activeSeatIndices)}]");
        }
        
        /// Returns the current world position of the compact node for the given seat.
        /// Returns Vector3.zero if the seat is not registered.
        public Vector3 GetNodePosition(int seatIndex)
        {
            if (_nodeBySeats.TryGetValue(seatIndex, out RectTransform rt) && rt != null)
                return rt.position;

            return Vector3.zero;
        }
        
        /// Returns the compact node RectTransform for the given seat index.
        /// Returns null if not found.
        public RectTransform GetNode(int seatIndex)
        {
            _nodeBySeats.TryGetValue(seatIndex, out RectTransform rt);
            return rt;
        }

        // ── Private helpers ───────────────────────────────────────────────────
        
        /// Distributes activeSeatIndices evenly across the available anchors.
        /// With N active seats and M anchors (M >= N), picks N anchors
        /// spread across [0..M-1] to keep nodes visually balanced on the oval.
        private void AssignAnchors(List<int> activeSeatIndices)
        {
            _anchorBySeat.Clear();

            if (activeSeatIndices == null || activeSeatIndices.Count == 0) return;

            int availableAnchors = ValidAnchorCount();
            int seatCount        = activeSeatIndices.Count;

            // Pick which anchor indices to use — evenly distributed
            List<int> chosenAnchors = PickAnchorIndices(seatCount, availableAnchors);

            for (int i = 0; i < seatCount && i < chosenAnchors.Count; i++)
                _anchorBySeat[activeSeatIndices[i]] = chosenAnchors[i];
        }
        
        /// Picks seatCount anchor indices spread evenly across availableAnchors slots.
        /// Example: 3 seats, 5 anchors → [0, 2, 4]
        /// Example: 5 seats, 5 anchors → [0, 1, 2, 3, 4]
        /// Example: 1 seat,  5 anchors → [2]  (center)
        private List<int> PickAnchorIndices(int seatCount, int availableAnchors)
        {
            List<int> result = new List<int>(seatCount);

            if (seatCount <= 0 || availableAnchors <= 0) return result;

            // Clamp seat count to available anchors
            int n = Mathf.Min(seatCount, availableAnchors);

            if (n == availableAnchors)
            {
                // Fill all anchors
                for (int i = 0; i < n; i++)
                    result.Add(i);

                return result;
            }

            if (n == 1)
            {
                // Single seat — use center anchor
                result.Add(availableAnchors / 2);
                return result;
            }

            // Spread evenly: step = (availableAnchors - 1) / (n - 1)
            float step = (float)(availableAnchors - 1) / (n - 1);

            for (int i = 0; i < n; i++)
                result.Add(Mathf.RoundToInt(i * step));

            return result;
        }
        
        /// Snaps all active nodes to their assigned anchor positions instantly.
        /// Used at match start.
        private void ApplyPositionsInstant(List<int> activeSeatIndices)
        {
            foreach (int seat in activeSeatIndices)
            {
                if (!_nodeBySeats.TryGetValue(seat, out RectTransform rt)) continue;
                if (!rt) continue;

                if (!_anchorBySeat.TryGetValue(seat, out int anchorIndex)) continue;

                Transform anchor = GetAnchor(anchorIndex);
                if (!anchor) continue;

                rt.DOKill();
                rt.position = anchor.position;
                rt.gameObject.SetActive(true);
            }
        }

        private Transform GetAnchor(int index)
        {
            if (index < 0 || index >= compactAnchors.Length) return null;
            return compactAnchors[index];
        }

        private int ValidAnchorCount()
        {
            int count = 0;
            for (int i = 0; i < compactAnchors.Length; i++)
                if (compactAnchors[i]) count++;
            return Mathf.Max(1, count);
        }
    }
}