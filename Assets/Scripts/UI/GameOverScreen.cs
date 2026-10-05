using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TexasHoldem
{
    public class GameOverScreen : MonoBehaviour
    {
        public static GameOverScreen Instance { get; private set; }

        // ── Inspector — Match Info ─────────────────────────────────────────────

        [Header("Match Info")]
        [SerializeField] private TextMeshProUGUI handsNumberText;
        [SerializeField] private TextMeshProUGUI totalDurationTime;

        // ── Inspector — Winner Sun ─────────────────────────────────────────────

        [Header("Winner Sun")]
        [SerializeField] private RectTransform winnerSun;
        [SerializeField] private float         winnerSunRotationSpeed = 45f;

        // ── Inspector — Rows (6 slots, drag top→bottom) ───────────────────────
        // Index 0 = top row (winner), index 5 = bottom row (first eliminated).
        // All arrays must have exactly 6 entries.

        [Header("Rows — 6 slots, drag top to bottom")]
        [SerializeField] private GameObject[]       rowRoots;       // length 6
        [SerializeField] private Image[]            rowPortraits;   // length 6
        [SerializeField] private TextMeshProUGUI[]  rowHandles;     // length 6
        [SerializeField] private TextMeshProUGUI[]  rowHandsWon;    // length 6 — shown first (Won)
        [SerializeField] private TextMeshProUGUI[]  rowFolds;       // length 6
        [SerializeField] private TextMeshProUGUI[]  rowRaises;      // length 6
        [SerializeField] private TextMeshProUGUI[]  rowCalls;       // length 6

        // ── Inspector — Screen Root ────────────────────────────────────────────

        [Header("Screen Root")]
        [SerializeField] private GameObject gameOverScreenRoot;

        // ── Inspector — Audio ─────────────────────────────────────────────────

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip   gameOverClip;

        // ── Private state ─────────────────────────────────────────────────────

        private MatchStatsPayload _pendingStats   = null;
        private bool              _matchCompleted = false;
        private int               _winnerSeat     = -1;
        private string            _lastMatchId    = null;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            Debug.Log("[GameOverScreen] Awake — singleton registered.");
        }

        private void Update()
        {
            if (winnerSun != null && winnerSun.gameObject.activeInHierarchy)
                winnerSun.Rotate(0f, 0f, winnerSunRotationSpeed * Time.deltaTime);
        }

        // ── Public API ────────────────────────────────────────────────────────
        
        /// Called by ActionManager when a new hand starts and the match ID changes.
        /// Resets the screen so it's ready for a fresh match.
        public void OnNewHand(string currentMatchId)
        {
            if (_lastMatchId != null && _lastMatchId != currentMatchId)
            {
                _pendingStats   = null;
                _matchCompleted = false;
                _winnerSeat     = -1;

                if (winnerSun != null)
                    winnerSun.localRotation = Quaternion.identity;

                if (gameOverScreenRoot != null)
                    gameOverScreenRoot.SetActive(false);

                Debug.Log("[GameOverScreen] New match detected — screen reset.");
            }
        }

        /// Signal 1: ActionManager fires this when MatchCompleted is processed.
        public void NotifyMatchCompleted(int winnerSeat)
        {
            _matchCompleted = true;
            _winnerSeat     = winnerSeat;
            _lastMatchId    = ActionManager.Instance?.CurrentMatchId;
            Debug.Log($"[GameOverScreen] Match completed signal. Winner seat: {winnerSeat}");
            TryShow();
        }

        /// Signal 2: WebGLReceiver fires this when React sends match stats.
        public void ReceiveMatchStats(string json)
        {
            try
            {
                _pendingStats = JsonConvert.DeserializeObject<MatchStatsPayload>(json);
                Debug.Log("[GameOverScreen] Match stats received.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameOverScreen] Failed to deserialize MatchStatsPayload: {e.Message}");
                return;
            }

            TryShow();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void TryShow()
        {
            if (!_matchCompleted || _pendingStats == null) return;
            PopulateAndShow();
        }

        private void PopulateAndShow()
        {
            Debug.Log($"[GameOverScreen] PopulateAndShow — total_hands={_pendingStats.total_hands}, duration={_pendingStats.duration_seconds}s, seats={(int)(_pendingStats.seats?.Count ?? 0)}");

            // ── Match info ────────────────────────────────────────────────────

            if (handsNumberText != null)
                handsNumberText.SetText($"{_pendingStats.total_hands}");
            else
                Debug.LogWarning("[GameOverScreen] handsNumberText is not wired.");

            if (totalDurationTime != null)
            {
                int minutes = _pendingStats.duration_seconds / 60;
                int seconds = _pendingStats.duration_seconds % 60;
                totalDurationTime.SetText($"{minutes:D2}:{seconds:D2}");
            }
            else
                Debug.LogWarning("[GameOverScreen] totalDurationTime is not wired.");

            // ── Sort seats: placement 1 (winner) first, highest placement last ─

            List<MatchStatsSeatPayload> sorted = new List<MatchStatsSeatPayload>();
            if (_pendingStats.seats != null)
                sorted.AddRange(_pendingStats.seats);

            sorted.Sort((a, b) => a.placement.CompareTo(b.placement));

            Debug.Log($"[GameOverScreen] Sorted {sorted.Count} seats.");
            for (int d = 0; d < sorted.Count; d++)
                Debug.Log($"[GameOverScreen]   Row {d}: handle='{sorted[d].handle}' avatar='{sorted[d].avatar_id}' placement={sorted[d].placement} F={sorted[d].folds} R={sorted[d].raises} C={sorted[d].calls}");

            // ── Populate rows ─────────────────────────────────────────────────

            AvatarRegistry avatarRegistry = FindAvatarRegistry();
            if (avatarRegistry == null)
                Debug.LogWarning("[GameOverScreen] AvatarRegistry not found via GameManager.");

            int rowCount = rowRoots != null ? rowRoots.Length : 0;
            Debug.Log($"[GameOverScreen] rowRoots.Length={rowCount}, rowHandles.Length={rowHandles?.Length ?? 0}, rowFolds.Length={rowFolds?.Length ?? 0}");

            for (int i = 0; i < rowCount; i++)
            {
                if (rowRoots[i] == null)
                {
                    Debug.LogWarning($"[GameOverScreen] rowRoots[{i}] is null — not wired in Inspector.");
                    continue;
                }

                if (i >= sorted.Count)
                {
                    rowRoots[i].SetActive(false);
                    continue;
                }

                rowRoots[i].SetActive(true);

                MatchStatsSeatPayload seat = sorted[i];

                // Portrait
                if (rowPortraits != null && i < rowPortraits.Length && rowPortraits[i] != null)
                {
                    if (avatarRegistry != null)
                    {
                        Sprite portrait = avatarRegistry.GetSprite(seat.avatar_id);
                        if (portrait != null)
                            rowPortraits[i].sprite = portrait;
                    }
                }
                else
                    Debug.LogWarning($"[GameOverScreen] rowPortraits[{i}] is null or array too short.");

                // Handle
                if (rowHandles != null && i < rowHandles.Length && rowHandles[i] != null)
                    rowHandles[i].SetText(seat.handle ?? "");
                else
                    Debug.LogWarning($"[GameOverScreen] rowHandles[{i}] is null or not wired.");

                // Won (hands won) — shown first
                if (rowHandsWon != null && i < rowHandsWon.Length && rowHandsWon[i] != null)
                    rowHandsWon[i].SetText(seat.hands_won.ToString());
                else
                    Debug.LogWarning($"[GameOverScreen] rowHandsWon[{i}] is null or not wired.");

                // Folds
                if (rowFolds != null && i < rowFolds.Length && rowFolds[i] != null)
                    rowFolds[i].SetText(seat.folds.ToString());
                else
                    Debug.LogWarning($"[GameOverScreen] rowFolds[{i}] is null or not wired.");

                // Raises
                if (rowRaises != null && i < rowRaises.Length && rowRaises[i] != null)
                    rowRaises[i].SetText(seat.raises.ToString());
                else
                    Debug.LogWarning($"[GameOverScreen] rowRaises[{i}] is null or not wired.");

                // Calls
                if (rowCalls != null && i < rowCalls.Length && rowCalls[i] != null)
                    rowCalls[i].SetText(seat.calls.ToString());
                else
                    Debug.LogWarning($"[GameOverScreen] rowCalls[{i}] is null or not wired.");
            }

            // ── Winner sun ────────────────────────────────────────────────────

            if (winnerSun && rowRoots != null && rowRoots.Length > 0 && rowRoots[0])
            {
                winnerSun.localRotation = Quaternion.identity;
                winnerSun.gameObject.SetActive(true);
            }

            // ── Show screen ───────────────────────────────────────────────────

            if (gameOverScreenRoot)
                gameOverScreenRoot.SetActive(true);
            else
                Debug.LogWarning("[GameOverScreen] gameOverScreenRoot is not wired.");

            // ── Audio ─────────────────────────────────────────────────────────

            PlayGameOverSound();

            // ── Signal frontend ───────────────────────────────────────────────

            JSBridge.SendMatchEnded();
        }
        
        /// Finds the AvatarRegistry ScriptableObject in the scene via GameManager.
        /// GameManager holds the serialized reference; we reach it through reflection-free
        /// approach: expose a public getter on GameManager.
        private AvatarRegistry FindAvatarRegistry()
        {
            // GameManager exposes AvatarRegistry via a public property (added below).
            if (GameManager.Instance)
                return GameManager.Instance.AvatarRegistry;
            return null;
        }

        private void PlayGameOverSound()
        {
            if (!audioSource || !gameOverClip) return;
            audioSource.PlayOneShot(gameOverClip);
        }
    }
}