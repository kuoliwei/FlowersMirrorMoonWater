using UnityEngine;
using DreamOfRedMansion.Core;

namespace DreamOfRedMansion
{
    public class GameFlowController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HandRaiseDetector handRaiseDetector;
        [SerializeField] private QuestionManager questionManager;
        [SerializeField] private ResultCalculator resultCalculator;
        [SerializeField] private ScreenUIController screenUI;
        [SerializeField] private GroundEffectController groundEffect;
        [SerializeField] private NoseAnswerDetector noseAnswerDetector;
        [SerializeField] private GroundEffectController groundEffectController;
        //[SerializeField] private AudioController audioController;

        private GameStateMachine _stateMachine;

        private void Awake()
        {
            // �إߨê�l�ƪ��A��
            _stateMachine = new GameStateMachine();

            // ��ť���A�ܤ�
            _stateMachine.OnStateChanged += HandleStateChanged;

            // �N���A�༽��������
            _stateMachine.OnStateChanged += (newState) =>
            {
                handRaiseDetector?.OnGameStateChanged(newState);
                noseAnswerDetector?.OnGameStateChanged(newState);
                groundEffectController?.OnGameStateChanged(newState);
            };

            // ���� Bootstrap �� Start()�A�T�O�Ҧ��ޥΡ]�Ҧp idleVideoPlayer�^���w��l��
        }

        private void Start()
        {
            // �Ҧ��ƥ�q�\������~�s����l���A
            _stateMachine.Bootstrap();
        }

        /// <summary>
        /// ���A�ܧ�ɰ�������y�{�C
        /// </summary>
        private void HandleStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Idle:
                    //audioController.StopBGM();
                    EnterIdle();
                    break;

                case GameState.Question:
                    //audioController.PlayQuestionBGM();
                    screenUI.ShowQuestion();
                    StartCoroutine(questionManager.RunQuestionFlow(() =>
                    {
                        _stateMachine.ChangeState(GameState.Result);
                    }));
                    break;

                case GameState.Result:
                    screenUI.ShowResult();
                    //audioController.StopBGM();
                    StartCoroutine(resultCalculator.RunResultPhase(questionManager.collectedAnswers, () =>
                    {
                        _stateMachine.ChangeState(GameState.Idle);
                    }));
                    break;
            }
        }

        /// <summary>
        /// �i�J Idle ���A�C
        /// </summary>
        private void EnterIdle()
        {
            screenUI.ShowIdleScreen();
            groundEffect.SetIdle();

            // �קK���Ƹj�w�ƥ�
            handRaiseDetector.OnHandsRaised = null;
            handRaiseDetector.OnHandsRaised += () =>
            {
                _stateMachine.ChangeState(GameState.Question);
            };
        }
    }
}
