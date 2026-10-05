using SimplePoker.ScriptableObjects;
using System.Collections;
using UnityEngine;

namespace SimplePoker.Logic
{
    /// <summary>
    /// Class representing a CPU player in the poker game, managing AI actions and behavior.
    /// </summary>
    public class Cpu : PlayerBase
    {
        [field: Header("SO Cpu")]
        [field: SerializeField] public CpuData CpuData { get; private set; }
        [SerializeField] private SpriteRenderer headSpriteRenderer;
        [SerializeField] private SpriteRenderer hatSpriteRenderer;
        [SerializeField] private SpriteRenderer mustacheSpriteRenderer;
        [SerializeField] private SpriteRenderer bodySpriteRenderer;
        [SerializeField] private Sprite[] headsSprite;
        [SerializeField] private Sprite[] hatsSprite;
        [SerializeField] private Sprite[] mustachesSprite;
        [SerializeField] private Sprite[] bodiesSprite;

        /// <summary>
        /// Sets the data for the CPU player and invokes the player activated event.
        /// </summary>
        /// <param name="cpuData">The CPU player's data.</param>
        public void SetData(CpuData cpuData)
        {
            CpuData = cpuData;
            OnPlayerActived?.Invoke();
        }

        /// <summary>
        /// Initiates the CPU player's turn in the game, 
        /// handling scenarios where the CPU player is required to perform specific actions.
        /// </summary>
        /// <param name="turn">The current turn in the game.</param>
        public override void DoTurn(GameManager.TurnPoker turn)
        {
            base.DoTurn(turn);

            // If the CPU player is scheduled to fold or go all-in next turn, skip this turn
            if (ContinueFoldNextTurn || ContinueAllInNextTurn)
                return;

            // If the CPU player has no chips left, skip this turn
            if (Chips <= 0)
                return;

            StartCoroutine(TurnWaitCoroutine(turn));
        }

        /// <summary>
        /// Coroutine to introduce a delay before executing the CPU player's turn.
        /// </summary>
        /// <param name="turn">The current turn in the game.</param>
        private IEnumerator TurnWaitCoroutine(GameManager.TurnPoker turn)
        {
            yield return new WaitForSeconds(asset.Time_CpuWaitChooseAction);
            switch (turn)
            {
                case GameManager.TurnPoker.Ante:
                    ChooseAction(CpuData.Ante);
                    break;
                case GameManager.TurnPoker.Flop:
                    ChooseAction(CpuData.Flop);
                    break;
                case GameManager.TurnPoker.Turn:
                    ChooseAction(CpuData.Turn);
                    break;
                case GameManager.TurnPoker.River:
                    ChooseAction(CpuData.River);
                    break;
                case GameManager.TurnPoker.Winner:
                    break;
            }

            yield return StartCoroutine(CallNextPlayerCoroutine(asset.Time_CallNextPlayer));
        }

        /// <summary>
        /// Chooses the CPU player's action based on the given probabilities.
        /// </summary>
        /// <param name="turnChoose">The probabilities for different actions in the current turn.</param>
        private void ChooseAction(TurnChoose turnChoose)
        {
            // Generate a random number to determine the CPU player's action
            int randomNumber = Random.Range(0, 101);

            // Fold if the random number falls within the fold probability range, only if there are bets on the round.
            if (randomNumber < turnChoose.Fold &&
                gameManager.PokerStats.Bet > 0 &&
                CurrentBet < gameManager.PokerStats.Bet &&
                gameManager.PokerPotBet.GetHighBetByTurn(gameManager.Turn) != 0)
            {
                DoFold();
            }
            else if (randomNumber < turnChoose.Fold + turnChoose.Check)
            {
                DoCheckOrCall();
            }
            // Fold if the random number falls within the fold probability range, only if there last playe its not in all in.
            else if ((gameManager.PokerStats.LastPlayerAction.PokerAction == PokerAction.CALL ||
                gameManager.PokerStats.LastPlayerAction.PokerAction == PokerAction.CHECK ||
                gameManager.PokerStats.LastPlayerAction.PokerAction == PokerAction.NOTHING) &&
                randomNumber < turnChoose.Fold + turnChoose.Check + turnChoose.Bet &&
                !gameManager.PokerStats.LastPlayerAction.ContinueAllInNextTurn)
            {
                CpuRaiseRandom();
            }
            // All In if the random number falls within the fold probability range, only if there last playe its not in all in.
            else if (randomNumber < turnChoose.Fold + turnChoose.Bet + turnChoose.AllIn && !gameManager.PokerStats.LastPlayerAction.ContinueAllInNextTurn)
            {
                DoAllIn();
            }
            else
            {
                //Someone probably went all in, then a call will be given to bet the same amount.
                DoCheckOrCall();
            }
        }

        /// <summary>
        /// Randomly determines the amount to raise and executes a raise action.
        /// </summary>
        private void CpuRaiseRandom()
        {
            // Generate a random number to determine the raise value
            int randomNumber = Random.Range(0, 101);
            int raiseValue = gameManager.BetBase + 50;

            // A new bet must always be greater than a base bet, based on probability the value is chosen.
            if (randomNumber < 20)
            {
                raiseValue = gameManager.BetBase + Mathf.RoundToInt(Chips / 10);
            }
            else if (randomNumber < 40)
            {
                raiseValue = gameManager.BetBase + Mathf.RoundToInt(Chips / 8);

            }
            else if (randomNumber < 60)
            {
                raiseValue = gameManager.BetBase + Mathf.RoundToInt(Chips / 6);

            }
            else if (randomNumber < 80)
            {
                raiseValue = gameManager.BetBase + Mathf.RoundToInt(Chips / 4);
            }
            else
            {
                raiseValue = gameManager.BetBase + Mathf.RoundToInt(Chips / 2);
            }

            // Round the raise value to the nearest multiple of 50
            int round50 = Mathf.RoundToInt(raiseValue / 50);
            if (round50 <= 0)
                round50 = 1;

            raiseValue = gameManager.BetBase + round50 * 50;

            DoRaise(raiseValue);
        }
    }
}