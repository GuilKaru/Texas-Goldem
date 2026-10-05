using SimplePoker.Data;
using SimplePoker.SaveLoad;
using SimplePoker.ScriptableObjects;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SimplePoker.UI
{
    public class ProfileUI : BaseUI
    {
        [Header("Profile Settings")]
        [Space(10)]
        [SerializeField] private PlayerData playerData;
        private int portraitIndex;

        [Header("UI Images")]
        [Space(10)]
        [SerializeField] private Image portraitImage;


        [Header("UI Buttons")]
        [Space(10)]
        [SerializeField] private Button backButton;
        [SerializeField] private Button rightChangePortraitButton;
        [SerializeField] private Button leftChangePortraitButton;

        UnityAction rightPortraitClickAction;
        UnityAction leftPortraitClickAction;

        public static Action<Sprite> OnPortraitChanged;


        protected override void Awake()
        {
            base.Awake();
            portraitIndex = DatabaseManager.LoadPlayerPortrait();

            backButton.onClick.AddListener(OnButtonBackClicked);

            rightPortraitClickAction = () => OnButtonChangePortraitClicked(direction: 1);
            leftPortraitClickAction = () => OnButtonChangePortraitClicked(direction: -1);

            rightChangePortraitButton.onClick.AddListener(rightPortraitClickAction);
            leftChangePortraitButton.onClick.AddListener(leftPortraitClickAction);
        }

        private void OnDestroy()
        {
            backButton.onClick.RemoveListener(OnButtonBackClicked);

            rightChangePortraitButton.onClick.RemoveListener(rightPortraitClickAction);
            leftChangePortraitButton.onClick.RemoveListener(leftPortraitClickAction);
        }

        private void Start()
        {
            playerData = PokerLevelData.Instance.PlayerData;
        }

        private void OnButtonBackClicked()
        {
            UIStateManager?.SetMenuState(UIState.MENU);
        }

        private void OnButtonChangePortraitClicked(int direction)
        {
            if (direction == 1)
            {
                portraitIndex = (portraitIndex + 1) % playerData.Portraits.Length;
                portraitImage.sprite = playerData.Portraits[portraitIndex];
            }
            else if (direction == -1)
            {
                portraitIndex = (playerData.Portraits.Length + (portraitIndex - 1)) % playerData.Portraits.Length;
                portraitImage.sprite = playerData.Portraits[portraitIndex];
            }

            DatabaseManager.SavePlayerPortrait(portraitID: portraitIndex);
            OnPortraitChanged?.Invoke(playerData.Portraits[portraitIndex]);
        }

        public override void Enable()
        {
            base.Enable();
            portraitImage.sprite = playerData.Portraits[portraitIndex];
        }

        public Sprite GetPortrait()
        {
            return playerData.Portraits[portraitIndex];
        }
    }
}