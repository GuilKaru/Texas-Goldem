using UnityEngine;

namespace SimplePoker.UI
{

    /// <summary>
    /// Manages the UI elements in the game.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [field: Header("Menu Scene")]
        [field: SerializeField] public MenuUI MenuUI { get; private set; }
        [field: SerializeField] public OptionUI OptionUI { get; private set; }
        [field: SerializeField] public ProfileUI ProfileUI { get; private set; }
        //[field: SerializeField] public AdsUI AdsUI { get; private set; }

        [field: Header("Game Scene")]
        [field: SerializeField] public GameplayUI GameplayUI { get; private set; }
        [field: SerializeField] public WinnerUI WinnerUI { get; private set; }
        [field: SerializeField] public DefeatUI DefeatUI { get; private set; }


        public void DisableAll()
        {
            MenuUI?.Disable();
            GameplayUI?.Disable();
            OptionUI?.Disable();
            ProfileUI?.Disable();
            //AdsUI?.Disable();
            WinnerUI?.Disable();
            DefeatUI?.Disable();
        }
    }
}