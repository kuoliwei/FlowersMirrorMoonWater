using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

namespace DreamOfRedMansion
{
    /// <summary>
    /// 控制 Idle 狀態下的畫面（影片 → 靜態封面）與淡入淡出。
    /// </summary>
    public class IdlePanelController : MonoBehaviour
    {
        [Header("組件參考")]
        [SerializeField] private RawImage rawImage;               // 影片播放用
        [SerializeField] private CanvasGroup imageGroup;          // 開場封面群組（含子物件）
        [SerializeField] private IdleVideoPlayer idleVideoPlayer; // 負責實際播放影片
        [SerializeField] private HandRaiseDetector handRaiseDetector; // 體驗者手舉偵測器
        [SerializeField] private AudioController audioController;

        [Header("淡入淡出設定")]
        public float fadeDuration = 1f;

        private bool _isPlayingVideo = false;

        private void Awake()
        {
            if (idleVideoPlayer != null) { }
                idleVideoPlayer.OnVideoFinished += HandleVideoFinished;
        }

        private void OnDestroy()
        {
            if (idleVideoPlayer != null)
                idleVideoPlayer.OnVideoFinished -= HandleVideoFinished;
        }

        /// <summary>
        /// Idle 狀態開始時呼叫。
        /// </summary>
        public void OnIdleEnter()
        {
            StopAllCoroutines();
            audioController.StopBGM();
            // 鎖住偵測直到影片結束
            if (handRaiseDetector != null)
            {
                handRaiseDetector.externalLock = true;
                handRaiseDetector.enabled = true; // 保持啟用狀態（不怕 GameFlow 再切換）
            }

            // 初始化畫面
            if (imageGroup != null)
            {
                imageGroup.gameObject.SetActive(false);
                imageGroup.alpha = 0f;
            }

            if (rawImage != null)
            {
                rawImage.gameObject.SetActive(true);
                SetRawImageAlpha(1f);
            }

            // 開始播放影片（不可中斷）
            _isPlayingVideo = true;
            idleVideoPlayer?.PlayOnce();
        }

        /// <summary>
        /// 當影片播放結束時觸發。
        /// </summary>
        private void HandleVideoFinished()
        {
            Debug.Log("HandleVideoFinished invoke");
            _isPlayingVideo = false;
            audioController.PlayQuestionBGM();
            StartCoroutine(FadeOutRawImageThenShowImage());
        }

        private IEnumerator FadeOutRawImageThenShowImage()
        {
            // RawImage 淡出
            if (rawImage != null)
            {
                float time = 0f;
                Color color = rawImage.color;
                float startAlpha = color.a;

                while (time < fadeDuration)
                {
                    time += Time.deltaTime;
                    color.a = Mathf.Lerp(startAlpha, 0f, time / fadeDuration);
                    rawImage.color = color;
                    yield return null;
                }

                color.a = 0f;
                rawImage.color = color;
                rawImage.gameObject.SetActive(false);
            }

            // ImageGroup 淡入
            if (imageGroup != null)
            {
                imageGroup.gameObject.SetActive(true);
                yield return StartCoroutine(FadeCanvasGroup(imageGroup, 0f, 1f, fadeDuration));
            }

            // 啟用手舉偵測
            if (handRaiseDetector != null)
                handRaiseDetector.externalLock = false;
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
        {
            float t = 0f;
            group.alpha = from;
            while (t < duration)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            group.alpha = to;
        }

        private void SetRawImageAlpha(float a)
        {
            if (rawImage == null) return;
            Color c = rawImage.color;
            c.a = a;
            rawImage.color = c;
        }
    }
}
