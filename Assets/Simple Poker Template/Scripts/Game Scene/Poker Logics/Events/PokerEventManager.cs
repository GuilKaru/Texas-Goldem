using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePoker.Logic
{
    /// <summary>
    /// The PokerEventManager class manages events related to the poker game, 
    /// such as round finishing, new round starting, player turn changing, and match ending.
    /// </summary>
    public class PokerEventManager : MonoBehaviour
    {
        public static Action<List<PlayerBase>> OnRoundFinished;
        public static Action OnNewRoundStarted;
        public static Action<PlayerBase, GameManager.TurnPoker> OnPlayerTurnChanged;
        public static Action OnTurnChanged;
        public static Action<int, int> OnPotAmountChanged;
        public static Action<PlayerBase> OnMatchEnd;

    }
}