using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TexasHoldem
{
    public class SeatTurnQueueLayoutManager : MonoBehaviour
    {
        [Header("Visual Queue Slots")]
        [Tooltip("Slot 0 should be the spotlight/current-player position.")]
        [SerializeField] private Transform[] visualSlots = new Transform[6];

        [Header("Animation")]
        [SerializeField] private float moveDuration = 0.35f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;

        [Header("Inactive Seat Behavior")]
        [Tooltip("Folded seats move to the end after their displayed turn is finished.")]
        [SerializeField] private bool moveFoldedSeatsToEnd = true;

        [Tooltip("All-in seats usually no longer act, so they can also move to the end after their turn.")]
        [SerializeField] private bool moveAllInSeatsToEnd = true;
        
        [Header("Spotlight Exit Path")]
        [SerializeField] private bool useExitWaypointsFromSpotlight = true;

        [SerializeField] private Transform[] exitWaypoints = new Transform[4];

        [SerializeField] private float exitPathDuration = 0.75f;
        [SerializeField] private float slotStepDuration = 0.12f;

        [SerializeField] private Ease exitPathEase = Ease.InOutSine;
        [SerializeField] private Ease slotStepEase = Ease.OutQuad;
        
        private readonly List<int> _visualOrder = new List<int>();
        private readonly HashSet<int> _inactiveThisHand = new HashSet<int>();

        private int? _previousActionSeat = null;
        private readonly Dictionary<int, int> _lastSlotBySeat = new Dictionary<int, int>();
        

        private void OnEnable()
        {
            PokerTableEvents.OnMatchInitialized += HandleMatchInitialized;
            PokerTableEvents.OnNewHand += HandleNewHand;
            PokerTableEvents.OnActionSeatChanged += HandleActionSeatChanged;
            PokerTableEvents.OnActiveSeatsChanged += HandleActiveSeatsChanged;
        }

        private void OnDisable()
        {
            PokerTableEvents.OnMatchInitialized -= HandleMatchInitialized;
            PokerTableEvents.OnNewHand -= HandleNewHand;
            PokerTableEvents.OnActionSeatChanged -= HandleActionSeatChanged;
            PokerTableEvents.OnActiveSeatsChanged -= HandleActiveSeatsChanged;
        }

        private void HandleMatchInitialized()
        {
            _inactiveThisHand.Clear();
            _previousActionSeat = null;

            RebuildOrderFromGameManager();
            ApplyLayoutInstant();
        }

        private void HandleNewHand(int handNumber)
        {
            _inactiveThisHand.Clear();
            _previousActionSeat = null;

            RebuildOrderFromGameManager();
            AnimateLayout();

            Debug.Log($"[SeatTurnQueueLayoutManager] New hand {handNumber}. Queue reset.");
        }

        private void HandleActiveSeatsChanged(List<int> activeSeats)
        {
            _visualOrder.Clear();
            _inactiveThisHand.Clear();

            if (activeSeats != null)
            {
                foreach (int seatIndex in activeSeats)
                {
                    SeatController seat = GameManager.Instance?.GetSeatController(seatIndex);
                    if (!seat || seat.IsEliminated) continue;

                    _visualOrder.Add(seatIndex);
                }
            }

            _previousActionSeat = null;

            Debug.Log($"[SeatTurnQueueLayoutManager] Active seats changed: [{string.Join(",", _visualOrder)}]");

            AnimateLayout();
        }

        private void HandleActionSeatChanged(int? activeSeat)
        {
            if (!activeSeat.HasValue)
                return;

            int currentSeat = activeSeat.Value;

            SeatController current = GameManager.Instance?.GetSeatController(currentSeat);
            if (!current || current.IsEliminated)
                return;

            int? previousSeat = _previousActionSeat;

            // The previous player has now finished their displayed turn.
            // Only now do we decide whether they should leave the active queue.
            if (previousSeat.HasValue && previousSeat.Value != currentSeat)
            {
                SeatController previous = GameManager.Instance?.GetSeatController(previousSeat.Value);

                if (previous && !previous.IsEliminated)
                {
                    if (!SeatShouldStayInActiveQueue(previous))
                        _inactiveThisHand.Add(previousSeat.Value);
                }
            }

            // Current player must always be treated as active visually.
            _inactiveThisHand.Remove(currentSeat);

            // Rebuild the order from the real current turn position.
            RebuildOrderFromCurrentSeat(currentSeat);

            _previousActionSeat = currentSeat;

            Debug.Log($"[SeatTurnQueueLayoutManager] Current action seat: {currentSeat}, order: [{string.Join(",", _visualOrder)}]");

            AnimateLayout();
        }

        private void RebuildOrderFromGameManager()
        {
            _visualOrder.Clear();

            if (!GameManager.Instance)
                return;

            foreach (SeatController seat in GameManager.Instance.AllSeats)
            {
                if (!seat || seat.IsEliminated) continue;
                _visualOrder.Add(seat.seatIndex);
            }
        }

        private void EnsureOrderContainsValidSeats()
        {
            if (GameManager.Instance == null)
                return;

            foreach (SeatController seat in GameManager.Instance.AllSeats)
            {
                if (seat == null || seat.IsEliminated) continue;
                if (_inactiveThisHand.Contains(seat.seatIndex)) continue;

                if (!_visualOrder.Contains(seat.seatIndex))
                    _visualOrder.Add(seat.seatIndex);
            }

            for (int i = _visualOrder.Count - 1; i >= 0; i--)
            {
                SeatController seat = GameManager.Instance.GetSeatController(_visualOrder[i]);

                if (seat == null || seat.IsEliminated || _inactiveThisHand.Contains(_visualOrder[i]))
                    _visualOrder.RemoveAt(i);
            }
        }

        private bool SeatShouldStayInActiveQueue(SeatController seat)
        {
            if (!seat) return false;
            if (seat.IsEliminated) return false;

            if (moveFoldedSeatsToEnd && seat.IsFolded)
                return false;

            if (moveAllInSeatsToEnd && seat.IsAllIn)
                return false;

            if (_inactiveThisHand.Contains(seat.seatIndex))
                return false;

            return true;
        }

        private List<int> BuildFinalVisualOrder()
        {
            List<int> finalOrder = new List<int>();

            // Active queue first.
            foreach (int seatIndex in _visualOrder)
            {
                SeatController seat = GameManager.Instance?.GetSeatController(seatIndex);
                if (!seat || seat.IsEliminated) continue;
                if (finalOrder.Contains(seatIndex)) continue;

                finalOrder.Add(seatIndex);
            }

            // Folded/all-in inactive seats at the end.
            if (GameManager.Instance)
            {
                foreach (SeatController seat in GameManager.Instance.AllSeats)
                {
                    if (!seat || seat.IsEliminated) continue;
                    if (!_inactiveThisHand.Contains(seat.seatIndex)) continue;
                    if (finalOrder.Contains(seat.seatIndex)) continue;

                    finalOrder.Add(seat.seatIndex);
                }
            }

            return finalOrder;
        }

        private void ApplyLayoutInstant()
        {
            List<int> finalOrder = BuildFinalVisualOrder();

            _lastSlotBySeat.Clear();

            for (int i = 0; i < finalOrder.Count && i < visualSlots.Length; i++)
            {
                SeatController seat = GameManager.Instance?.GetSeatController(finalOrder[i]);
                Transform slot = visualSlots[i];

                if (!seat || !slot) continue;

                RectTransform rt = seat.GetComponent<RectTransform>();
                if (!rt) continue;

                rt.DOKill();
                rt.position = slot.position;
                rt.rotation = slot.rotation;

                _lastSlotBySeat[finalOrder[i]] = i;
            }
        }

        private void AnimateLayout()
        {
            List<int> finalOrder = BuildFinalVisualOrder();

            Dictionary<int, int> newSlotBySeat = new Dictionary<int, int>();

            for (int i = 0; i < finalOrder.Count && i < visualSlots.Length; i++)
            {
                int seatIndex = finalOrder[i];

                SeatController seat = GameManager.Instance?.GetSeatController(seatIndex);
                Transform targetSlot = visualSlots[i];

                if (!seat || !targetSlot) continue;

                RectTransform rt = seat.GetComponent<RectTransform>();
                if (!rt) continue;

                int previousSlot = _lastSlotBySeat.TryGetValue(seatIndex, out int oldSlot)
                    ? oldSlot
                    : i;

                bool leavingSpotlight =
                    useExitWaypointsFromSpotlight &&
                    previousSlot == 0 &&
                    i > 0 &&
                    HasValidExitPath();

                rt.DOKill();

                if (leavingSpotlight)
                {
                    AnimateLeavingSpotlightThroughTurnSlots(rt, targetSlot.rotation, i);
                }
                else
                {
                    rt.DOMove(targetSlot.position, moveDuration)
                        .SetEase(moveEase);

                    rt.DORotateQuaternion(targetSlot.rotation, moveDuration)
                        .SetEase(moveEase);
                }

                newSlotBySeat[seatIndex] = i;
            }

            _lastSlotBySeat.Clear();

            foreach (KeyValuePair<int, int> pair in newSlotBySeat)
                _lastSlotBySeat[pair.Key] = pair.Value;
        }
        
        private bool HasValidExitPath()
        {
            if (exitWaypoints == null || exitWaypoints.Length == 0)
                return false;

            for (int i = 0; i < exitWaypoints.Length; i++)
            {
                if (exitWaypoints[i])
                    return true;
            }

            return false;
        }

        private void AnimateLeavingSpotlightThroughTurnSlots(
            RectTransform rt,
            Quaternion finalRotation,
            int finalSlotIndex)
        {
            List<Vector3> pathPoints = new List<Vector3>();

            // 1. Exit screen / oval route.
            if (exitWaypoints != null)
            {
                for (int i = 0; i < exitWaypoints.Length; i++)
                {
                    Transform waypoint = exitWaypoints[i];
                    if (!waypoint) continue;

                    pathPoints.Add(waypoint.position);
                }
            }

            // 2. Re-enter at the last visual slot.
            int lastSlotIndex = GetLastValidVisualSlotIndex();
            lastSlotIndex = Mathf.Max(lastSlotIndex, finalSlotIndex);

            // 3. Add each TurnSlot from last slot back to the target slot.
            // DOPath will make this smooth instead of stopping on each point.
            for (int slotIndex = lastSlotIndex; slotIndex >= finalSlotIndex; slotIndex--)
            {
                Transform slot = visualSlots[slotIndex];
                if (!slot) continue;

                pathPoints.Add(slot.position);
            }

            if (pathPoints.Count == 0)
                return;

            float totalDuration =
                exitPathDuration +
                Mathf.Max(0, lastSlotIndex - finalSlotIndex) * slotStepDuration;

            rt.DOPath(pathPoints.ToArray(), totalDuration, PathType.CatmullRom)
                .SetEase(Ease.InOutSine);

            rt.DORotateQuaternion(finalRotation, totalDuration)
                .SetEase(Ease.InOutSine);
        }
        private int GetLastValidVisualSlotIndex()
        {
            if (visualSlots == null || visualSlots.Length == 0)
                return 0;

            for (int i = visualSlots.Length - 1; i >= 0; i--)
            {
                if (visualSlots[i])
                    return i;
            }

            return 0;
        }
        
        private void RebuildOrderFromCurrentSeat(int currentSeat)
        {
            _visualOrder.Clear();

            if (!GameManager.Instance)
                return;

            SeatController current = GameManager.Instance.GetSeatController(currentSeat);
            if (!current || current.IsEliminated)
                return;

            // Slot 0 is always the current actor.
            _visualOrder.Add(currentSeat);

            int seatCount = GameManager.Instance.SeatCount;
            if (seatCount <= 0)
                return;

            // Add the rest in turn order after the current seat.
            for (int offset = 1; offset < seatCount; offset++)
            {
                int seatIndex = (currentSeat + offset) % seatCount;

                SeatController seat = GameManager.Instance.GetSeatController(seatIndex);
                if (!seat || seat.IsEliminated)
                    continue;

                if (_inactiveThisHand.Contains(seatIndex))
                    continue;

                if (!SeatShouldStayInActiveQueue(seat))
                    continue;

                if (!_visualOrder.Contains(seatIndex))
                    _visualOrder.Add(seatIndex);
            }
        }
    }
}