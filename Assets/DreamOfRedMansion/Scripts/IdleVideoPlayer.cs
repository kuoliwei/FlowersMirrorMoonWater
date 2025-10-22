using UnityEngine;
using UnityEngine.Video;
using System;
using System.IO;

namespace DreamOfRedMansion
{
    /// <summary>
    /// 控制 Idle 狀態時播放影片（僅播放一次）。
    /// </summary>
    public class IdleVideoPlayer : MonoBehaviour
    {
        [Header("影片設定")]
        public string idleDirectoryName;

        [SerializeField] private VideoPlayer videoPlayer;

        public event Action OnVideoFinished;

        private void Awake()
        {
            if (videoPlayer == null)
                videoPlayer = GetComponent<VideoPlayer>();

            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false; // 確保播放一次後自動停下
            videoPlayer.loopPointReached += OnVideoEndReached;
        }

        private void OnDestroy()
        {
            videoPlayer.loopPointReached -= OnVideoEndReached;
        }

        public void PlayOnce()
        {
            string videoPath = Path.Combine(Application.dataPath, idleDirectoryName);
            if (!Directory.Exists(videoPath))
            {
                Debug.LogWarning($"[IdleVideoPlayer] 指定資料夾不存在: {videoPath}");
                OnVideoFinished?.Invoke(); // 若路徑不存在跳過影片播放繼續執行
                return;
            }

            string[] files = Directory.GetFiles(videoPath, "*.mp4");
            if (files.Length == 0)
            {
                Debug.LogWarning("[IdleVideoPlayer] 找不到 mp4 影片檔");
                OnVideoFinished?.Invoke(); // 若影片不存在跳過影片播放繼續執行
                return;
            }

            videoPlayer.url = files[0];
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnPrepared;
        }

        private void OnPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnPrepared;
            vp.Play();
            Debug.Log($"[IdleVideoPlayer] 開始播放影片：{vp.url}");
        }

        private void OnVideoEndReached(VideoPlayer vp)
        {
            Debug.Log("[IdleVideoPlayer] 影片播放結束");
            OnVideoFinished?.Invoke();
        }
    }
}
