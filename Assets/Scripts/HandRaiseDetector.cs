using DreamOfRedMansion.Core;
using PoseTypes;
using UnityEngine;

public class HandRaiseDetector : MonoBehaviour
{
    [Header("�����Ѽ�")]
    public float requiredHoldSeconds = 1.0f;   // �ݭn�s�򦳮Ħh���ɶ��~Ĳ�o
    public float maxValidGap = 0.2f;           // �ⵧ���ĸ�ƶ��̤j���\���j
    public float shoulderOffset = 0.0f;        // �ӻH���׷L��
    public bool debugLog = false;

    [Header("���[���")]
    public FrameSample currentFrame;

    public System.Action OnHandsRaised;

    private bool _isActive = false;            // �u�b Idle �~�Ұ�
    public bool externalLock = false; // �~������O�_���\�����]�Ҧp�v����������^
    private float _holdAccum = 0f;             // �w�ֿn�����Įɶ�
    private float _lastValidTs = -1f;          // �W�@���u���ġv��ƨ�F�ɶ��]Time.time�^
    private bool _hasTriggered = false;

    public void OnGameStateChanged(GameState newState)
    {
        _isActive = (newState == GameState.Idle);
        if (debugLog) Debug.Log($"[HandRaiseDetector] ���A���� �� {newState}�A����{(_isActive ? "�ҥ�" : "����")}");
        ResetState();
    }

    private void ResetState()
    {
        _holdAccum = 0f;
        _lastValidTs = -1f;
        _hasTriggered = false;
    }

    /// <summary>
    /// �Ȧb������s���@�V��ƮɳQ�I�s�F���ϥ� Update()
    /// </summary>
    public void UpdateFrame(FrameSample frame)
    {
        if (debugLog) Debug.Log($"_isActive:{_isActive},externalLock:{externalLock}");
        if (!_isActive || frame == null || frame.persons == null || frame.persons.Count == 0 || externalLock)
            return;

        currentFrame = frame;
        var person = frame.persons[0];

        // ���������`
        if (!person.TryGet(JointId.LeftShoulder, out var ls) ||
            !person.TryGet(JointId.RightShoulder, out var rs) ||
            !person.TryGet(JointId.LeftWrist, out var lw) ||
            !person.TryGet(JointId.RightWrist, out var rw))
        {
            // ����������L�� �� �ߧY�k�s
            ResetState();
            return;
        }

        // ���ˬd�ƭȬO�_���ġ]���� confidence�^
        if (float.IsNaN(ls.z) || float.IsNaN(rs.z) || float.IsNaN(lw.z) || float.IsNaN(rw.z))
        {
            ResetState();
            return;
        }

        // �H z �����ס]�w�M SkeletonDataProcessor ����^�A�ӻH�������A�[�i�ﰾ��
        float shoulderHeight = (ls.z + rs.z) * 0.5f + shoulderOffset;
        bool leftUp = lw.z > shoulderHeight;
        bool rightUp = rw.z > shoulderHeight;
        bool valid = leftUp && rightUp;

        float now = Time.time;

        if (!valid)
        {
            // ����L�ĸ�� �� �ߧY�k�s
            if (debugLog) Debug.Log("[HandRaiseDetector] �L�ĸ�� �� �k�s");
            ResetState();
            return;
        }

        // ����o�̥N���V�O�u���ĸ�ơv
        if (_lastValidTs < 0f)
        {
            // �Ĥ@�����ĸ�ơG�u�O�ɶ��A���֥[
            _lastValidTs = now;
            _holdAccum = 0f;
        }
        else
        {
            float gap = now - _lastValidTs;

            if (gap > maxValidGap)
            {
                // ���j�Ӥ[ �� �ֿn�k�s�A�������s�}�l
                if (debugLog) Debug.Log($"[HandRaiseDetector] ���Ķ��j�W�L {maxValidGap:F2}s �� ���m�ֿn");
                _holdAccum = 0f;
            }
            else
            {
                // �b���\���j�� �� �֭p�������j
                _holdAccum += gap;
                //if (debugLog) Debug.Log($"[HandRaiseDetector] �s�򦳮Ĳֿn = {_holdAccum:F3}s");
            }

            _lastValidTs = now;
        }

        // �P�_�O�_�F��
        if (!_hasTriggered && _holdAccum >= requiredHoldSeconds)
        {
            _hasTriggered = true;
            if (debugLog) Debug.Log("[HandRaiseDetector] �s�򦳮ĹF�� �� Ĳ�o OnHandsRaised");
            OnHandsRaised?.Invoke();
        }
    }
}
