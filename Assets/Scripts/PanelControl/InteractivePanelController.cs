using UnityEngine;
using System.Collections;

namespace MirrorWater
{
    public class InteractivePanelController : BasePanelController
    {
        [Header("References")]
        [SerializeField] private GameFlowController flowController;

        [Header("Auto Return Settings")]
        [SerializeField] private float autoReturnDelay = 10f; // ���ʶ��q����ɶ��]��^

        private bool interactionActive = false;
        private Coroutine autoReturnRoutine;

        protected override void OnFadeComplete(bool isNowVisible)
        {
            if (isNowVisible)
                StartInteraction();
            else
                StopInteraction();
        }

        // �Ұʤ����޿�
        private void StartInteraction()
        {
            Debug.Log("[InteractivePanel] �}�l���ʶ��q");
            interactionActive = true;

            // �Ұʦ۰ʪ�^�˼�
            if (autoReturnRoutine != null) StopCoroutine(autoReturnRoutine);
            autoReturnRoutine = StartCoroutine(AutoReturnCoroutine());
        }

        // ������޿�
        private void StopInteraction()
        {
            Debug.Log("[InteractivePanel] �������ʶ��q");
            interactionActive = false;

            if (autoReturnRoutine != null)
            {
                StopCoroutine(autoReturnRoutine);
                autoReturnRoutine = null;
            }
        }

        // �@�q�ɶ���۰ʦ^��v�����A
        private IEnumerator AutoReturnCoroutine()
        {
            yield return new WaitForSeconds(autoReturnDelay);

            if (interactionActive)
            {
                Debug.Log("[InteractivePanel] �ɶ���A�۰ʪ�^�v�����q");
                flowController.OnInteractionFinished();
            }
        }

        // �Y�n���Ĳ�o�����]�O�d��\��^
        public void OnInteractionComplete()
        {
            if (!interactionActive) return;
            flowController?.OnInteractionFinished();
        }
    }
}
