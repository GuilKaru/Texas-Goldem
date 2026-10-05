using UnityEngine;

namespace SimplePoker.Logic
{
    [System.Serializable]
    /// <summary>
    /// Represents statistics for the current state of the poker game, 
    /// including the total pot, current bet amount, and the last player who took action.
    /// </summary>
    public class PokerStats
    {
        public int TotalPot { get; private set; }
        public int Bet { get; private set; }
        [field: SerializeField] public PlayerBase LastPlayerAction { get; private set; }

        public PokerStats(PlayerBase lastPlayerAction, int totalPot, int bet)
        {
            LastPlayerAction = lastPlayerAction;
            TotalPot = totalPot;
            Bet = bet;
        }
    }
}