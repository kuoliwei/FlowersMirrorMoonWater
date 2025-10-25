using System.Collections;
using UnityEngine;

namespace MirrorWater
{
    public abstract class BasePanelController : MonoBehaviour
    {
        [Header("Base Panel Settings")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected float fadeDuration = 1f;
        public float FadeDuration => fadeDuration; // 提供外部讀取淡入淡出時間

        protected bool isVisible = false;
        protected Coroutine fadeRoutine;

        protected virtual void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        // 淡入
        public virtual void Show()
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeCanvasGroup(0f, 1f, true));
        }

        // 淡出
        public virtual void Hide()
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeCanvasGroup(1f, 0f, false));
        }

        // 淡入淡出過程
        private IEnumerator FadeCanvasGroup(float start, float end, bool show)
        {
            isVisible = show;
            float timer = 0f;
            canvasGroup.alpha = start;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(start, end, timer / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = end;
            canvasGroup.interactable = show;
            canvasGroup.blocksRaycasts = show;
            OnFadeComplete(show);
        }

        // 淡入淡出結束後的回呼，供子類別覆寫
        protected virtual void OnFadeComplete(bool isNowVisible) { }
    }
}
