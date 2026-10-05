using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TexasHoldem
{
    public class JSBridge : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void SendMatchRequestToJS();

        [DllImport("__Internal")]
        private static extern void SendMatchEndedToJS();

        public static event Action OnMockMatchRequested;

        public static void SendMatchRequest()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SendMatchRequestToJS();
#else
            Debug.Log("[JSBridge Mock] Match Requested.");
            OnMockMatchRequested?.Invoke();
#endif
        }

        public static void SendMatchEnded()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SendMatchEndedToJS();
#else
            Debug.Log("[JSBridge Mock] PokerMatchEnded signal sent.");
#endif
        }
    }
}