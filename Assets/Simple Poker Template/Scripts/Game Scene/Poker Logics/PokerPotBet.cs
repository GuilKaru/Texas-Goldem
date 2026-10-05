using System.Collections.Generic;
using UnityEngine;

namespace SimplePoker.Logic
{
    [System.Serializable]
    /// <summary>
    /// Manages the poker pot and bets, including tracking total pot amount, 
    /// sub-pots for each turn, adding bets, and determining winnings.
    /// </summary>
    public class PokerPotBet
    {
        [field: SerializeField] public int TotalPot { get; private set; }
        [field: SerializeField] public List<Pot> SubPotList { get; private set; }

        public PokerPotBet()
        {
            SubPotList = new List<Pot>();

            Pot antePot = new Pot(GameManager.TurnPoker.Ante);
            Pot flopPot = new Pot(GameManager.TurnPoker.Flop);
            Pot turnPot = new Pot(GameManager.TurnPoker.Turn);
            Pot riverPot = new Pot(GameManager.TurnPoker.River);

            SubPotList.Add(antePot);
            SubPotList.Add(flopPot);
            SubPotList.Add(turnPot);
            SubPotList.Add(riverPot);
        }

        /// <summary>
        /// Gets the total chips in the pot.
        /// </summary>
        /// <returns>The total chips in the pot.</returns>
        public int GetTotalPot()
        {
            int total = 0;
            for (int i = 0; i < SubPotList.Count; i++)
            {
                for (int j = 0; j < SubPotList[i].BetList.Count; j++)
                {
                    total += SubPotList[i].BetList[j].BetChips;
                }
            }

            // Notifies listeners about the pot amount change.
            PokerEventManager.OnPotAmountChanged?.Invoke(TotalPot, total);
            return total;
        }

        /// <summary>
        /// Gets the highest bet amount in the specified turn of the game.
        /// </summary>
        /// <param name="turnPoker">The turn of the game.</param>
        /// <returns>The highest bet amount.</returns>
        public int GetHighBetByTurn(GameManager.TurnPoker turnPoker)
        {
            int turnIndex = GetTurnIndex(turnPoker);

            int maxBet = 0;
            for (int i = 0; i < SubPotList[turnIndex].BetList.Count; i++)
            {
                if (maxBet <= SubPotList[turnIndex].BetList[i].BetChips)
                {
                    maxBet = SubPotList[turnIndex].BetList[i].BetChips;
                }
            }

            return maxBet;
        }

        /// <summary>
        /// Gets the bet amount of a player in the specified turn of the turn poker.
        /// </summary>
        /// <param name="player">The player to retrieve the bet amount for.</param>
        /// <param name="turnPoker">The turn of the game.</param>
        /// <returns>The bet amount of the player in the specified turn.</returns>
        public int GetPlayerBetByTurn(PlayerBase player, GameManager.TurnPoker turnPoker)
        {
            int turnIndex = GetTurnIndex(turnPoker);
            for (int i = 0; i < SubPotList[turnIndex].BetList.Count; i++)
            {
                if (SubPotList[turnIndex].BetList[i].Player == player)
                    return SubPotList[turnIndex].BetList[i].BetChips;
            }

            return 0;
        }

        /// <summary>
        /// Gets the index of the specified turn in the turn poker.
        /// </summary>
        /// <param name="turnPoker">The turn of the game.</param>
        /// <returns>The index of the specified turn.</returns>
        public int GetTurnIndex(GameManager.TurnPoker turnPoker)
        {
            int turnIndex = 0;

            switch (turnPoker)
            {
                case GameManager.TurnPoker.Ante:
                    turnIndex = 0;
                    break;
                case GameManager.TurnPoker.Flop:
                    turnIndex = 1;
                    break;
                case GameManager.TurnPoker.Turn:
                    turnIndex = 2;
                    break;
                case GameManager.TurnPoker.River:
                    turnIndex = 3;
                    break;
            }
            return turnIndex;
        }

        /// <summary>
        /// Adds a bet to the specified turn of the game for the specified player.
        /// </summary>
        /// <param name="turnPoker">The turn of the game.</param>
        /// <param name="player">The player placing the bet.</param>
        /// <param name="chips">The number of chips to bet.</param>
        public void AddBet(GameManager.TurnPoker turnPoker, PlayerBase player, int chips)
        {
            int anteIndex = 0;
            int flopIndex = 1;
            int turnIndex = 2;
            int riverIndex = 3;

            switch (turnPoker)
            {
                case GameManager.TurnPoker.Ante:
                    SubPotList[anteIndex].AddBet(player, chips);
                    break;
                case GameManager.TurnPoker.Flop:
                    SubPotList[flopIndex].AddBet(player, chips);
                    break;
                case GameManager.TurnPoker.Turn:
                    SubPotList[turnIndex].AddBet(player, chips);
                    break;
                case GameManager.TurnPoker.River:
                    SubPotList[riverIndex].AddBet(player, chips);
                    break;
            }
            TotalPot = GetTotalPot();
        }


        /// <summary>
        /// Determines the chips a player has won from the pot and updates the pot accordingly.
        /// </summary>
        /// <param name="player">The player to calculate winnings for.</param>
        /// <returns>The total chips won by the player.</returns>
        public int WinnerPotChips(PlayerBase player)
        {
            int totalChips = 0;
            for (int i = 0; i < SubPotList.Count; i++)
            {
                if (SubPotList[i].BetList.Count > 0)
                {
                    int baseChips = GetPlayerBet(SubPotList[i].BetList, player);

                    for (int j = 0; j < SubPotList[i].BetList.Count; j++)
                    {
                        int restChips = SubPotList[i].BetList[j].BetChips - baseChips;
                        if (restChips <= 0)
                        {
                            totalChips += SubPotList[i].BetList[j].BetChips;
                            SubPotList[i].BetList[j].BetChips = 0;
                        }
                        else
                        {
                            totalChips += baseChips;
                            SubPotList[i].BetList[j].BetChips = restChips;
                        }
                    }
                }
            }
            return totalChips;
        }

        /// <summary>
        /// Returns any remaining chips in the pot to the respective players.
        /// </summary>
        public void ReturnRemainingChipsToPlayers()
        {
            for (int i = 0; i < SubPotList.Count; i++)
            {
                for (int j = 0; j < SubPotList[i].BetList.Count; j++)
                {
                    PlayerBase player = SubPotList[i].BetList[j].Player;
                    int remainingChips = SubPotList[i].BetList[j].BetChips;

                    // If there are remaining chips, return them to the player.
                    if (remainingChips > 0)
                        player.AddChips(remainingChips);
                }
            }
        }

        /// <summary>
        /// Gets the bet amount of a specified player from a list of bets.
        /// </summary>
        /// <param name="bets">The list of bets to search through.</param>
        /// <param name="player">The player to retrieve the bet amount for.</param>
        /// <returns>The bet amount of the specified player.</returns>
        private int GetPlayerBet(List<Bet> bets, PlayerBase player)
        {
            for (int i = 0; i < bets.Count; i++)
            {
                if (bets[i].Player == player)
                {
                    return bets[i].BetChips;
                }
            }
            return 0;
        }
    }

    [System.Serializable]
    /// <summary>
    /// Represents a bet made by a player.
    /// </summary>
    public class Bet
    {
        [field: SerializeField] public PlayerBase Player { get; private set; }
        [field: SerializeField] public int BetChips { get; set; }

        public Bet(PlayerBase player, int betChips)
        {
            Player = player;
            BetChips = betChips;
        }

    }

    [System.Serializable]
    /// <summary>
    /// Represents a pot in the poker game with the list of bets made to the pot.
    /// </summary>
    public class Pot
    {
        [field: SerializeField] public GameManager.TurnPoker TurnPoker { get; private set; }
        [field: SerializeField] public List<Bet> BetList = new List<Bet>();

        public Pot(GameManager.TurnPoker turnPoker)
        {
            TurnPoker = turnPoker;
        }

        /// <summary>
        /// Adds a bet for the specified player.
        /// </summary>
        /// <param name="player">The player making the bet.</param>
        /// <param name="chips">The number of chips being bet.</param>
        public void AddBet(PlayerBase player, int chips)
        {
            bool updateBet = false;
            Bet auxBet = null;
            for (int i = 0; i < BetList.Count; i++)
            {
                if (BetList[i].Player == player)
                {
                    updateBet = true;
                    auxBet = BetList[i];
                    break;
                }
            }

            if (updateBet)
            {
                auxBet.BetChips = chips;
            }
            else
            {
                BetList.Add(new Bet(player, chips));
            }
        }
    }
}