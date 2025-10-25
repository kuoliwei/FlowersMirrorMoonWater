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

            videoPlayer.loopPointReached -= OnVideoFinished; // �T�O�����ƭq�\
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Play();

            Debug.Log("[VideoPanel] �v���}�l����");
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            Debug.Log("[VideoPanel] �v�����񧹲�");
            flowController?.OnVideoFinished();
        }
    }
}
