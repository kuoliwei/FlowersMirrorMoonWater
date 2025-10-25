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
        private bool isTransitioning = false; // �����A�קK���Ƥ���

        public bool IsVideoFinished { get; private set; }
        public bool IsInteractionFinished { get; private set; }

        private void Start()
        {
            stateMachine = new GameStateMachine();
            ChangeToVideo(); // �w�]�q�v�����A�}�l
        }

        private void Update()
        {
            stateMachine.Update();
        }

        // ���A������v���Ҧ�
        public void ChangeToVideo()
        {
            if (isTransitioning) return;
            stateMachine.ChangeState(new VideoState(this));
            StartCoroutine(TransitionPanels(interactivePanel, videoPanel));
        }

        // ���A�����줬�ʼҦ�
        public void ChangeToInteractive()
        {
            if (isTransitioning) return;
            stateMachine.ChangeState(new InteractiveState(this));
            StartCoroutine(TransitionPanels(videoPanel, interactivePanel));
        }

        // ����O�H�X�B�H�J������y�{
        private IEnumerator TransitionPanels(BasePanelController from, BasePanelController to)
        {
            isTransitioning = true;

            if (from != null)
                from.Hide();

            yield return new WaitForSeconds(from != null ? from.FadeDuration : 0f);

            if (to != null)
                to.Show();

            // ���ݷs���O�H�J������A�Ѱ���w
            yield return new WaitForSeconds(to != null ? to.FadeDuration : 0f);

            isTransitioning = false;
        }

        // ���A�޿�G����v��
        public void PlayVideo()
        {
            Debug.Log("����v����...");
            IsVideoFinished = false;
        }

        // ���A�޿�G�v�����񧹲�
        public void OnVideoFinished()
        {
            if (isTransitioning) return;
            IsVideoFinished = true;
        }

        // ���A�޿�G�ҥΤ��ʼҦ�
        public void EnableInteraction()
        {
            Debug.Log("�ҥΤ��ʼҦ�");
            IsInteractionFinished = false;
        }

        // ���A�޿�G���Τ��ʼҦ�
        public void DisableInteraction()
        {
            Debug.Log("���Τ��ʼҦ�");
        }

        // ���A�޿�G���ʧ���
        public void OnInteractionFinished()
        {
            if (isTransitioning) return;
            IsInteractionFinished = true;
        }
    }
}
