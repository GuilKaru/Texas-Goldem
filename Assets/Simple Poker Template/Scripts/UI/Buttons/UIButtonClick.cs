using SimplePoker.Audio;
using SimplePoker.Data;
using SimplePoker.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePoker.UI
{
    public class UIButtonClick : MonoBehaviour
    {
        private PokerGameAssetData asset;
        private Button button;
        private void Start()
        {
            asset = PokerGameAsset.Instance.PokerGameAssetData;
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);
        }
        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            SoundManager.Instance?.PlayOneShotSong(asset.Audio_ButtonClick);
        }
    }
}