using SimplePoker.ScriptableObjects;
using UnityEngine;

namespace SimplePoker.Data
{
    /// <summary>
    /// Singleton class for accessing the assets used in the poker game.
    /// </summary>
    public class PokerGameAsset : Singleton<PokerGameAsset>
    {
        [field: SerializeField] public PokerGameAssetData PokerGameAssetData { get; private set; }
    }
}