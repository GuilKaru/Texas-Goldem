using DG.Tweening;
using SimplePoker.Attribute;
using UnityEngine;

namespace SimplePoker.UI
{
    /// <summary>
    /// Base class for UI panels elements.
    /// </summary>
    public class BaseUI : MonoBehaviour
    {
        [SerializeField, ReadOnly] protected UIStateManager UIStateManager;
        [SerializeField, ReadOnly] protected GameObject Panel;
        [SerializeField, ReadOnly] protected CanvasGroup CanvasGroup;
        [SerializeField] protected float enableFadeDuration = 0.05f;
        [SerializeField] protected float disableFadeDuration = 0.05f;


        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            Panel = transform.GetChild(0).gameObject;
            Panel.SetActive(false);
        }

        /// <summary>
        /// Enables the UI element with fade-in animation.
        /// </summary>
        public virtual void Enable()
        {
            Panel.SetActive(true);
            CanvasGroup.alpha = 0.1f;
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.DOFade(1, enableFadeDuration).SetUpdate(true).OnComplete(() => { CanvasGroup.blocksRaycasts = true; });
            UIStateManager = UIStateManager.Instance;
        }

        /// <summary>
        /// Disables the UI element with fade-out animation.
        /// </summary>
        public virtual void Disable()
        {
            if (!Panel.activeInHierarchy)
                return;
            CanvasGroup.alpha = 1;
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.DOFade(0, disableFadeDuration).SetUpdate(true).OnComplete(() =>
            {
                CanvasGroup.blocksRaycasts = false;
                Panel.SetActive(false);
            });
        }
    }
}