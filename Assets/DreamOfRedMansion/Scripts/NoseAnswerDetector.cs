using UnityEngine;
using PoseTypes;
using DreamOfRedMansion.Core;
using UnityEngine.Experimental.GlobalIllumination;

namespace DreamOfRedMansion
{
    /// <summary>
    /// �ھڻ�l X �y�Ц۰ʳ]�w���סG
    /// x < 0 �� ���]true�^�Ax > 0 �� �e�e�]false�^
    /// �Ȧb Question ���A�U�ҥΡC
    /// </summary>
    public class NoseAnswerDetector : MonoBehaviour
    {
        [Header("�ޥΤ���")]
        public QuestionManager questionManager;

        [Tooltip("�~�� WebSocket �ǤJ�����[���")]
        public FrameSample currentFrame;

        [Tooltip("�n�l�ܪ��ؼФH������ (0=�Ĥ@��)")]
        public int personIndex = 0;

        [Tooltip("�O�_��X�����T��")]
        public bool debugLog = true;

        private bool _isActive = false;

        [Tooltip("���жb�~�t")]
        [SerializeField] private Vector2 offset;

        /// <summary>
        /// �ѥ~���]�Ҧp GameFlowController�^�I�s�A�Ω�P�B�C�����A�C
        /// </summary>
        public void OnGameStateChanged(GameState newState)
        {
            _isActive = (newState == GameState.Question);

            if (debugLog)
                Debug.Log($"[NoseAnswerDetector] ���A���� �� {_isActive}");
        }

        public void UpdateFrame(FrameSample frame)
        {
            currentFrame = frame;
            if (debugLog)
                Debug.Log($"�O�_���\�P�_��l��m{_isActive}");
            if (!_isActive) return;

            if (frame == null || frame.persons == null || frame.persons.Count == 0)
                return;

            if (personIndex < 0 || personIndex >= frame.persons.Count)
                return;

            var person = frame.persons[personIndex];
            if (!person.TryGet(JointId.Nose, out var nose))
                return;

            //if (!nose.IsValid)
            //    return;

            //bool answer = nose.x < 0f; // ���� = ���(true)�A�k�� = �e�e(false)

            bool answer = DetermineTrueOrFalse(new Vector2(nose.x, nose.y), offset, out float result);
            questionManager.SetAnswer(answer);

            if (debugLog)
                Debug.Log($"[NoseAnswerDetector] ��l x-y={result} �� {(answer ? "���" : "�e�e")}");
        }
        bool DetermineTrueOrFalse(Vector2 coordinate, Vector2 offset, out float result)
        {
            Vector2 corrected = coordinate - offset;
            result = corrected.x - corrected.y;
            if (debugLog)
                Debug.Log($"x={corrected.x},y={corrected.y},x-y={result}");
            return result > 0;
        }

        private void Update()
        {

        }
    }
}
