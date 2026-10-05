using SimplePoker.Attribute;
using SimplePoker.SaveLoad;
using UnityEngine;

namespace SimplePoker.UI
{

    [RequireComponent(typeof(UIManager))]
    /// <summary>
    /// Manages the state of the UI and enables/disables corresponding UI elements based on the current state.
    /// </summary>
    public class UIStateManager : Singleton<UIStateManager>
    {
        [field: SerializeField] public UIState UIState { get; private set; }
        [field: SerializeField, ReadOnly] public UIManager UIManager { get; private set; }

        private void Start()
        {
            UIManager = GetComponent<UIManager>();
            SetMenuState(UIState);
        }

        /// <summary>
        /// Sets the menu state based on the provided state enum.
        /// </summary>
        /// <param name="state">The UI state to set.</param>
        public void SetMenuState(UIState state)
        {
            UIManager.OptionUI?.Disable();
            UIManager.ProfileUI?.Disable();
            //UIManager.AdsUI?.Disable();
            UIManager.WinnerUI?.Disable();
            UIManager.DefeatUI?.Disable();

            UIState = state;
            switch (UIState)
            {
                case UIState.MENU:
                    UIManager.MenuUI?.Enable();
                    break;
                case UIState.OPTIONS:
                    UIManager.OptionUI?.Enable();
                    break;
                case UIState.PROFILE:
                    UIManager.ProfileUI?.Enable();
                    break;
                /*case UIState.ADS:
                    UIManager.AdsUI?.Enable();
                    break;*/
                case UIState.GAMEPLAY:
                    UIManager.GameplayUI?.Enable();
                    break;
                case UIState.WINNER:
                    UIManager.WinnerUI?.Enable();
                    break;
                case UIState.DEFEAT:
                    UIManager.DefeatUI?.Enable();
                    break;
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            // For debug purposes: pressing F1 deletes the game save.
            if (Input.GetKeyDown(KeyCode.F1))
                DatabaseManager.DeleteSave();
#endif
        }
    }

    public enum UIState
    {
        MENU, OPTIONS, PROFILE, ADS, GAMEPLAY, WINNER, DEFEAT
    }

}