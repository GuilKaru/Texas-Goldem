using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TexasHoldem
{
    public class DealerChipController : MonoBehaviour
    {
        [Serializable]
        public class SeatDealerChipOffset
        {
            public int seatIndex;
            public Vector2 offset;
        }

        [Header("References")]
        [SerializeField] private RectTransform dealerChip;

        [Header("Per-Seat Dealer Chip Offsets")]
        [SerializeField] private List<SeatDealerChipOffset> seatOffsets = new List<SeatDealerChipOffset>
        {
            new SeatDealerChipOffset { seatIndex = 0, offset = new Vector2(0f, 45f) },
            new SeatDealerChipOffset { seatIndex = 1, offset = new Vector2(0f, 45f) },
            new SeatDealerChipOffset { seatIndex = 2, offset = new Vector2(0f, 45f) },
            new SeatDealerChipOffset { seatIndex = 3, offset = new Vector2(0f, 45f) },
            new SeatDealerChipOffset { seatIndex = 4, offset = new Vector2(0f, 45f) },
            new SeatDealerChipOffset { seatIndex = 5, offset = new Vector2(0f, 45f) },
        };

        [Header("Fallback")]
        [SerializeField] private Vector2 fallbackOffset = new Vector2(0f, 45f);

        [Header("Animation")]
        [SerializeField] private float moveDuration = 0.35f;
        [SerializeField] private Ease moveEase = Ease.OutBack;

        private void Awake()
        {
            if (dealerChip != null)
                dealerChip.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            PokerTableEvents.OnDealerButtonChanged += MoveDealerChipToSeat;
            PokerTableEvents.OnMatchInitialized += HideDealerChip;
        }

        private void OnDisable()
        {
            PokerTableEvents.OnDealerButtonChanged -= MoveDealerChipToSeat;
            PokerTableEvents.OnMatchInitialized -= HideDealerChip;
        }

        private void MoveDealerChipToSeat(int seatIndex)
        {
            if (!dealerChip)
            {
                Debug.LogWarning("[DealerChipController] Dealer chip reference is missing.");
                return;
            }

            SeatController seat = GameManager.Instance?.GetSeatController(seatIndex);
            if (!seat)
            {
                Debug.LogWarning($"[DealerChipController] Could not find seat {seatIndex}.");
                return;
            }

            RectTransform seatRect = seat.GetComponent<RectTransform>();
            if (!seatRect)
            {
                Debug.LogWarning($"[DealerChipController] Seat {seatIndex} has no RectTransform.");
                return;
            }

            Vector2 offset = GetOffsetForSeat(seatIndex);

            dealerChip.gameObject.SetActive(true);
            dealerChip.SetParent(seatRect, worldPositionStays: false);

            dealerChip.DOKill();
            dealerChip.DOAnchorPos(offset, moveDuration).SetEase(moveEase);
        }

        private Vector2 GetOffsetForSeat(int seatIndex)
        {
            foreach (SeatDealerChipOffset entry in seatOffsets)
            {
                if (entry != null && entry.seatIndex == seatIndex)
                    return entry.offset;
            }

            return fallbackOffset;
        }

        private void HideDealerChip()
        {
            if (!dealerChip) return;

            dealerChip.DOKill();
            dealerChip.gameObject.SetActive(false);
        }
    }
}