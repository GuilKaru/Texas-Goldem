using DG.Tweening;
using SimplePoker.Audio;
using SimplePoker.Data;
using SimplePoker.SaveLoad;
using SimplePoker.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimplePoker.Logic
{
    /// <summary>
    /// The GameManager class manages the poker game, 
    /// including player turns, bets, card distribution, and round progression.
    /// It implements the Singleton pattern to ensure a single global instance is accessible.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        public enum TurnPoker
        {
            Ante = 0,
            Flop,
            Turn,
            River,
            Winner
        }

        [Header("Game Info")]
        [Space(10)]

        [SerializeField] private int dealerTurn;

        [field: SerializeField] public Deck Deck { get; private set; }
        [field: SerializeField] public TurnPoker Turn { get; private set; }


        [SerializeField] private List<PlayerBase> players = new List<PlayerBase>();
        [SerializeField] private List<PlayerBase> playersInGame = new List<PlayerBase>();

        [SerializeField] int smallBlindIndex;
        [SerializeField] int bigBlindIndex;
        [SerializeField] int initialPlayerIndex;

        [SerializeField] int playerTurnIndex;

        [SerializeField] private PlayerBase bigBlindPlayer;
        [SerializeField] private PlayerBase smallBlindPlayer;
        [SerializeField] private PlayerBase currentPlayerTurn;
        [SerializeField] private PlayerBase endPlayerTurn;

        [SerializeField] private List<Card> cardsInTable = new List<Card>();
        [SerializeField] private Transform[] cardsInTablePositions = new Transform[5];

        [SerializeField] private int currentForcedBet = 50;
        public int IncreaseForcedBet = 100;

        [field: SerializeField] public int BetBase;
        [field: SerializeField] public int TurnIndex { get; private set; }

        [SerializeField] private GameObject betIncreasedInfo;

        [Header("Game Settings")]
        [Space(10)]

        [SerializeField, Range(2, 5)] private int quantityOfPlayers = 5;
        [SerializeField] private int initialChip = 10000;
        private bool tiebreakerKickerHighCard;

        [field: SerializeField] public PokerStats PokerStats { get; private set; }
        [field: SerializeField] public PokerPotBet PokerPotBet { get; private set; }

        [Header("Audios")]
        private SoundManager soundManager;

        private PokerLevelData pokerLevelData;
        private PokerGameAssetData asset;
        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 60;
            PokerPotBet = new PokerPotBet();
            pokerLevelData = PokerLevelData.Instance;
        }

        private IEnumerator Start()
        {
            soundManager = SoundManager.Instance;
            asset = PokerGameAsset.Instance.PokerGameAssetData;

            quantityOfPlayers = pokerLevelData.LevelData.AmountOfPlayers;
            initialChip = pokerLevelData.LevelData.Ticket;
            currentForcedBet = Mathf.RoundToInt(initialChip / 20f);
            IncreaseForcedBet = currentForcedBet;
            TurnIndex = 0;
            CreateDeck();

            yield return new WaitForSeconds(asset.Time_WaitToBeginGame);
            yield return StartCoroutine(EnablePlayersInMatchCoroutine());
            yield return new WaitForSeconds(asset.Time_Delay);

            PokerEventManager.OnNewRoundStarted?.Invoke();
            yield return StartCoroutine(DistributeCardsToPlayersCoroutine());
            yield return new WaitForSeconds(asset.Time_Delay);
            yield return StartCoroutine(OrganizeFirstTurnCoroutine());
        }

        /// <summary>
        /// Creates a deck, makes it, and shuffles it.
        /// </summary>
        private void CreateDeck()
        {
            int amountOfDecks = 1;
            Deck.MakeDeck(amountOfDecks);
            Deck.ShuffleDeck();
        }

        /// <summary>
        /// Enables players in the match one by one in a specific order, symmetrically.
        /// </summary>
        private IEnumerator EnablePlayersInMatchCoroutine()
        {
            playersInGame.Clear();
            players.ForEach(p => p.gameObject.SetActive(false));
            int activePlayers = quantityOfPlayers;
            int playersEnabled = 0;
            int centerIndex = players.Count / 2;

            // Enable the first player.
            yield return EnablePlayerByIndexCoroutine(0);
            playersEnabled++;

            // Enable the player at the center if there's an even number of players.
            if (activePlayers % 2 == 0)
            {
                yield return EnablePlayerByIndexCoroutine(centerIndex);
                playersEnabled++;
            }

            // Enable players on both sides of the center.
            for (int i = 1; i <= activePlayers / 2; i++)
            {
                if (centerIndex - i > 0 && playersEnabled < quantityOfPlayers)
                {
                    yield return EnablePlayerByIndexCoroutine(centerIndex - i);
                    playersEnabled++;
                }

                if (centerIndex + i < players.Count && playersEnabled < quantityOfPlayers)
                {
                    yield return EnablePlayerByIndexCoroutine(centerIndex + i);
                    playersEnabled++;
                }
            }

            playersInGame = playersInGame.OrderBy(p => p.PositionIndex).ToList();
        }

        /// <summary>
        /// Enables a player by index, setting up their initial state and appearance.
        /// </summary>
        /// <param name="index">The index of the player to be enabled.</param>
        private IEnumerator EnablePlayerByIndexCoroutine(int index)
        {
            players[index].gameObject.SetActive(true);
            players[index].transform.localScale = Vector3.zero;
            players[index].transform.DOScale(1, asset.Time_EnablePlayerOnTable).SetEase(Ease.OutBounce);
            players[index].ResetSetup();
            players[index].SetChips(initialChip);
            playersInGame.Add(players[index]);
            yield return new WaitForSeconds(asset.Time_EnablePlayerOnTable);
            players[index].Setup(index);

            // Sets up player data based on whether the player is a human or CPU.
            if (players[index].TryGetComponent(out Human playerHuman))
                playerHuman.SetData(PokerLevelData.Instance?.PlayerData);
            else if (players[index].TryGetComponent(out Cpu playerCpu))
                playerCpu.SetData(PokerLevelData.Instance?.GetNextCpu());
        }

        /// <summary>
        /// Initial distribution of cards to players, alternating between two rounds, two cards per player.
        /// </summary>
        private IEnumerator DistributeCardsToPlayersCoroutine()
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < playersInGame.Count; j++)
                {
                    Card card = Deck.GetFirstCardFromDeck();
                    Deck.AddCardToListCardsInGame(card);
                    playersInGame[j].AddCardToHand(card);
                    yield return new WaitForSeconds(asset.Time_DistributeCard);
                }
            }
        }

        /// <summary>
        /// Organizes the initial turn of the game, including making bets and enabling player turns.
        /// </summary>
        private IEnumerator OrganizeFirstTurnCoroutine()
        {
            yield return new WaitForSeconds(asset.Time_Delay);
            MakeInitialBets();
            UpdatePokerHandOfPlayers();
            yield return new WaitForSeconds(asset.Time_WaitToBeginTurn);
            EnablePlayerTurn();
        }

        /// <summary>
        /// Makes initial bets, set the current turn , and sets up initial game state.
        /// </summary>
        private void MakeInitialBets()
        {
            if (TurnIndex % 2 == 0 && TurnIndex > 0)
            {
                currentForcedBet += IncreaseForcedBet;
                if (currentForcedBet >= initialChip / 2)
                    currentForcedBet = initialChip / 2;

                betIncreasedInfo.SetActive(true);
                Sequence sequence = DOTween.Sequence();
                sequence.Append(betIncreasedInfo.transform.DOShakeScale(0.2f));
                sequence.AppendInterval(3);
                sequence.OnComplete(() =>
                {
                    betIncreasedInfo.SetActive(false);
                });
            }
            TurnIndex++;


            if (playersInGame.Count >= 2)
            {
                dealerTurn = dealerTurn % playersInGame.Count;
                smallBlindIndex = (dealerTurn + 1) % playersInGame.Count;
                bigBlindIndex = (dealerTurn + 2) % playersInGame.Count;
                initialPlayerIndex = (dealerTurn + 3) % playersInGame.Count;
                playerTurnIndex = initialPlayerIndex;

                playersInGame[dealerTurn].SetDealer();
                playersInGame[smallBlindIndex].SetSmallBlind(currentForcedBet);
                playersInGame[bigBlindIndex].SetBigBlind(currentForcedBet * 2);

                smallBlindPlayer = playersInGame[smallBlindIndex];
                bigBlindPlayer = playersInGame[bigBlindIndex];
                currentPlayerTurn = playersInGame[initialPlayerIndex];
                endPlayerTurn = bigBlindPlayer;

                BetBase = currentForcedBet * 2;

                PokerStats = new PokerStats(bigBlindPlayer, 150, BetBase);
            }
        }

        /// <summary>
        /// Updates the poker hands of players based on the cards on the table.
        /// </summary>
        private void UpdatePokerHandOfPlayers()
        {
            foreach (PlayerBase player in playersInGame)
            {
                player.UpdatePokerHand(cardsInTable);
            }
        }

        /// <summary>
        /// Enables the turn of the current player and invokes an event signaling the turn change.
        /// </summary>
        private void EnablePlayerTurn()
        {
            PokerEventManager.OnPlayerTurnChanged?.Invoke(currentPlayerTurn, Turn);
        }

        /// <summary>
        /// Moves the game to the next player's turn and checking if it is necessary to change the round.
        /// </summary>
        public void NextPlayer()
        {
            // Updates the betting base for the current round with the highest bet of the round.
            BetBase = PokerPotBet.GetHighBetByTurn(Turn);
            int playersActiveInTable = 0;
            PlayerBase winnerPlayer = null;

            // Count active players who haven't folded.
            foreach (PlayerBase p in playersInGame)
            {
                if (!p.ContinueFoldNextTurn)
                {
                    winnerPlayer = p;
                    playersActiveInTable++;
                }
            }

            // If only one player remains active, its the end of match, no further turns are needed.
            if (playersActiveInTable <= 1)
                return;

            int lastPlayer = playerTurnIndex;
            playerTurnIndex = (playerTurnIndex + 1) % playersInGame.Count;
            currentPlayerTurn = playersInGame[playerTurnIndex];

            // If the current player finishes the round, the next poker turn begins.
            if (currentPlayerTurn == endPlayerTurn && endPlayerTurn.DidAnAction())
            {
                playerTurnIndex = smallBlindIndex;
                currentPlayerTurn = playersInGame[smallBlindIndex];
                endPlayerTurn = currentPlayerTurn;
                StartCoroutine(NextTurnCoroutine());
            }
            else
            {
                // On Ante turn, the player who finishes the round is the player with BigBlind, if he did not increase the bet, goes to the next Turn
                if (Turn == TurnPoker.Ante &&
                    endPlayerTurn == bigBlindPlayer &&
                    playersInGame[lastPlayer] == endPlayerTurn &&
                    !endPlayerTurn.IsRaise &&
                    !endPlayerTurn.IsAllIn)
                {
                    playerTurnIndex = smallBlindIndex;
                    currentPlayerTurn = playersInGame[smallBlindIndex];
                    endPlayerTurn = currentPlayerTurn;
                    StartCoroutine(NextTurnCoroutine());
                }
                else
                {
                    EnablePlayerTurn();
                }
            }
        }

        /// <summary>
        /// Initiates the next turn in the game, updating game state and managing transitions.
        /// </summary>
        public IEnumerator NextTurnCoroutine()
        {
            IsJustOneNotAllIn();
            PokerEventManager.OnTurnChanged?.Invoke();
            PokerStats pokerStats = new PokerStats(currentPlayerTurn, BetBase, 0);
            BetBase = 0;

            Turn++;
            switch (Turn)
            {
                case TurnPoker.Ante:
                    break;
                case TurnPoker.Flop:
                    yield return StartCoroutine(PokerFlopCoroutine());
                    break;
                case TurnPoker.Turn:
                    yield return StartCoroutine(PokerTurnCoroutine());
                    break;
                case TurnPoker.River:
                    yield return StartCoroutine(PokerRiverCoroutine());
                    break;
                case TurnPoker.Winner:
                    soundManager.PlayOneShotSong(asset.Audio_RoundCardsShow);
                    List<PlayerBase> playersWinnerInRound = WinnersInRound();
                    WinnersHighLightCards(playersWinnerInRound);
                    WinnersGetPotChips(playersWinnerInRound);
                    yield return new WaitForSeconds(asset.Time_WaitNextRound);
                    StartCoroutine(NewRoundCoroutine());
                    break;
            }

            yield return new WaitForSeconds(asset.Time_Delay);
        }

        /// <summary>
        /// Deals the FLOP cards and initiates the next turn.
        /// </summary>
        private IEnumerator PokerFlopCoroutine()
        {
            for (int i = 0; i < 3; i++)
            {
                DistributeCardInTable(i);
                yield return new WaitForSeconds(asset.Time_DistributeCard);
            }

            yield return new WaitForSeconds(asset.Time_NextTurnDelay);
            EnablePlayerTurn();
        }

        /// <summary>
        /// Deals the TURN card and initiates the next turn.
        /// </summary>
        private IEnumerator PokerTurnCoroutine()
        {
            int turnPosition = 3;
            DistributeCardInTable(turnPosition);

            yield return new WaitForSeconds(asset.Time_DistributeCard);
            yield return new WaitForSeconds(asset.Time_NextTurnDelay);
            EnablePlayerTurn();
        }

        /// <summary>
        /// Deals the RIVER card and initiates the next turn.
        /// </summary>
        private IEnumerator PokerRiverCoroutine()
        {
            int riverPosition = 4;
            DistributeCardInTable(riverPosition);

            yield return new WaitForSeconds(asset.Time_DistributeCard);
            yield return new WaitForSeconds(asset.Time_NextTurnDelay);
            EnablePlayerTurn();
        }

        /// <summary>
        /// Deals a card onto the table and updates the players' poker hands.
        /// </summary>
        /// <param name="positionIndex">The index of the position on the table where the card should be placed.</param>
        private void DistributeCardInTable(int positionIndex)
        {
            soundManager?.PlayOneShotSong(asset.Audio_3Cards);

            Card card = Deck.GetFirstCardFromDeck();
            card.ShowCard = true;

            Deck.AddCardToListCardsInGame(card);
            cardsInTable.Add(card);
            card.transform.SetParent(cardsInTablePositions[positionIndex]);
            card.transform.DOLocalMove(Vector2.zero, 0.3f);
            card.transform.DOLocalRotate(Vector2.zero, 0.1f);
            UpdatePokerHandOfPlayers();
        }

        /// <summary>
        /// Distributes the pot chips to the winners and returns remaining chips to players.
        /// </summary>
        /// <param name="winners">The list of players who won the round.</param>
        private void WinnersGetPotChips(List<PlayerBase> winners)
        {
            for (int i = 0; i < winners.Count; i++)
            {
                int chips = PokerPotBet.WinnerPotChips(winners[i]);
                winners[i].AddChips(chips);
            }

            PokerPotBet.ReturnRemainingChipsToPlayers();
            PokerEventManager.OnRoundFinished?.Invoke(winners);
        }

        /// <summary>
        /// Highlights the winning cards of the winners on the table.
        /// </summary>
        /// <param name="winners">The list of players who won the round.</param>
        private void WinnersHighLightCards(List<PlayerBase> winners)
        {
            for (int i = 0; i < winners.Count; i++)
            {
                for (int j = 0; j < winners[i].PokerHand.Cards.Count; j++)
                {
                    // Show outline for cards based on hand ranking
                    if (winners[i].PokerHand.Hand != HAND.HIGH_CARD)
                    {
                        winners[i].PokerHand.Cards[j].ShowOutline(asset.Color_HighlightPokerHandWinner);

                        // Show outline for kicker card if applicable
                        if (tiebreakerKickerHighCard && winners[i].PokerHand.KickerHighCard != null)
                            winners[i].PokerHand.KickerHighCard.ShowOutline(asset.Color_HighlightPokerHandWinner);
                    }
                    else
                    {
                        // Show outline for kicker card if applicable
                        if (winners[i].PokerHand.KickerHighCard != null)
                        {
                            winners[i].PokerHand.KickerHighCard.ShowOutline(asset.Color_HighlightPokerHandWinner);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines the winners in the current round based on their poker hands.
        /// </summary>
        private List<PlayerBase> WinnersInRound()
        {
            // Exclude folded players from round.
            List<PlayerBase> players = playersInGame.Where(p => p.ContinueFoldNextTurn == false).ToList();

            // Identify the highest hand among active players.
            HAND currentHand = players.Select(h => h.PokerHand.Hand).OrderByDescending(h => h).First();
            List<PlayerBase> possibleWinners = players.Select(h => h).Where(h => h.PokerHand.Hand == currentHand).ToList();

            // Resolve tiebreakers among current winners.
            List<PlayerBase> winners = new Tiebreaker().PokerHandsTiebreaker(possibleWinners, currentHand);

            // Handle tiebreakers and determine final winners.
            if (winners.Count > 1)
            {
                // If still poker hands tied, apply second tiebreaker by kickers.
                winners = new Tiebreaker().PokerKickersTiebreaker(winners);

                // If still tied, SPLIT POT.
                if (winners.Count > 1)
                {
                    tiebreakerKickerHighCard = true;
                }
                // Found winner, won by kickers
                else if (winners.Count == 1)
                {
                    tiebreakerKickerHighCard = true;
                    List<PlayerBase> winnerPlayers = new List<PlayerBase> { winners[0] };
                    PokerEventManager.OnRoundFinished?.Invoke(winnerPlayers);
                    return winnerPlayers;
                }
            }
            // Found winner, won by poker hand
            else if (winners.Count == 1)
            {
                List<PlayerBase> winnerPlayers = new List<PlayerBase> { winners[0] };
                PokerEventManager.OnRoundFinished?.Invoke(winnerPlayers);
                return winnerPlayers;
            }
            else
            {
                Debug.LogWarning("No winners found");
            }
            PokerEventManager.OnRoundFinished?.Invoke(winners);
            return winners;
        }

        /// <summary>
        /// Initiates a new round of the game.
        /// </summary>
        private IEnumerator NewRoundCoroutine()
        {
            ResetRound();
            RemoveLoserPlayers();

            // If the game is not over, initiates a new round of the game.
            if (!IsGameOver())
            {
                dealerTurn++;
                PokerEventManager.OnNewRoundStarted?.Invoke();
                yield return new WaitForSeconds(asset.Time_Delay);
                yield return StartCoroutine(DistributeCardsToPlayersCoroutine());
                yield return new WaitForSeconds(asset.Time_Delay);
                yield return StartCoroutine(OrganizeFirstTurnCoroutine());
            }
            else
            {
                // If the game is over, handle the winner and reward chips.
                PlayerBase winnerPlayer = PlayerActive();
                if (winnerPlayer.IsPlayer)
                {
                    int playerChips = DatabaseManager.LoadPlayerChips() + pokerLevelData.LevelData.Reward;
                    DatabaseManager.SavePlayerChips(playerChips);
                }
                PokerEventManager.OnMatchEnd?.Invoke(winnerPlayer);
            }
        }

        /// <summary>
        /// Resets the game state for a new round.
        /// </summary>
        private void ResetRound()
        {
            tiebreakerKickerHighCard = false;
            cardsInTable.Clear();
            foreach (PlayerBase player in playersInGame)
            {
                player.ClearHand();
                player.ResetActions();
                player.ResetSetup();
            }

            Deck.ResetDeck();
            Deck.ShuffleDeck();
            Turn = TurnPoker.Ante;
            PokerPotBet = new PokerPotBet();
        }

        /// <summary>
        /// Removes players with zero chips from the game.
        /// </summary>
        private void RemoveLoserPlayers()
        {
            foreach (PlayerBase player in playersInGame)
            {
                if (player.Chips <= 0)
                {
                    player.gameObject.SetActive(false);
                }
            }
            playersInGame = playersInGame.Where(player => player.Chips > 0).ToList();
        }

        /// <summary>
        /// Checks if the game is over by determining if only one player remains active 
        /// or if all non-player players are eliminated.
        /// </summary>
        private bool IsGameOver()
        {
            int count = 0;
            foreach (PlayerBase player in playersInGame)
            {
                if (player.gameObject.activeInHierarchy)
                    count++;
            }

            bool playerEliminated = true;
            foreach (PlayerBase player in playersInGame)
            {
                if (player.IsPlayer)
                {
                    playerEliminated = false;
                    break;
                }
            }

            return count == 1 || playerEliminated;
        }

        /// <summary>
        /// Finds and returns the active player in game.
        /// </summary>
        private PlayerBase PlayerActive()
        {
            foreach (PlayerBase player in playersInGame)
            {
                if (player.gameObject.activeInHierarchy)
                    return player;
            }
            return null;
        }

        /// <summary>
        /// Adds chips to the pot made by one of the players.
        /// </summary>
        /// <param name="player">The player who added the chips.</param>
        /// <param name="chips">The number of chips to add to the pot.</param>
        public void AddChipsToPot(PlayerBase player, int chips)
        {
            PokerPotBet.AddBet(Turn, player, chips);
        }

        /// <summary>
        /// Defines the player who will be responsible for finishing the round of the next poker turn.
        /// </summary>
        /// <param name="player">The player who finished the next turn.</param>
        public void SetPlayerEndTurn(PlayerBase player)
        {
            endPlayerTurn = player;
        }

        /// <summary>
        /// Handles a player's folding action.
        /// </summary>
        /// <param name="player">The player who folds.</param>
        public void PlayerFold(PlayerBase player)
        {
            int playersActiveInTable = 0;
            PlayerBase winnerPlayer = null;
            foreach (PlayerBase p in playersInGame)
            {
                if (!p.ContinueFoldNextTurn)
                {
                    winnerPlayer = p;
                    playersActiveInTable++;
                }
            }

            // If there is only one player active, force victory.
            if (playersActiveInTable == 1 && winnerPlayer != null)
            {
                StartCoroutine(ForceVictory(winnerPlayer));
            }
            else
            {
                // Move to the next player's turn if the current player folded.
                if (player == endPlayerTurn)
                {
                    endPlayerTurn = playersInGame[playerTurnIndex + 1];
                }
            }
        }

        /// <summary>
        /// Handles a player going all-in.
        /// </summary>
        /// <param name="player">The player going all-in.</param>
        public void PlayerAllIn(PlayerBase player)
        {
            PokerStats = new PokerStats(player, 1, player.CurrentBet);
        }


        /// <summary>
        /// Forces a victory for a player, ending the round.
        /// </summary>
        /// <param name="player">The player who will be forced to win the round.</param>
        private IEnumerator ForceVictory(PlayerBase player)
        {
            PokerEventManager.OnRoundFinished?.Invoke(new List<PlayerBase> { player });
            WinnersGetPotChips(new List<PlayerBase> { player });
            yield return new WaitForSeconds(asset.Time_WaitNextRound);
            StartCoroutine(NewRoundCoroutine());
        }

        /// <summary>
        /// Checks if there's a bet in the current round.
        /// </summary>
        public bool HasBetInTable()
        {
            bool hasBet = true;
            for (int i = 0; i < playersInGame.Count; i++)
            {
                if (!playersInGame[i].IsRaise)
                {
                    hasBet = false;
                    break;
                }
            }

            return hasBet;
        }

        /// <summary>
        /// Checks if there's only one player who hasn't gone all-in yet.
        /// </summary>
        public bool IsJustOneNotAllIn()
        {
            int playersNotInAllIn = 0;
            PlayerBase player = null;

            List<PlayerBase> playersToCheck = new List<PlayerBase>();
            // Select players who haven't folded.
            for (int i = 0; i < playersInGame.Count; i++)
            {
                if (!playersInGame[i].ContinueFoldNextTurn)
                    playersToCheck.Add(playersInGame[i]);
            }

            // Count players who haven't gone all-in yet.
            for (int i = 0; i < playersToCheck.Count; i++)
            {
                if (!playersToCheck[i].ContinueAllInNextTurn)
                {
                    player = playersToCheck[i];
                    playersNotInAllIn++;
                }
            }

            // If there is only one player who has not gone all in and there are at least two active players, add the all in flag to end the round.
            if (player != null && playersNotInAllIn == 1 && playersToCheck.Count >= 2)
                player.ContinueAllInNextTurn = true;

            return playersNotInAllIn == 1 && playersToCheck.Count >= 2;
        }
    }
}