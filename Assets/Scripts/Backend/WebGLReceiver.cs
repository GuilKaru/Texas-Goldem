using Newtonsoft.Json;
using UnityEngine;

namespace TexasHoldem
{
    public class WebGLReceiver : MonoBehaviour
    {
        // ── InitializeMatch ───────────────────────────────────────────────────

        public void InitializeMatch(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError("[WebGLReceiver] InitializeMatch received empty JSON.");
                return;
            }

            MatchSetupPayload setup;
            try
            {
                setup = JsonConvert.DeserializeObject<MatchSetupPayload>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[WebGLReceiver] Failed to deserialize MatchSetupPayload: {e.Message}");
                return;
            }

            if (setup == null)
            {
                Debug.LogError("[WebGLReceiver] MatchSetupPayload is null.");
                return;
            }

            if (!GameManager.Instance)
            {
                Debug.LogError("[WebGLReceiver] GameManager.Instance is null.");
                return;
            }

            Debug.Log($"[WebGLReceiver] InitializeMatch — match_id: {setup.match_id}");
            GameManager.Instance.InitializeMatch(setup);
        }

        // ── ReceiveEvent ──────────────────────────────────────────────────────

        public void ReceiveEvent(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("[WebGLReceiver] ReceiveEvent received empty JSON.");
                return;
            }

            if (ActionManager.Instance == null)
            {
                Debug.LogError("[WebGLReceiver] ActionManager.Instance is null.");
                return;
            }

            ActionManager.Instance.EnqueueEvent(json);
        }

        // ── ResetMatch ────────────────────────────────────────────────────────

        public void ResetMatch(string json)
        {
            Debug.Log("[WebGLReceiver] ResetMatch received.");
            GameManager.Instance?.ResetMatch();
        }
        
        // ── ReceiveMarketData ─────────────────────────────────────────────────────
        // React pushes this on its Kash polling interval while the match runs.
        // Deserialization and routing handled by ActionManager.

        public void ReceiveMarketData(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("[WebGLReceiver] ReceiveMarketData received empty JSON.");
                return;
            }

            if (!ActionManager.Instance)
            {
                Debug.LogError("[WebGLReceiver] ActionManager.Instance is null.");
                return;
            }

            ActionManager.Instance.ReceiveMarketData(json);
        }

        public void StopMarketData(string _)
        {
            Debug.Log("[WebGLReceiver] StopMarketData received.");
            ActionManager.Instance?.StopMarketData();
        }

        // ── ReceiveMatchStats ─────────────────────────────────────────────────────

        public void ReceiveMatchStats(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("[WebGLReceiver] ReceiveMatchStats received empty JSON.");
                return;
            }

            MatchStatsPayload payload;
            try
            {
                payload = JsonConvert.DeserializeObject<MatchStatsPayload>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[WebGLReceiver] Failed to deserialize MatchStatsPayload: {e.Message}");
                return;
            }

            if (!GameOverScreen.Instance)
            {
                Debug.LogError("[WebGLReceiver] GameOverScreen.Instance is null.");
                return;
            }

            Debug.Log("[WebGLReceiver] ReceiveMatchStats received.");
            GameOverScreen.Instance.ReceiveMatchStats(json);
        }
    }
}