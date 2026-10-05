using SimplePoker.Audio;
using SimplePoker.Data;
using SimplePoker.Logic;
using SimplePoker.ScriptableObjects;
using SimplePoker.UI;
using UnityEngine;

namespace SimplePoker.Visual
{
    /// <summary>
    /// Represents the UI elements and functionality for the end of the poker match.
    /// </summary>
    public class UIPokerEndMatch : MonoBehaviour
    {
        private PokerGameAssetData asset;
        private SoundManager soundManager;
        private UIStateManager uiStateManager;

        private void Awake()
        {
            PokerEventManager.OnMatchEnd += PokerEventManager_HandleEndMatch;
        }

        private void OnDisable()
        {
            PokerEventManager.OnMatchEnd -= PokerEventManager_HandleEndMatch;
        }

        private void Start()
        {
            soundManager = SoundManager.Instance;
            asset = PokerGameAsset.Instance.PokerGameAssetData;
            uiStateManager = UIStateManager.Instance;
        }

        /// <summary>
        /// Handles the event when the match ends by updating the UI state based on the winner.
        /// </summary>
        /// <param name="winnerPlayer">The player who won the match.</param>
        private void PokerEventManager_HandleEndMatch(PlayerBase winnerPlayer)
        {
            if (winnerPlayer.IsPlayer)
            {
                soundManager?.PlayOneShotSong(asset.Audio_PlayerWon);
                uiStateManager.SetMenuState(UIState.WINNER);
            }
            else
            {
                soundManager?.PlayOneShotSong(asset.Audio_PlayerLost);
                uiStateManager.SetMenuState(UIState.DEFEAT);
            }
        }
    }
}