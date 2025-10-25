using System.Collections;
using UnityEngine;

namespace MirrorWater
{
    public class GameFlowController : MonoBehaviour
    {
        [Header("Panel Controllers")]
        [SerializeField] private VideoPanelController videoPanel;
        [SerializeField] private InteractivePanelController interactivePanel;

        private GameStateMachine stateMachine;
        private bool isTransitioning = false; // 轉場鎖，避免重複切換

        public bool IsVideoFinished { get; private set; }
        public bool IsInteractionFinished { get; private set; }

        private void Start()
        {
            stateMachine = new GameStateMachine();
            ChangeToVideo(); // 預設從影片狀態開始
        }

        private void Update()
        {
            stateMachine.Update();
        }

        // 狀態切換到影片模式
        public void ChangeToVideo()
        {
            if (isTransitioning) return;
            stateMachine.ChangeState(new VideoState(this));
            StartCoroutine(TransitionPanels(interactivePanel, videoPanel));
        }

        // 狀態切換到互動模式
        public void ChangeToInteractive()
        {
            if (isTransitioning) return;
            stateMachine.ChangeState(new InteractiveState(this));
            StartCoroutine(TransitionPanels(videoPanel, interactivePanel));
        }

        // 控制面板淡出、淡入的轉場流程
        private IEnumerator TransitionPanels(BasePanelController from, BasePanelController to)
        {
            isTransitioning = true;

            if (from != null)
                from.Hide();

            yield return new WaitForSeconds(from != null ? from.FadeDuration : 0f);

            if (to != null)
                to.Show();

            // 等待新面板淡入完成後再解除鎖定
            yield return new WaitForSeconds(to != null ? to.FadeDuration : 0f);

            isTransitioning = false;
        }

        // 狀態邏輯：播放影片
        public void PlayVideo()
        {
            Debug.Log("播放影片中...");
            IsVideoFinished = false;
        }

        // 狀態邏輯：影片播放完畢
        public void OnVideoFinished()
        {
            if (isTransitioning) return;
            IsVideoFinished = true;
        }

        // 狀態邏輯：啟用互動模式
        public void EnableInteraction()
        {
            Debug.Log("啟用互動模式");
            IsInteractionFinished = false;
        }

        // 狀態邏輯：停用互動模式
        public void DisableInteraction()
        {
            Debug.Log("停用互動模式");
        }

        // 狀態邏輯：互動完成
        public void OnInteractionFinished()
        {
            if (isTransitioning) return;
            IsInteractionFinished = true;
        }
    }
}
