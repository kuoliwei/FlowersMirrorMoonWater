using DreamOfRedMansion.Core;
using PoseTypes;
using UnityEngine;

public class HandRaiseDetector : MonoBehaviour
{
    [Header("偵測參數")]
    public float requiredHoldSeconds = 1.0f;   // 需要連續有效多長時間才觸發
    public float maxValidGap = 0.2f;           // 兩筆有效資料間最大允許間隔
    public float shoulderOffset = 0.0f;        // 肩膀高度微調
    public bool debugLog = false;

    [Header("骨架資料")]
    public FrameSample currentFrame;

    public System.Action OnHandsRaised;

    private bool _isActive = false;            // 只在 Idle 才啟動
    public bool externalLock = false; // 外部控制是否允許偵測（例如影片播放時鎖住）
    private float _holdAccum = 0f;             // 已累積的有效時間
    private float _lastValidTs = -1f;          // 上一筆「有效」資料到達時間（Time.time）
    private bool _hasTriggered = false;

    public void OnGameStateChanged(GameState newState)
    {
        _isActive = (newState == GameState.Idle);
        if (debugLog) Debug.Log($"[HandRaiseDetector] 狀態切換 → {newState}，偵測{(_isActive ? "啟用" : "停用")}");
        ResetState();
    }

    private void ResetState()
    {
        _holdAccum = 0f;
        _lastValidTs = -1f;
        _hasTriggered = false;
    }

    /// <summary>
    /// 僅在接收到新的一幀資料時被呼叫；不使用 Update()
    /// </summary>
    public void UpdateFrame(FrameSample frame)
    {
        if (debugLog) Debug.Log($"_isActive:{_isActive},externalLock:{externalLock}");
        if (!_isActive || frame == null || frame.persons == null || frame.persons.Count == 0 || externalLock)
            return;

        currentFrame = frame;
        var person = frame.persons[0];

        // 取必需關節
        if (!person.TryGet(JointId.LeftShoulder, out var ls) ||
            !person.TryGet(JointId.RightShoulder, out var rs) ||
            !person.TryGet(JointId.LeftWrist, out var lw) ||
            !person.TryGet(JointId.RightWrist, out var rw))
        {
            // 取不到視為無效 → 立即歸零
            ResetState();
            return;
        }

        // 僅檢查數值是否有效（不看 confidence）
        if (float.IsNaN(ls.z) || float.IsNaN(rs.z) || float.IsNaN(lw.z) || float.IsNaN(rw.z))
        {
            ResetState();
            return;
        }

        // 以 z 為高度（已和 SkeletonDataProcessor 對齊），肩膀取平均再加可選偏移
        float shoulderHeight = (ls.z + rs.z) * 0.5f + shoulderOffset;
        bool leftUp = lw.z > shoulderHeight;
        bool rightUp = rw.z > shoulderHeight;
        bool valid = leftUp && rightUp;

        float now = Time.time;

        if (!valid)
        {
            // 收到無效資料 → 立即歸零
            if (debugLog) Debug.Log("[HandRaiseDetector] 無效資料 → 歸零");
            ResetState();
            return;
        }

        // 走到這裡代表本幀是「有效資料」
        if (_lastValidTs < 0f)
        {
            // 第一筆有效資料：只記時間，不累加
            _lastValidTs = now;
            _holdAccum = 0f;
        }
        else
        {
            float gap = now - _lastValidTs;

            if (gap > maxValidGap)
            {
                // 間隔太久 → 累積歸零，視為重新開始
                if (debugLog) Debug.Log($"[HandRaiseDetector] 有效間隔超過 {maxValidGap:F2}s → 重置累積");
                _holdAccum = 0f;
            }
            else
            {
                // 在允許間隔內 → 累計本次間隔
                _holdAccum += gap;
                //if (debugLog) Debug.Log($"[HandRaiseDetector] 連續有效累積 = {_holdAccum:F3}s");
            }

            _lastValidTs = now;
        }

        // 判斷是否達標
        if (!_hasTriggered && _holdAccum >= requiredHoldSeconds)
        {
            _hasTriggered = true;
            if (debugLog) Debug.Log("[HandRaiseDetector] 連續有效達標 → 觸發 OnHandsRaised");
            OnHandsRaised?.Invoke();
        }
    }
}
