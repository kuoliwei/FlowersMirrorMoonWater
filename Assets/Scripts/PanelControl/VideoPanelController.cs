using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Collections;
using UnityEngine.UI;

namespace MirrorWater
{
    public class VideoPanelController : BasePanelController
    {
        [Header("References")]
        [SerializeField] private GameFlowController flowController;
        [SerializeField] private VideoPlayer videoPlayer;


        [SerializeField] RawImage rawImage;

        [Header("影片資料夾設定")]
        [SerializeField] private string idleDirectoryName = "VideoClip";

        private bool isOnVideoFinishedCalled = false;
        protected override void OnFadeComplete(bool isNowVisible)
        {
            isOnVideoFinishedCalled = false;
            if (isNowVisible)
            {
                PlayVideoFromDirectory();
            }
            else if (videoPlayer != null)
            {
                videoPlayer.Stop();
            }
        }

        private void PlayVideoFromDirectory()
        {
            if (videoPlayer == null)
            {
                Debug.LogWarning("[VideoPanel] VideoPlayer 未指定");
                return;
            }

            string videoPath = Path.Combine(Application.dataPath, idleDirectoryName);

            if (!Directory.Exists(videoPath))
            {
                Debug.LogWarning($"[VideoPanel] 指定資料夾不存在: {videoPath}");
                StartCoroutine(DelayNotifyVideoFinished());
                return;
            }

            string[] files = Directory.GetFiles(videoPath, "*.mp4");
            if (files.Length == 0)
            {
                Debug.LogWarning("[VideoPanel] 找不到 mp4 影片檔");
                StartCoroutine(DelayNotifyVideoFinished());
                return;
            }

            // 取第一個影片檔播放
            string selectedVideo = files[0];
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.url = selectedVideo;

            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.prepareCompleted += OnPrepared;
            videoPlayer.Prepare();


            Debug.Log($"[VideoPanel] 準備播放影片: {selectedVideo}");
        }
        private IEnumerator DelayNotifyVideoFinished()
        {
            yield return new WaitUntil(() => !videoPlayer.isPlaying);
            yield return new WaitUntil(() => !flowController.IsTransitioning);
            flowController?.OnVideoFinished();
        }
        private void OnPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnPrepared;
            vp.Play();
            StartCoroutine(AfterVideoPlay());
            Debug.Log($"[VideoPanel] 開始播放影片: {vp.url}");
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            Debug.Log($"OnVideoFinished.isOnVideoFinishedCalled:{isOnVideoFinishedCalled}");
            if (!isOnVideoFinishedCalled)
            {
                isOnVideoFinishedCalled = true;
                Debug.Log("[VideoPanel] 影片播放完畢");
                StartCoroutine(DelayNotifyVideoFinished());
            }
        }
        private IEnumerator AfterVideoPlay()
        {
            Debug.Log($"AfterVideoPlay,isPlaying:{videoPlayer.isPlaying}");
            yield return new WaitUntil(() => videoPlayer.isPlaying);
            Debug.Log($"AfterVideoPlay,isPlaying:{videoPlayer.isPlaying}");
            yield return StartCoroutine(FadeRawImage(1, 1));
            yield return new WaitUntil(() => !videoPlayer.isPlaying);
            Debug.Log($"AfterVideoPlay,isPlaying:{videoPlayer.isPlaying}");
            yield return StartCoroutine(FadeRawImage(0, 1));
            OnVideoFinished(videoPlayer);
        }
        private IEnumerator FadeRawImage(float targetAlpha, float duration)
        {
            if (rawImage == null)
                yield break;

            float startAlpha = rawImage.color.a;
            float timer = 0f;

            // 防止除以零
            duration = Mathf.Max(0.0001f, duration);

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);

                Color c = rawImage.color;
                c.a = newAlpha;
                rawImage.color = c;

                yield return null;
            }

            // 確保最後一幀透明度精確設定
            Color finalColor = rawImage.color;
            finalColor.a = targetAlpha;
            rawImage.color = finalColor;
        }
    }
}
