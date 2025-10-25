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

        [Header("�v����Ƨ��]�w")]
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
                Debug.LogWarning("[VideoPanel] VideoPlayer �����w");
                return;
            }

            string videoPath = Path.Combine(Application.dataPath, idleDirectoryName);

            if (!Directory.Exists(videoPath))
            {
                Debug.LogWarning($"[VideoPanel] ���w��Ƨ����s�b: {videoPath}");
                StartCoroutine(DelayNotifyVideoFinished());
                return;
            }

            string[] files = Directory.GetFiles(videoPath, "*.mp4");
            if (files.Length == 0)
            {
                Debug.LogWarning("[VideoPanel] �䤣�� mp4 �v����");
                StartCoroutine(DelayNotifyVideoFinished());
                return;
            }

            // ���Ĥ@�Ӽv���ɼ���
            string selectedVideo = files[0];
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.url = selectedVideo;

            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.prepareCompleted += OnPrepared;
            videoPlayer.Prepare();


            Debug.Log($"[VideoPanel] �ǳƼ���v��: {selectedVideo}");
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
            Debug.Log($"[VideoPanel] �}�l����v��: {vp.url}");
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            Debug.Log($"OnVideoFinished.isOnVideoFinishedCalled:{isOnVideoFinishedCalled}");
            if (!isOnVideoFinishedCalled)
            {
                isOnVideoFinishedCalled = true;
                Debug.Log("[VideoPanel] �v�����񧹲�");
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

            // ����H�s
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

            // �T�O�̫�@�V�z���׺�T�]�w
            Color finalColor = rawImage.color;
            finalColor.a = targetAlpha;
            rawImage.color = finalColor;
        }
    }
}
