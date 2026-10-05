/*using SimplePoker.Attribute;
using System;
using UnityEngine;
using UnityEngine.Advertisements;

namespace SimplePoker.UnityAds
{
    [RequireComponent(typeof(RewardedAd))]
    [RequireComponent(typeof(InterstitialAd))]
    public class AdsInitializer : Singleton<AdsInitializer>, IUnityAdsInitializationListener
    {
        public static Action OnAdsInitialized;

        [SerializeField] string _androidGameId = "0000000";
        [SerializeField] string _iOSGameId = "0000000";
        [SerializeField] bool _testMode = false;
        private string _gameId;

        [field: SerializeField, ReadOnly] public bool IsInitialized { get; private set; }
        [field: SerializeField, ReadOnly] public RewardedAd RewardedAd { get; private set; }
        [field: SerializeField, ReadOnly] public InterstitialAd InterstitialAd { get; private set; }


        /*protected override void Awake()
        {
            base.Awake();
            InvokeRepeating("InitializeAds", 2f, 2f);

            RewardedAd = GetComponent<RewardedAd>();
            InterstitialAd = GetComponent<InterstitialAd>();
        }#1#

        public void InitializeAds()
        {
            Debug.Log("Trying Unity Ads initialization...");
            _testMode = false;
#if UNITY_IOS
            _gameId = _iOSGameId;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
#elif UNITY_EDITOR
            _gameId = _androidGameId; //Only for testing the functionality in the Editor
            _testMode = true;
#endif
            if (!Advertisement.isInitialized && Advertisement.isSupported)
            {
                Advertisement.Initialize(_gameId, _testMode, this);
            }
        }

        public void OnInitializationComplete()
        {
            Debug.Log("Unity Ads initialization complete.");
            IsInitialized = true;
            OnAdsInitialized?.Invoke();
            CancelInvoke("InitializeAds");
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
            IsInitialized = false;
        }
    }
}*/