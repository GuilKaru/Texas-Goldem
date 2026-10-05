using SimplePoker.Attribute;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePoker.Audio
{
    public class AudioToggle : MonoBehaviour
    {
        private enum TypeAudio { MUSIC, SOUND_EFFECT }
        [SerializeField] private TypeAudio typeAudio;
        [SerializeField] private Toggle toggle;
        [SerializeField, ReadOnly] private bool isMute;

        [SerializeField, ReadOnly] private int valueKey;
        [SerializeField, ReadOnly] private string keyValuePlayerPrefs;
        [SerializeField, ReadOnly] private string keyVolumePlayerPrefs;

        private void OnEnable()
        {
            Setup();
        }

        private void Awake()
        {
            toggle.onValueChanged.AddListener(delegate { OnTogglePreferenceChanged(); });
        }

        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(delegate { OnTogglePreferenceChanged(); });
        }

        private void Setup()
        {
            keyVolumePlayerPrefs = GetKeyVolume();
            keyValuePlayerPrefs = GetKeyValue();
            int value = PlayerPrefs.GetInt(keyValuePlayerPrefs, 0);

            if (value == 0)
                toggle.isOn = true;
            else
                toggle.isOn = false;

            valueKey = value;

            OnTogglePreferenceChanged();
        }

        private void OnTogglePreferenceChanged()
        {
            isMute = !toggle.isOn;
            int volume = 0;
            if (isMute)
            {
                PlayerPrefs.SetInt(keyValuePlayerPrefs, 1);
                valueKey = 1;
                volume = -80;
            }
            else
            {
                PlayerPrefs.SetInt(keyValuePlayerPrefs, 0);
                valueKey = 0;
                volume = 0;
            }

            PlayerPrefs.SetFloat(keyVolumePlayerPrefs, volume);
            switch (typeAudio)
            {
                case TypeAudio.MUSIC:
                    SoundManager.Instance.SetVolume(volume, SoundManager.AUDIO_OUTPUT.MUSIC);
                    break;
                case TypeAudio.SOUND_EFFECT:
                    SoundManager.Instance.SetVolume(volume, SoundManager.AUDIO_OUTPUT.EFFECT);
                    break;
            }
        }


        public string GetKeyValue()
        {
            switch (typeAudio)
            {
                case TypeAudio.MUSIC:
                    return "music_configuration_toggle";
                case TypeAudio.SOUND_EFFECT:
                    return "effect_configuration_toggle";

            }
            return "music_configuration";
        }

        public string GetKeyVolume()
        {
            switch (typeAudio)
            {
                case TypeAudio.MUSIC:
                    return "music_configuration";
                case TypeAudio.SOUND_EFFECT:
                    return "effect_configuration";

            }
            return "music_configuration";
        }
    }
}