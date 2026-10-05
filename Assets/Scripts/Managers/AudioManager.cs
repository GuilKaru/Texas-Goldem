using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TexasHoldem
{
    
        public class AudioManager : MonoBehaviour
        {
             public static AudioManager Instance { get; private set; }

            [Header("Audio Sources")]
            [SerializeField] private AudioSource ambienceSource;
            [SerializeField] private AudioSource musicSource;

            [Header("Ambience")]
            [SerializeField] private AudioClip ambienceClip;
            [SerializeField] [Range(0f, 1f)] private float ambienceVolume = 1f;

            [Header("Music Playlist")]
            [SerializeField] private List<AudioClip> musicTracks = new List<AudioClip>();
            [SerializeField] [Range(0f, 1f)] private float musicVolume = 1f;

            private Coroutine musicLoopCoroutine;
            private int lastTrackIndex = -1;
            private bool isPlayingMatchAudio = false;

            private void Awake()
            {
                if (Instance != null && Instance != this)
                {
                    Destroy(gameObject);
                    return;
                }

                Instance = this;
            }

            public void StartMatchAudio()
            {
                if (isPlayingMatchAudio)
                    return;

                isPlayingMatchAudio = true;
            }

            public void StopAllAudio()
            {
                isPlayingMatchAudio = false;

                if (musicLoopCoroutine != null)
                {
                    StopCoroutine(musicLoopCoroutine);
                    musicLoopCoroutine = null;
                }

                if (ambienceSource != null)
                    ambienceSource.Stop();

                if (musicSource != null)
                    musicSource.Stop();

                lastTrackIndex = -1;
            }

            public void StartAmbience()
            {

                ambienceSource.clip = ambienceClip;
                ambienceSource.volume = ambienceVolume;
                ambienceSource.loop = true;

                if (!ambienceSource.isPlaying)
                    ambienceSource.Play();
            }

            public void StopAmbience()
            {
                if (ambienceSource != null)
                    ambienceSource.Stop();
            }

            public void StartMusicLoop()
            {

                if (musicLoopCoroutine != null)
                    StopCoroutine(musicLoopCoroutine);

                musicLoopCoroutine = StartCoroutine(MusicLoopRoutine());
            }

            public void StopMusic()
            {
                if (musicLoopCoroutine != null)
                {
                    StopCoroutine(musicLoopCoroutine);
                    musicLoopCoroutine = null;
                }

                if (musicSource != null)
                    musicSource.Stop();

                lastTrackIndex = -1;
            }

            private IEnumerator MusicLoopRoutine()
            {
                while (isPlayingMatchAudio)
                {
                    AudioClip nextTrack = GetRandomTrack();
                    if (nextTrack == null)
                        yield break;

                    musicSource.clip = nextTrack;
                    musicSource.volume = musicVolume;
                    musicSource.loop = false;
                    Debug.Log($"[AudioManager] Now playing track: {nextTrack.name}");
                    musicSource.Play();
                    
                    
                    yield return new WaitWhile(() => musicSource != null && musicSource.isPlaying);
                }

                musicLoopCoroutine = null;
            }

            private AudioClip GetRandomTrack()
            {
                if (musicTracks == null || musicTracks.Count == 0)
                    return null;

                if (musicTracks.Count == 1)
                {
                    lastTrackIndex = 0;
                    return musicTracks[0];
                }

                int randomIndex;
                do
                {
                    randomIndex = Random.Range(0, musicTracks.Count);
                }
                while (randomIndex == lastTrackIndex);

                lastTrackIndex = randomIndex;
                return musicTracks[randomIndex];
            }
        }
}
