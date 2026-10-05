using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;
using SimplePoker.Attribute;

namespace SimplePoker.SceneController
{
    /// <summary>
    /// Singleton class to manages scene transitions with fading effects.
    /// </summary>
    public class TransitionSceneManager : Singleton<TransitionSceneManager>
    {
        #region VARIABLES
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private float defaultDurationFade = 0.3f;
        [SerializeField] private float delayTransition = 0.5f;
        [SerializeField, ReadOnly] private string nextScene;


        public event EventHandler OnFadingStart;
        public event EventHandler OnFadingEnd;

        public event EventHandler OnFadeOutStart;
        public event EventHandler OnFadeOutEnd;
        #endregion

        #region UNITY_FUNCTIONS
        private void OnEnable()
        {
            canvas.gameObject.SetActive(false);
            canvas.worldCamera = Camera.main;
            SceneManager.activeSceneChanged += ChangedActiveScene;
        }
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= ChangedActiveScene;
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        /// <summary>
        /// Loads the specified scene with optional delay.
        /// </summary>
        /// <param name="scene">The name of the scene to load.</param>
        /// <param name="delay">Optional delay before loading the scene.</param>
        public void LoadScene(string scene, float delay = 0) => StartCoroutine(LoadSceneCoroutine(scene, delay));

        /// <summary>
        /// Restarts the current scene with optional delay.
        /// </summary>
        /// <param name="delay">Optional delay before restarting the scene.</param>
        public void RestartScene(float delay = 0) => StartCoroutine(LoadSceneCoroutine(SceneManager.GetActiveScene().name, delay));

        /// <summary>
        /// Starts the scene transition.
        /// </summary>
        public void StartScene() => FadeOut();
        private IEnumerator LoadSceneCoroutine(string scene, float time)
        {
            nextScene = scene;
            canvasGroup.alpha = 0;
            yield return new WaitForSecondsRealtime(time);
            Fading();
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void ChangedActiveScene(Scene current, Scene next)
        {
            Time.timeScale = 1;
            FadeOut();
        }
        private void Fading()
        {
            OnFadingStart?.Invoke(this, EventArgs.Empty);

            if (EventSystem.current != null)
                EventSystem.current.enabled = false;
            
            canvas.gameObject.SetActive(true);
    
            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);
            sequence.Append(canvasGroup.DOFade(1, defaultDurationFade));
            sequence.AppendInterval(delayTransition);
            sequence.OnComplete(() =>
            {
                canvasGroup.alpha = 1;
                OnFadingEnd?.Invoke(this, EventArgs.Empty);
                LoadNextScene();
            });
        }
        private void FadeOut()
        {
            OnFadeOutStart?.Invoke(this, EventArgs.Empty);

            canvasGroup.alpha = 1;
            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);
            sequence.Append(canvasGroup.DOFade(0, defaultDurationFade));
            sequence.AppendInterval(delayTransition);
            sequence.OnComplete(() =>
            {
                if (EventSystem.current != null)
                    EventSystem.current.enabled = true;
                canvasGroup.alpha = 0;
                canvas.gameObject.SetActive(false);
                OnFadeOutEnd?.Invoke(this, EventArgs.Empty);
            });
        }
        private void LoadNextScene()
        {
            DOTween.KillAll();
            SceneManager.LoadSceneAsync(nextScene);
        }
        #endregion
    }
}