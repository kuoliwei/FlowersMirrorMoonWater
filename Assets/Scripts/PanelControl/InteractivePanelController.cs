using UnityEngine;
using System.Collections;

namespace MirrorWater
{
    public class InteractivePanelController : BasePanelController
    {
        [Header("References")]
        [SerializeField] private GameFlowController flowController;

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

        // 啟動互動邏輯
        private void StartInteraction()
        {
            Debug.Log("[InteractivePanel] 開始互動階段");
            interactionActive = true;

            // 啟動自動返回倒數
            if (autoReturnRoutine != null) StopCoroutine(autoReturnRoutine);
            autoReturnRoutine = StartCoroutine(AutoReturnCoroutine());
        }

        // 停止互動邏輯
        private void StopInteraction()
        {
            Debug.Log("[InteractivePanel] 結束互動階段");
            interactionActive = false;

            if (autoReturnRoutine != null)
            {
                StopCoroutine(autoReturnRoutine);
                autoReturnRoutine = null;
            }
        }

        // 一段時間後自動回到影片狀態
        private IEnumerator AutoReturnCoroutine()
        {
            yield return new WaitForSeconds(autoReturnDelay);

            if (interactionActive)
            {
                Debug.Log("[InteractivePanel] 時間到，自動返回影片階段");
                flowController.OnInteractionFinished();
            }
        }

        // 若要手動觸發結束（保留原功能）
        public void OnInteractionComplete()
        {
            if (!interactionActive) return;
            flowController?.OnInteractionFinished();
        }
    }
}
