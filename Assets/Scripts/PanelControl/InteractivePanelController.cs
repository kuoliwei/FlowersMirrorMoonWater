using UnityEngine;
using System.Collections;

namespace MirrorWater
{
    public class InteractivePanelController : BasePanelController
    {
        [Header("References")]
        [SerializeField] private GameFlowController flowController;

        [SerializeField] private InteractiveSequenceController sequenceController;

        [Header("Auto Return Settings")]
        [SerializeField] private float autoReturnDelay = 10f; // 互動階段持續時間（秒）

        private bool interactionActive = false;
        private Coroutine autoReturnRoutine;

        protected override void OnFadeComplete(bool isNowVisible)
        {
            if (isNowVisible)
                StartInteraction();
            else
                StopInteraction();
        }

        private void StartInteraction()
        {
            Debug.Log("[InteractivePanel] 開始互動階段");
            interactionActive = true;

            // 在面板淡入完成後啟動輪播
            if (sequenceController != null)
            {
                Debug.Log("[InteractivePanel] 通知 InteractiveSequenceController 開始輪播");
                sequenceController.StartSequence();
            }
            else
            {
                Debug.LogWarning("[InteractivePanel] 尚未指定 InteractiveSequenceController");
            }

            // 啟動自動返回倒數
            if (autoReturnRoutine != null)
                StopCoroutine(autoReturnRoutine);
            autoReturnRoutine = StartCoroutine(AutoReturnCoroutine());
        }

        private void StopInteraction()
        {
            Debug.Log("[InteractivePanel] 結束互動階段");
            interactionActive = false;

            // 在面板淡出時停止輪播
            if (sequenceController != null)
            {
                Debug.Log("[InteractivePanel] 通知 InteractiveSequenceController 停止輪播");
                sequenceController.StopSequence();
            }

            if (autoReturnRoutine != null)
            {
                StopCoroutine(autoReturnRoutine);
                autoReturnRoutine = null;
            }
        }

        private IEnumerator AutoReturnCoroutine()
        {
            yield return new WaitForSeconds(autoReturnDelay);

            if (interactionActive)
            {
                Debug.Log("[InteractivePanel] 時間到，自動返回影片階段");
                flowController.OnInteractionFinished();
            }
        }

        public void OnInteractionComplete()
        {
            if (!interactionActive) return;
            flowController?.OnInteractionFinished();
        }
    }
}
