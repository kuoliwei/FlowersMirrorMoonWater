using System.Collections;
using UnityEngine;

namespace MirrorWater
{
    public abstract class BasePanelController : MonoBehaviour
    {
        [Header("Base Panel Settings")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected float fadeDuration = 1f;
        public float FadeDuration => fadeDuration; // ���ѥ~��Ū���H�J�H�X�ɶ�

        protected bool isVisible = false;
        protected Coroutine fadeRoutine;

        protected virtual void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        // �H�J
        public virtual void Show()
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeCanvasGroup(0f, 1f, true));
        }

        // �H�X
        public virtual void Hide()
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeCanvasGroup(1f, 0f, false));
        }

        // �H�J�H�X�L�{
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

        // �H�J�H�X�����᪺�^�I�A�Ѥl���O�мg
        protected virtual void OnFadeComplete(bool isNowVisible) { }
    }
}
