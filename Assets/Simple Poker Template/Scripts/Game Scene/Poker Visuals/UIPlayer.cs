using DG.Tweening;
using SimplePoker.Audio;
using SimplePoker.Data;
using SimplePoker.Logic;
using SimplePoker.SaveLoad;
using SimplePoker.ScriptableObjects;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePoker.Visual
{
    /// <summary>
    /// Represents the UI elements and functionality for a player in the poker game.
    /// </summary>
    public class UIPlayer : MonoBehaviour
    {
        private PokerGameAssetData asset;

        [SerializeField] private PlayerBase player;
        [SerializeField] private Hand handVisual;

        [Header("UI Images")]
        [SerializeField] private Image characterPortrait;
        [SerializeField] private Image turnFrameImage;
        [SerializeField] private Image frameMaskImage;
        [SerializeField] private Image myTurnHaloRotateImage;
        [SerializeField] private Image winnerCrownImage;
        [SerializeField] private Image backgroundNameImage;
        [SerializeField] private Image backgroundChipsImage;
        [SerializeField] private Image backgroundHandTypeImage;
        [SerializeField] private Image backgroundActionChooseImage;
        [SerializeField] private Image backgroundBetChipsImage;
        [SerializeField] private Image dealerImage;
        [SerializeField] private Image smallBlindImage;
        [SerializeField] private Image bigBlindImage;
        [SerializeField] private Image backgroundWinnerImage;
        [SerializeField] private Image winnerHaloRotateImage;


        [Header("UI Gameobjects")]
        [SerializeField] private GameObject crown;
        [SerializeField] private GameObject myTurnHighlightEffect;
        [SerializeField] private GameObject winnerEffect;

        [SerializeField] private GameObject dealerChip;
        [SerializeField] private GameObject smallBlindChip;
        [SerializeField] private GameObject bigBlindChip;
        [SerializeField] private GameObject betMoney;
        [SerializeField] private Vector3 betMoneyPositionOffset;



        [Header("UI Text")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI chipsText;
        [SerializeField] private TextMeshProUGUI pokerHandText;
        [SerializeField] private TextMeshProUGUI actionChooseText;
        [SerializeField] private TextMeshProUGUI betChipsText;

        private SoundManager soundManager;

        /// <summary>
        /// Initializes the component's references and subscribes to player events.
        /// </summary>
        private void Awake()
        {
            asset = PokerGameAsset.Instance.PokerGameAssetData;
            player = GetComponent<PlayerBase>();
            player.OnChipsAmountChanged += Player_HandleUpdateChipsText;
            player.OnHandUpdated += Player_HandleUpdateHandText;
            player.OnBetMaked += Player_HandleBetMake;
            player.OnActionChoosed += Player_HandleActionChoosed;
            player.OnPokerChipSeted += Player_HandlePokerChip;
            player.OnCardAddedToHand += Player_HandleCardAdded;
            player.OnPlayerActived += Player_HandlePlayerActived;

            PokerEventManager.OnPlayerTurnChanged += PokerEventManager_HandlePlayerTurnChange;
            PokerEventManager.OnTurnChanged += PokerEventManager_HandleTurnChanged;
            PokerEventManager.OnNewRoundStarted += PokerEventManager_HandleNewRoundStarted;
            PokerEventManager.OnRoundFinished += PokerEventManager_HandleRoundFinished;
            PokerEventManager.OnMatchEnd += PokerEventManager_HandleMatchEnd;

            betMoney.transform.position = betMoney.transform.position + betMoneyPositionOffset;
        }

        /// <summary>
        /// Unsubscribes from player events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            player.OnChipsAmountChanged -= Player_HandleUpdateChipsText;
            player.OnHandUpdated -= Player_HandleUpdateHandText;
            player.OnBetMaked -= Player_HandleBetMake;
            player.OnActionChoosed -= Player_HandleActionChoosed;
            player.OnPokerChipSeted -= Player_HandlePokerChip;
            player.OnCardAddedToHand -= Player_HandleCardAdded;
            player.OnPlayerActived -= Player_HandlePlayerActived;

            PokerEventManager.OnPlayerTurnChanged -= PokerEventManager_HandlePlayerTurnChange;
            PokerEventManager.OnTurnChanged -= PokerEventManager_HandleTurnChanged;
            PokerEventManager.OnNewRoundStarted -= PokerEventManager_HandleNewRoundStarted;
            PokerEventManager.OnRoundFinished -= PokerEventManager_HandleRoundFinished;
            PokerEventManager.OnMatchEnd -= PokerEventManager_HandleMatchEnd;
        }

        /// <summary>
        /// Initializes the component's references and settings.
        /// </summary>
        private void Start()
        {
            soundManager = SoundManager.Instance;

            /*turnFrameImage.sprite = asset.Sprite_NormalFramePortrait;
            frameMaskImage.sprite = asset.Sprite_NormalFramePortrait;
            myTurnHaloRotateImage.sprite = asset.Sprite_MyTurnHighlightRotate;*/
            myTurnHaloRotateImage.color = asset.Color_MyTurnHaloRotate;
            winnerCrownImage.sprite = asset.Sprite_WinnerRoundCrown;
            backgroundNameImage.sprite = asset.Sprite_BackgroundPlayerName;
            backgroundChipsImage.sprite = asset.Sprite_BackgroundPlayerChips;
            backgroundHandTypeImage.sprite = asset.Sprite_BackgroundPlayerHandType;
            backgroundActionChooseImage.sprite = asset.Sprite_BackgroundPlayerActionChoose;
            backgroundBetChipsImage.sprite = asset.Sprite_BackgroundBetChip;
            dealerImage.sprite = asset.Sprite_DealerToken;
            smallBlindImage.sprite = asset.Sprite_SmallBlindToken;
            bigBlindImage.sprite = asset.Sprite_BigBlindToken;
            backgroundWinnerImage.sprite = asset.Sprite_BackgroundWinner;
            winnerHaloRotateImage.sprite = asset.Sprite_WinnerHighlightRotate;
            winnerHaloRotateImage.color = asset.Color_WinnerHaloRotate;

            /*nameText.font = asset.DefaultFont;
            chipsText.font = asset.DefaultFont;
            pokerHandText.font = asset.DefaultFont;
            actionChooseText.font = asset.DefaultFont;
            betChipsText.font = asset.DefaultFont;*/
        }

        /// <summary>
        /// Activates the background text with animation scale.
        /// </summary>
        /// <param name="text">The text to activate the background.</param>
        private void ActiveBackgroundText(TextMeshProUGUI text)
        {
            Transform parentTransform = text.transform.parent;
            if (!parentTransform.gameObject.activeInHierarchy)
            {
                parentTransform.gameObject.SetActive(true);
                parentTransform.localScale = Vector3.zero;
                parentTransform.DOScale(Vector3.one, 0.05f);
            }
        }

        /// <summary>
        /// Deactivates the background text by scaling it from one to zero.
        /// </summary>
        /// <param name="text">The text to deactivate the background.</param>
        private void DisableBackgroundText(TextMeshProUGUI text)
        {
            Transform parentTransform = text.transform.parent;

            parentTransform.DOScale(Vector3.zero, 0.1f).OnComplete(() =>
            {
                parentTransform.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// Handles the player's chosen action by updating the UI.
        /// </summary>
        /// <param name="action">The action chosen by the player.</param>
        private void Player_HandleActionChoosed(PokerAction action)
        {
            ActiveBackgroundText(actionChooseText);
            Image actionChooseImage = actionChooseText.GetComponentInParent<Image>();
            switch (action)
            {
                case PokerAction.NOTHING:
                    break;
                case PokerAction.FOLD:
                    actionChooseText.SetText("FOLD");
                    actionChooseImage.color = asset.Color_BackgroundChooseFold;
                    soundManager?.PlayOneShotSong(asset.Audio_DoFold);
                    break;
                case PokerAction.CHECK:
                    actionChooseText.SetText("CHECK");
                    actionChooseImage.color = asset.Color_BackgroundChooseCallCheck;
                    soundManager?.PlayOneShotSong(asset.Audio_DoCheckCall);
                    break;
                case PokerAction.CALL:
                    actionChooseText.SetText("CALL");
                    actionChooseImage.color = asset.Color_BackgroundChooseCallCheck;
                    soundManager?.PlayOneShotSong(asset.Audio_BetChips);
                    break;
                case PokerAction.RAISE:
                    actionChooseText.SetText("RAISE");
                    actionChooseImage.color = asset.Color_BackgroundChooseRaise;
                    soundManager.PlayOneShotSong(asset.Audio_BetChips);
                    break;
                case PokerAction.ALL_IN:
                    actionChooseText.SetText("ALL IN");
                    actionChooseImage.color = asset.Color_BackgroundChooseAllIn;
                    soundManager?.PlayOneShotSong(asset.Audio_BetChips);
                    break;
            }
        }

        /// <summary>
        /// Handles the update of the player's chips amount by animating the text.
        /// </summary>
        /// <param name="beforeChips">The chips amount before the update.</param>
        /// <param name="afterChips">The chips amount after the update.</param>
        private void Player_HandleUpdateChipsText(int beforeChips, int afterChips)
        {
            DOTweenTextValueUpdate(beforeChips, afterChips, chipsText);
        }

        /// <summary>
        /// Handles the update of the player's hand by updating the UI text.
        /// </summary>
        /// <param name="pokerHand">The player's poker hand.</param>
        private void Player_HandleUpdateHandText(PokerHand pokerHand)
        {
            pokerHandText.SetText(pokerHand.Hand.ToString());
        }

        /// <summary>
        /// Animates the update of a TextMeshProUGUI component using DOTween.
        /// </summary>
        /// <param name="beforeValue">The value before the update.</param>
        /// <param name="afterValue">The value after the update.</param>
        /// <param name="text">The TextMeshProUGUI component to update.</param>
        private void DOTweenTextValueUpdate(int beforeValue, int afterValue, TextMeshProUGUI text)
        {
            float duration = asset.Time_UpdatePotValueText;
            DOTween.To(() => beforeValue, value => UpdateText(text, value), afterValue, duration);
        }

        /// <summary>
        /// Updates the bet chips text UI element.
        /// </summary>
        /// <param name="chips">The chips amount to display.</param>
        private void UpdateText(TextMeshProUGUI textMesh, int value)
        {
            textMesh.SetText($"${value}");
        }

        /// <summary>
        /// Updates the active poker chip UI element.
        /// </summary>
        /// <param name="pokerChip">The type of poker chip to display.</param>
        private void Player_HandleBetMake(int chips)
        {
            ActiveBackgroundText(betChipsText);
            betChipsText.SetText($"${chips}");
        }

        /// <summary>
        /// Handles the addition of a card to the player's hand by updating the UI.
        /// </summary>
        /// <param name="newCard">The card added to the hand.</param>
        /// <param name="cards">The list of cards in the hand.</param>
        private void Player_HandlePokerChip(PokerChip pokerChip)
        {
            dealerChip.SetActive(false);
            smallBlindChip.SetActive(false);
            bigBlindChip.SetActive(false);

            switch (pokerChip)
            {
                case PokerChip.DEALER:
                    dealerChip.SetActive(true);
                    break;
                case PokerChip.SMALL_BLIND:
                    smallBlindChip.SetActive(true);
                    break;
                case PokerChip.BIG_BLIND:
                    bigBlindChip.SetActive(true);
                    break;
            }
        }

        /// <summary>
        /// Handles the activation of the player by updating the UI elements.
        /// </summary>
        private void Player_HandleCardAdded(Card newCard, List<Card> cards)
        {
            soundManager?.PlayOneShotSong(asset.Audio_3Cards);
            newCard.transform.SetParent(handVisual.transform);
            newCard.ShowCard = player.IsPlayer;
            handVisual.OrganizeHand(cards);
        }

        /// <summary>
        /// Handles the change of player turn by updating the UI elements.
        /// </summary>
        /// <param name="newPlayer">The player whose turn it is.</param>
        /// <param name="turnPoker">The current poker turn.</param>
        private void Player_HandlePlayerActived()
        {
            if (player.IsPlayer)
            {
                PlayerData playerData = GetComponent<Human>().PlayerData;
                Sprite playerPortrait = playerData.Portraits[DatabaseManager.LoadPlayerPortrait()];
                characterPortrait.sprite = playerPortrait;
                nameText.SetText(playerData.Name);
                gameObject.name = playerData.Name;
            }
            else
            {
                CpuData cpuData = GetComponent<Cpu>().CpuData;
                characterPortrait.sprite = cpuData.Portrait;
                nameText.SetText(cpuData.Name);
                gameObject.name = cpuData.Name;
            }
        }

        /// <summary>
        /// Handles the change of player turn by updating the UI elements.
        /// </summary>
        /// <param name="newPlayer">The player whose turn it is.</param>
        /// <param name="turnPoker">The current poker turn.</param>
        private void PokerEventManager_HandlePlayerTurnChange(PlayerBase newPlayer, GameManager.TurnPoker turnPoker)
        {
            SetMyTurnFrameVisual(isMyTurn: false);
            if (newPlayer == player && !player.ContinueAllInNextTurn && !player.ContinueFoldNextTurn)
            {
                DisableBackgroundText(actionChooseText);
            }


            if (player.Chips <= 0)
            {
                ActiveBackgroundText(actionChooseText);
                actionChooseText.SetText("ALL IN");
            }
            else if (player.IsFold)
            {
                ActiveBackgroundText(actionChooseText);
                actionChooseText.SetText("FOLD");
            }
            else
            {
                bool isPlayerTurn = player == newPlayer;
                if (isPlayerTurn && !player.ContinueAllInNextTurn && !player.ContinueFoldNextTurn)
                    SetMyTurnFrameVisual(isMyTurn: true);
            }
        }

        /// <summary>
        /// Handles the event when the turn changes by updating the UI elements.
        /// </summary>
        private void PokerEventManager_HandleTurnChanged()
        {
            SetMyTurnFrameVisual(isMyTurn: false);
            DisableBackgroundText(betChipsText);

            if (!player.ContinueAllInNextTurn && !player.ContinueFoldNextTurn)
                DisableBackgroundText(actionChooseText);
        }

        /// <summary>
        /// Handles the event when a new round starts by updating the UI elements.
        /// </summary>
        private void PokerEventManager_HandleNewRoundStarted()
        {
            dealerChip.SetActive(false);
            smallBlindChip.SetActive(false);
            bigBlindChip.SetActive(false);
            crown.SetActive(false);
            winnerEffect.SetActive(false);
            DisableBackgroundText(actionChooseText);
            DisableBackgroundText(betChipsText);


            if (player.IsPlayer)
                ActiveBackgroundText(pokerHandText);
            else
                DisableBackgroundText(pokerHandText);
        }

        /// <summary>
        /// Handles the event when a round finishes by updating the UI elements.
        /// </summary>
        /// <param name="winners">The list of winners in the round.</param>
        private void PokerEventManager_HandleRoundFinished(List<PlayerBase> winners)
        {
            SetMyTurnFrameVisual(isMyTurn: false);
            if (player.ContinueFoldNextTurn)
                return;

            ActiveBackgroundText(pokerHandText);
            DisableBackgroundText(actionChooseText);

            // Show cards
            Sequence sequence = DOTween.Sequence();

            foreach (Card card in player.Cards)
            {
                sequence.Join(card.transform.DOLocalRotate(new Vector3(0, 180, 0), 0.1f, RotateMode.FastBeyond360));
                sequence.AppendCallback(() =>
                {
                    card.ShowCard = true;
                });
                sequence.Join(card.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.1f));
            }

            // Show crown
            sequence = DOTween.Sequence();
            if (winners.Contains(player)) // Winner
            {
                backgroundHandTypeImage.color = Color.yellow;

                crown.SetActive(true);
                crown.transform.localScale = Vector3.zero;

                winnerEffect.gameObject.SetActive(true);
                winnerEffect.transform.localScale = Vector3.zero;

                sequence.AppendInterval(0.5f);
                sequence.Append(crown.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBounce));
                sequence.Join(winnerEffect.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce));
            }
            else // Loser
            {
                backgroundHandTypeImage.color = Color.black;
            }
        }

        /// <summary>
        /// Handles the event when the match ends by updating the UI elements.
        /// </summary>
        /// <param name="winner">The winner of the match.</param>
        private void PokerEventManager_HandleMatchEnd(PlayerBase winner)
        {
            dealerChip.SetActive(false);
            smallBlindChip.SetActive(false);
            bigBlindChip.SetActive(false);
            crown.SetActive(false);
            winnerEffect.SetActive(false);
            DisableBackgroundText(actionChooseText);
            DisableBackgroundText(betChipsText);
            DisableBackgroundText(pokerHandText);
        }

        /// <summary>
        /// Sets the visual representation for the current player's turn.
        /// </summary>
        /// <param name="isMyTurn">Boolean indicating whether it's the current player's turn.</param>
        private void SetMyTurnFrameVisual(bool isMyTurn)
        {
            /*if (isMyTurn)
                turnFrameImage.sprite = asset.Sprite_MyTurnFramePortrait;
            else
                turnFrameImage.sprite = asset.Sprite_NormalFramePortrait;*/
            myTurnHighlightEffect.SetActive(isMyTurn);
        }
    }
}