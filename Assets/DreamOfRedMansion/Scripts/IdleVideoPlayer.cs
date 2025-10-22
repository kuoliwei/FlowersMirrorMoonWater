using UnityEngine;
using UnityEngine.Video;
using System;
using System.IO;

namespace DreamOfRedMansion
{
    /// <summary>
    /// ���� Idle ���A�ɼ���v���]�ȼ���@���^�C
    /// </summary>
    public class IdleVideoPlayer : MonoBehaviour
    {
        [Header("�v���]�w")]
        public string idleDirectoryName;

        [SerializeField] private VideoPlayer videoPlayer;

        public event Action OnVideoFinished;

        private void Awake()
        {
            if (videoPlayer == null)
                videoPlayer = GetComponent<VideoPlayer>();

            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false; // �T�O����@����۰ʰ��U
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
                Debug.LogWarning($"[IdleVideoPlayer] ���w��Ƨ����s�b: {videoPath}");
                OnVideoFinished?.Invoke(); // �Y���|���s�b���L�v�������~�����
                return;
            }

            string[] files = Directory.GetFiles(videoPath, "*.mp4");
            if (files.Length == 0)
            {
                Debug.LogWarning("[IdleVideoPlayer] �䤣�� mp4 �v����");
                OnVideoFinished?.Invoke(); // �Y�v�����s�b���L�v�������~�����
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
            Debug.Log($"[IdleVideoPlayer] �}�l����v���G{vp.url}");
        }

        private void OnVideoEndReached(VideoPlayer vp)
        {
            Debug.Log("[IdleVideoPlayer] �v�����񵲧�");
            OnVideoFinished?.Invoke();
        }
    }
}
