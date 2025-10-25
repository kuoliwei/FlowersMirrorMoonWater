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

        private void StartInteraction()
        {
            Debug.Log("[InteractivePanel] �}�l���ʶ��q");
            interactionActive = true;

            // �b���O�H�J������Ұʽ���
            if (sequenceController != null)
            {
                Debug.Log("[InteractivePanel] �q�� InteractiveSequenceController �}�l����");
                sequenceController.StartSequence();
            }
            else
            {
                Debug.LogWarning("[InteractivePanel] �|�����w InteractiveSequenceController");
            }

            // �Ұʦ۰ʪ�^�˼�
            if (autoReturnRoutine != null)
                StopCoroutine(autoReturnRoutine);
            autoReturnRoutine = StartCoroutine(AutoReturnCoroutine());
        }

        private void StopInteraction()
        {
            Debug.Log("[InteractivePanel] �������ʶ��q");
            interactionActive = false;

            // �b���O�H�X�ɰ������
            if (sequenceController != null)
            {
                Debug.Log("[InteractivePanel] �q�� InteractiveSequenceController �������");
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
                Debug.Log("[InteractivePanel] �ɶ���A�۰ʪ�^�v�����q");
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
