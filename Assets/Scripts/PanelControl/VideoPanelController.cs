using UnityEngine;
using UnityEngine.Video;

namespace MirrorWater
{
    public class VideoPanelController : BasePanelController
    {
        [Header("References")]
        [SerializeField] private GameFlowController flowController;
        [SerializeField] private VideoPlayer videoPlayer;

        protected override void OnFadeComplete(bool isNowVisible)
        {
            if (isNowVisible)
            {
                PlayVideo();
            }
            else if (videoPlayer != null)
            {
                videoPlayer.Stop();
            }
        }

        private void PlayVideo()
        {
            if (videoPlayer == null) return;

            videoPlayer.loopPointReached -= OnVideoFinished; // 確保不重複訂閱
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Play();

            Debug.Log("[VideoPanel] 影片開始播放");
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            Debug.Log("[VideoPanel] 影片播放完畢");
            flowController?.OnVideoFinished();
        }
    }
}
