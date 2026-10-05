using DG.Tweening;
using SimplePoker.Data;
using SimplePoker.Logic;
using SimplePoker.ScriptableObjects;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePoker.Visual
{
    /// <summary>
    /// Represents the UI elements and functionality for poker pot visual.
    /// </summary>
    public class UIPokerPot : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject potCollectingPrefab;
        [SerializeField] private GameObject posCanvas;

        [Header("UI Images")]
        [SerializeField] private Image backgroundPotImage;
        [SerializeField] private Image potImage;

        [Header("UI Texts")]
        [SerializeField] private TextMeshProUGUI potText;

        private PokerGameAssetData asset;

        private void Awake()
        {
            PokerEventManager.OnPotAmountChanged += PokerEventManager_UpdatePotText;
            PokerEventManager.OnRoundFinished += PokerEventManager_MovePotToPlayer;
            PokerEventManager.OnNewRoundStarted += PokerEventManager_OnNewRoundStarted;
        }

        private void OnDisable()
        {
            PokerEventManager.OnPotAmountChanged -= PokerEventManager_UpdatePotText;
            PokerEventManager.OnRoundFinished -= PokerEventManager_MovePotToPlayer;
            PokerEventManager.OnNewRoundStarted -= PokerEventManager_OnNewRoundStarted;

        }

        private void Start()
        {
            asset = PokerGameAsset.Instance.PokerGameAssetData;
            backgroundPotImage.sprite = asset.Sprite_BackgroundPotChips;
            potImage.sprite = asset.Sprite_PotChips;
           // potText.font = asset.DefaultFont;
        }

        /// <summary>
        /// Updates the pot text with a smooth transition.
        /// </summary>
        /// <param name="beforeChips">The value of the pot before the update.</param>
        /// <param name="afterChips">The value of the pot after the update.</param>
        private void PokerEventManager_UpdatePotText(int beforeChips, int afterChips)
        {
            float duration = asset.Time_UpdatePotValueText;
            DOTween.To(() => beforeChips, value => UpdatePotText(potText, value), afterChips, duration);
        }

        /// <summary>
        /// Updates the pot value text mesh.
        /// </summary>
        /// <param name="textMesh">The TextMeshProUGUI component to update.</param>
        /// <param name="value">The value to be displayed.</param>
        private void UpdatePotText(TextMeshProUGUI textMesh, int value)
        {
            textMesh.SetText($"POT: ${value}");
        }

        /// <summary>
        /// Moves the pot chips to the winning player(s) with an animation.
        /// </summary>
        /// <param name="players">The list of players who won the pot.</param>
        private void PokerEventManager_MovePotToPlayer(List<PlayerBase> players)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(1f);

            sequence.AppendInterval(0.1f);
            Transform[] coins = new Transform[4];

            foreach (PlayerBase player in players)
            {

                for (int i = 0; i < coins.Length; i++)
                {
                    float duration = Random.Range(1f, 1.3f);

                    Vector3 initialPosition = new Vector3(transform.position.x + Random.Range(-0.5f, 0.5f),
                        transform.position.y + Random.Range(-0.5f, 0.5f),
                        transform.position.z);
                    GameObject potCollecting = Instantiate(potCollectingPrefab, initialPosition, Quaternion.identity);
                    coins[i] = potCollecting.transform;

                    potCollecting.transform.SetParent(transform);

                    sequence.Join(potCollecting.transform.DOJump(player.transform.position, 2, 1, duration).AppendCallback(() =>
                    {
                        potCollecting.gameObject.SetActive(false);
                    }));
                }
            }
        }

        /// <summary>
        /// Resets the pot text to display $0 at the start of a new round.
        /// </summary>
        private void PokerEventManager_OnNewRoundStarted()
        {
            potText.SetText($"POT: $0");
        }
    }
}