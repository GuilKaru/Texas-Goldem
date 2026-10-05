using System.Collections.Generic;
using UnityEngine;

namespace TexasHoldem
{
    public class SpawnManager : MonoBehaviour
    {
        [Header("Default Seat Prefab (fallback if slot is empty)")]
        [SerializeField] private SeatController defaultSeatPrefab;

        [Header("Per-Seat Prefabs (size 6 — leave empty to use default)")]
        [SerializeField] private SeatController[] seatPrefabs; // length 6

        [Header("Seat Anchor Positions (size 6 — designer-placed on oval)")]
        [SerializeField] private Transform[] seatAnchors; // length 6

        public List<SeatController> SpawnSeats(int numSeats)
        {
            List<SeatController> seats = new List<SeatController>();

            if (!defaultSeatPrefab && (seatPrefabs == null || seatPrefabs.Length == 0))
            {
                Debug.LogError("[SpawnManager] No seat prefabs assigned!");
                return seats;
            }

            numSeats = Mathf.Clamp(numSeats, 1, 6);

            for (int i = 0; i < numSeats; i++)
            {
                SeatController prefabToUse = GetPrefabForSeat(i);

                if (!prefabToUse)
                {
                    Debug.LogError($"[SpawnManager] No prefab for seat {i} and no default assigned!");
                    continue;
                }

                Transform anchor = GetAnchor(i);

                // Spawn as child of the anchor — position resets to zero
                // so the seat sits exactly at the anchor's position.
                SeatController seat = Instantiate(
                    prefabToUse,
                    Vector3.zero,
                    Quaternion.identity,
                    anchor ? anchor : transform);

                RectTransform seatRT = seat.GetComponent<RectTransform>();
                if (seatRT)
                {
                    seatRT.anchoredPosition = Vector2.zero;
                    seatRT.localRotation    = Quaternion.identity;
                    seatRT.localScale       = Vector3.one;
                }

                seat.seatIndex = i;
                seat.gameObject.SetActive(true);
                seat.gameObject.name = $"Seat_{i}";

                seats.Add(seat);

                Debug.Log($"[SpawnManager] Spawned Seat_{i} at anchor {i} using prefab '{prefabToUse.name}'.");
            }

            return seats;
        }

        private SeatController GetPrefabForSeat(int index)
        {
            if (seatPrefabs != null &&
                index < seatPrefabs.Length &&
                seatPrefabs[index])
            {
                return seatPrefabs[index];
            }

            return defaultSeatPrefab;
        }

        private Transform GetAnchor(int index)
        {
            if (seatAnchors != null &&
                index < seatAnchors.Length &&
                seatAnchors[index])
            {
                return seatAnchors[index];
            }

            Debug.LogWarning($"[SpawnManager] seatAnchors[{index}] not assigned. Spawning at SpawnManager position.");
            return transform;
        }
    }
}