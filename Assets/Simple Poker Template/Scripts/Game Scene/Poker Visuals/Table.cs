using SimplePoker.Data;
using SimplePoker.ScriptableObjects;
using UnityEngine;

namespace SimplePoker.Visual
{
    /// <summary>
    /// Represents the Sprite element for the table poker art.
    /// </summary>
    public class Table : MonoBehaviour
    {
        private PokerGameAssetData asset;
        [SerializeField] private SpriteRenderer tableSpriteRendenrer;

        private void Start()
        {
            asset = PokerGameAsset.Instance.PokerGameAssetData;
            tableSpriteRendenrer.sprite = asset.Sprite_Table;
        }
    }
}