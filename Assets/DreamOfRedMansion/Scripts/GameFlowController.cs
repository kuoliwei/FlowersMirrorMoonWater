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
            // 建立並初始化狀態機
            _stateMachine = new GameStateMachine();

            // 監聽狀態變化
            _stateMachine.OnStateChanged += HandleStateChanged;

            // 將狀態轉播給偵測器
            _stateMachine.OnStateChanged += (newState) =>
            {
                handRaiseDetector?.OnGameStateChanged(newState);
                noseAnswerDetector?.OnGameStateChanged(newState);
                groundEffectController?.OnGameStateChanged(newState);
            };

            // 延後 Bootstrap 到 Start()，確保所有引用（例如 idleVideoPlayer）都已初始化
        }

        private void Start()
        {
            // 所有事件訂閱完成後才廣播初始狀態
            _stateMachine.Bootstrap();
        }

        /// <summary>
        /// 狀態變更時執行對應流程。
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
        /// 進入 Idle 狀態。
        /// </summary>
        private void EnterIdle()
        {
            screenUI.ShowIdleScreen();
            groundEffect.SetIdle();

            // 避免重複綁定事件
            handRaiseDetector.OnHandsRaised = null;
            handRaiseDetector.OnHandsRaised += () =>
            {
                _stateMachine.ChangeState(GameState.Question);
            };
        }
    }
}
