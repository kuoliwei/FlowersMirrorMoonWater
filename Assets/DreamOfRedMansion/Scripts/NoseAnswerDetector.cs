using UnityEngine;
using PoseTypes;
using DreamOfRedMansion.Core;
using UnityEngine.Experimental.GlobalIllumination;

namespace DreamOfRedMansion
{
    /// <summary>
    /// 根據鼻子 X 座標自動設定答案：
    /// x < 0 → 圈圈（true），x > 0 → 叉叉（false）
    /// 僅在 Question 狀態下啟用。
    /// </summary>
    public class NoseAnswerDetector : MonoBehaviour
    {
        [Header("引用元件")]
        public QuestionManager questionManager;

        [Tooltip("外部 WebSocket 傳入的骨架資料")]
        public FrameSample currentFrame;

        [Tooltip("要追蹤的目標人物索引 (0=第一位)")]
        public int personIndex = 0;

        [Tooltip("是否輸出除錯訊息")]
        public bool debugLog = true;

        private bool _isActive = false;

        [Tooltip("坐標軸誤差")]
        [SerializeField] private Vector2 offset;

        /// <summary>
        /// 由外部（例如 GameFlowController）呼叫，用於同步遊戲狀態。
        /// </summary>
        public void OnGameStateChanged(GameState newState)
        {
            _isActive = (newState == GameState.Question);

            if (debugLog)
                Debug.Log($"[NoseAnswerDetector] 狀態切換 → {_isActive}");
        }

        public void UpdateFrame(FrameSample frame)
        {
            currentFrame = frame;
            if (debugLog)
                Debug.Log($"是否允許判斷鼻子位置{_isActive}");
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

            //bool answer = nose.x < 0f; // 左邊 = 圈圈(true)，右邊 = 叉叉(false)

            bool answer = DetermineTrueOrFalse(new Vector2(nose.x, nose.y), offset, out float result);
            questionManager.SetAnswer(answer);

            if (debugLog)
                Debug.Log($"[NoseAnswerDetector] 鼻子 x-y={result} → {(answer ? "圈圈" : "叉叉")}");
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
