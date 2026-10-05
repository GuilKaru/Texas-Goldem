using TMPro;
using UnityEngine;

namespace SimplePoker.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewGameAssetData", menuName = "Poker Data/Game Asset Base Data", order = 1)]
    public class PokerGameAssetData : ScriptableObject
    {

        public Sprite Sprite_Table;
        /*public Sprite Sprite_NormalFramePortrait;
        public Sprite Sprite_MyTurnFramePortrait;
        public Sprite Sprite_MyTurnHighlightRotate;*/
        public Sprite Sprite_WinnerRoundCrown;
        public Sprite Sprite_BackgroundPlayerName;
        public Sprite Sprite_BackgroundPlayerChips;
        public Sprite Sprite_BackgroundPlayerHandType;
        public Sprite Sprite_BackgroundPlayerActionChoose;
        public Sprite Sprite_BackgroundBetChip;
        public Sprite Sprite_BetChip;
        public Sprite Sprite_DealerToken;
        public Sprite Sprite_SmallBlindToken;
        public Sprite Sprite_BigBlindToken;
        public Sprite Sprite_BackgroundWinner;
        public Sprite Sprite_WinnerHighlightRotate;
        public Sprite Sprite_BackgroundPotChips;
        public Sprite Sprite_PotChips;

        public Color Color_BackgroundChooseFold = Color.black;
        public Color Color_BackgroundChooseCallCheck = Color.yellow;
        public Color Color_BackgroundChooseRaise = Color.green;
        public Color Color_BackgroundChooseAllIn = Color.green;
        public Color Color_MyTurnHaloRotate = Color.green;
        public Color Color_WinnerHaloRotate = Color.yellow;
        public Color Color_HighlightPokerHandWinner = Color.red;

        [field: Header("Game Font")]
        //public TMP_FontAsset DefaultFont;


        [field: Header("Game Flow Times")]
        [Space(10)]
        public readonly float Time_Delay = 0.1f;
        [Range(0.5f, 5)] public float Time_WaitToBeginGame = 2f;
        [Range(0.01f, 1)] public float Time_EnablePlayerOnTable = 0.1f;
        [Range(0.01f, 5)] public float Time_WaitToBeginTurn = 1f;
        [Range(0.01f, 1)] public float Time_DistributeCard = 0.1f;
        [Range(0.01f, 1)] public float Time_NextTurnDelay = 1f;
        [Range(0.01f, 9)] public float Time_WaitNextRound = 5f;
        [Range(0.01f, 3)] public float Time_CpuWaitChooseAction = 075f;
        [Range(0.01f, 3)] public float Time_CallNextPlayer = 075f;
        [Range(0.01f, 3)] public float Time_UpdatePotValueText = 0.3f;
        public readonly float Time_CallNextPlayerOnAllIn = 0.01f;

        [field: Header("Audios")]
        [Space(11)]
        public AudioClip Audio_1Card;
        public AudioClip Audio_2Cards;
        public AudioClip Audio_3Cards;
        public AudioClip Audio_BetChips;
        public AudioClip Audio_DoFold;
        public AudioClip Audio_DoCheckCall;
        public AudioClip Audio_DoCall;
        public AudioClip Audio_DoRaise;
        public AudioClip Audio_DoAllIn;
        public AudioClip Audio_PlayerWon;
        public AudioClip Audio_PlayerLost;
        public AudioClip Audio_RoundCardsShow;
        public AudioClip Audio_ButtonClick;
    }
}