using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;
using SimplePoker.Attribute;

namespace SimplePoker.Audio
{
    /// <summary>
    /// Singleton class for managing game sounds, including music and effects.
    /// </summary>
    public class SoundManager : Singleton<SoundManager>
    {
        #region VARIABLES
        private const float MIN_PITCH = 0.9f;
        private const float MAX_PITCH = 1.1f;
        private const float NORMAL_PITCH = 1f;

        [ReadOnly] public string ID;
        [SerializeField] private AudioMixer audioMixer;

        [SerializeField, ReadOnly] private AudioSource musicAudioSource;
        [SerializeField, ReadOnly] private AudioSource effectAudioSource;
        [SerializeField, ReadOnly] private AudioSource[] effectAudioSourceArray;
        private Sequence sequence;
        public enum AUDIO_OUTPUT
        {
            MUSIC,
            EFFECT
        };

        private enum PLAY_SONG
        {
            ONESHOT,
            PLAY
        }
        #endregion

        #region UNITY_FUNCTIONS
        protected override void Awake()
        {
            base.Awake();
            musicAudioSource = transform.Find("Music AudioSource").GetComponent<AudioSource>();
            effectAudioSource = transform.Find("Effect AudioSource").GetComponent<AudioSource>();
            effectAudioSourceArray = transform.Find("Group AudioSource").GetComponentsInChildren<AudioSource>();
        }
        private void Start()
        {
            UpdateAudioVolume();
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public void SetVolume(float volume, AUDIO_OUTPUT output)
        {
            switch (output)
            {
                case AUDIO_OUTPUT.MUSIC:
                    audioMixer.SetFloat("Music", volume);
                    break;
                case AUDIO_OUTPUT.EFFECT:
                    audioMixer.SetFloat("SoundEffect", volume);
                    break;
            }
        }
        public void PlayMusic(AudioClip audioClip, AUDIO_OUTPUT output = AUDIO_OUTPUT.MUSIC, float pitch = NORMAL_PITCH, bool loop = true)
        {
            PlayAudioClip(audioClip, output, PLAY_SONG.PLAY, pitch, loop);
        }
        public void PlayOneShotSong(AudioClip audioClip, AUDIO_OUTPUT output = AUDIO_OUTPUT.EFFECT, float pitch = NORMAL_PITCH, bool loop = false)
        {
            PlayAudioClip(audioClip, output, PLAY_SONG.ONESHOT, pitch, loop);
        }
        public void PlayOneShotSong_RandomPitch(AudioClip audioClip, AUDIO_OUTPUT output = AUDIO_OUTPUT.EFFECT, float minPitch = MIN_PITCH, float maxPitch = MAX_PITCH, bool loop = false)
        {
            float pitch = Random.Range(minPitch, maxPitch);
            PlayAudioClip(audioClip, output, PLAY_SONG.ONESHOT, pitch, loop);
        }
        public void PlayEffectSong_GroupAudioSource(AudioClip audioClip, float pitch = NORMAL_PITCH, bool loop = false)
        {
            PlayEffectSong(audioClip, pitch, loop);
        }
        public void PlayEffectSong_GroupAudioSource_RandomPitch(AudioClip audioClip, float minPitch = MIN_PITCH, float maxPitch = MAX_PITCH, bool loop = false)
        {
            float pitch = Random.Range(minPitch, maxPitch);
            PlayEffectSong(audioClip, pitch, loop);
        }

        public void Mute(bool value, AUDIO_OUTPUT output)
        {
            switch (output)
            {
                case AUDIO_OUTPUT.MUSIC:
                    musicAudioSource.mute = value;
                    break;
                case AUDIO_OUTPUT.EFFECT:
                    effectAudioSource.mute = value;
                    foreach (var audioSource in effectAudioSourceArray)
                        audioSource.mute = value;
                    break;
            }
        }
        public void MuteAll(bool value)
        {
            musicAudioSource.mute = value;
            effectAudioSource.mute = value;
            foreach (var audioSource in effectAudioSourceArray)
                audioSource.mute = value;
        }
        public void Stop(AUDIO_OUTPUT output)
        {
            switch (output)
            {
                case AUDIO_OUTPUT.MUSIC:
                    musicAudioSource.Stop();
                    break;
                case AUDIO_OUTPUT.EFFECT:
                    effectAudioSource.Stop();
                    foreach (var audioSource in effectAudioSourceArray)
                        audioSource.Stop();
                    break;
            }
        }
        public void StopAll()
        {
            musicAudioSource.Stop();
            effectAudioSource.Stop();
            foreach (var audioSource in effectAudioSourceArray)
                audioSource.Stop();
        }
        public void MusicFadeIn(float duration = 1)
        {
            float volume = -80;
            MusicFade(volume, duration, true);
        }
        public void MusicFadeIn(System.Action callback, float duration = 1)
        {
            float volume = -80;
            MusicFade(volume, duration, callback, true);
        }
        public void MusicFadeOut(float duration = 1)
        {
            float muteVolume = -80;
            SetVolume(muteVolume, AUDIO_OUTPUT.MUSIC);
            float volume = PlayerPrefs.GetFloat("music_configuration", 0);
            MusicFade(volume, duration, false);
        }
        public void MusicFadeOut(System.Action callback, float duration = 1)
        {
            float muteVolume = -80;
            SetVolume(muteVolume, AUDIO_OUTPUT.MUSIC);
            float volume = PlayerPrefs.GetFloat("music_configuration", 0);
            MusicFade(volume, duration, callback, false);
        }
        #endregion

        #region PRIVATE_FUNCTION
        private void UpdateAudioVolume()
        {
            float musicVolume = PlayerPrefs.GetFloat("music_configuration", 0);
            SetVolume(musicVolume, AUDIO_OUTPUT.MUSIC);

            float effectVolume = PlayerPrefs.GetFloat("effect_configuration", 0);
            SetVolume(effectVolume, AUDIO_OUTPUT.EFFECT);
        }
        private void PlayAudioClip(AudioClip audioClip, AUDIO_OUTPUT output, PLAY_SONG typePlay, float pitch, bool loop)
        {
            if (audioClip != null)
            {
                audioMixer.SetFloat("Pitch SoundEffect", pitch);
                UpdateAudioVolume();
                AudioSource audioSource = effectAudioSource;
                switch (output)
                {
                    case AUDIO_OUTPUT.MUSIC:
                        audioSource = musicAudioSource;
                        break;
                    case AUDIO_OUTPUT.EFFECT:
                        audioSource = effectAudioSource;
                        break;
                }

                switch (typePlay)
                {
                    case PLAY_SONG.PLAY:
                        audioSource.clip = audioClip;
                        audioSource.loop = loop;
                        audioSource.Play();
                        break;
                    case PLAY_SONG.ONESHOT:
                        audioSource.PlayOneShot(audioClip);
                        break;
                }
            }
            else
            {
                Debug.LogError($"The 'AudioClip' is Null");
            }
        }
        private void PlayEffectSong(AudioClip audioClip, float pitch, bool loop)
        {
            if (audioClip != null)
            {
                AudioSource freeAudioSource = effectAudioSource;
                foreach (AudioSource audioSource in effectAudioSourceArray)
                {
                    if (!audioSource.isPlaying)
                    {
                        freeAudioSource = audioSource;
                        break;
                    }
                }

                audioMixer.SetFloat("Pitch SoundEffect", pitch);
                UpdateAudioVolume();
                freeAudioSource.clip = audioClip;
                freeAudioSource.Play();
                freeAudioSource.PlayOneShot(audioClip);
            }
            else
            {
                Debug.LogError($"The 'AudioClip' is Null");
            }
        }
        private void MusicFade(float volume, float duration, bool stop)
        {
            KillSequence();
            sequence = DOTween.Sequence();
            sequence.Append(audioMixer.DOSetFloat("Music", volume, duration));
            sequence.OnComplete(() =>
            {
                if (stop)
                {
                    musicAudioSource.clip = null;
                    Stop(AUDIO_OUTPUT.MUSIC);
                }
            });
        }
        private void MusicFade(float volume, float duration, System.Action callback, bool stop, float delay = 0)
        {
            KillSequence();
            sequence = DOTween.Sequence();
            sequence.Append(audioMixer.DOSetFloat("Music", volume, duration));
            sequence.AppendInterval(delay);
            sequence.OnComplete(() =>
            {
                callback();
                if (stop)
                {
                    musicAudioSource.clip = null;
                    Stop(AUDIO_OUTPUT.MUSIC);
                }
            });
        }
        private void KillSequence()
        {
            if (sequence != null)
                sequence.Kill();
        }
        #endregion
    }
}