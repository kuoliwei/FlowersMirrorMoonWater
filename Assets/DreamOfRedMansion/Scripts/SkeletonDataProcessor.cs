using PoseTypes; // JointId / FrameSample / PersonSkeleton
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SkeletonDataProcessor : MonoBehaviour
{
    [Header("�i����")]
    public GameObject jointPrefab;
    public Transform skeletonParent;
    public Vector3 jointScale = Vector3.one;

    [Header("�y���ഫ�]��� -> ���a�y�С^")]
    public Vector3 positionScale = Vector3.one;
    public Vector3 positionOffset = Vector3.zero;

    [Header("��ܱ���")]
    public bool hideWhenLowConfidence = false;
    public float minConfidence = 0f;

    [Header("Console �C�L")]
    public bool enableConsoleLog = true;
    public bool logOnlyWhenSomeonePresent = true;

    [Header("���[�W�v�έp")]
    [SerializeField] private bool logFpsEachSecond = true;
    [SerializeField] private bool logOnlyWhenValid = true;

    // �����έp
    private int _recvFramesThisSec = 0;
    private int _validFramesThisSec = 0;
    private float _fpsWindowStart = 0f;
    private float _lastValidFrameTime = -1f;
    private readonly List<float> _validIntervals = new List<float>();

    // �i���Ƹ��
    class SkeletonVisual
    {
        public int personId;
        public GameObject root;
        public Transform[] joints = new Transform[PoseSchema.JointCount];
        public Renderer[] renderers = new Renderer[PoseSchema.JointCount];
    }

    private readonly Dictionary<int, SkeletonVisual> visuals = new Dictionary<int, SkeletonVisual>();
    private readonly List<int> _tmpToRemove = new List<int>();

    /// <summary>
    /// �����@�V���[��ơG��s/�إ�/�R���i���ơA�åi��ܦC�L��T�C
    /// </summary>
    public void HandleSkeletonFrame(FrameSample frame)
    {
        if (frame == null || frame.persons == null)
            return;

        var seen = new HashSet<int>();

        // �έp�����V
        _recvFramesThisSec++;

        bool anyPerson = frame.persons.Count > 0;
        if (anyPerson)
        {
            _validFramesThisSec++;

            if (_lastValidFrameTime > 0f)
            {
                float interval = Time.time - _lastValidFrameTime;
                _validIntervals.Add(interval);
            }
            _lastValidFrameTime = Time.time;
        }

        // �C��L�X FPS ��T
        if (logFpsEachSecond && Time.time - _fpsWindowStart >= 1f)
        {
            if (!logOnlyWhenValid || _validFramesThisSec > 0)
            {
                string intervalsStr = _validIntervals.Count > 0
                    ? string.Join(", ", _validIntervals.Select(v => v.ToString("F2")))
                    : "N/A";

                // Debug.Log($"[Pose/FPS] recv={_recvFramesThisSec}/s, valid={_validFramesThisSec}/s, intervals=[{intervalsStr}]");
            }

            _validIntervals.Clear();
            _fpsWindowStart += 1f;
            _recvFramesThisSec = 0;
            _validFramesThisSec = 0;
        }

        // ��s���[���
        for (int p = 0; p < frame.persons.Count; p++)
        {
            var person = frame.persons[p];
            if (person == null || person.joints == null || person.joints.Length < PoseSchema.JointCount)
                continue;

            seen.Add(p);

            // �S���N�إߥi����
            if (!visuals.TryGetValue(p, out var vis))
            {
                vis = CreateVisualForPerson(p);
                visuals.Add(p, vis);
            }

            StringBuilder sb = enableConsoleLog ? new StringBuilder() : null;
            if (enableConsoleLog)
                sb.AppendLine($"[Pose] frame={frame.frameIndex} person={p} joints:");

            // �v���`��s��m
            for (int j = 0; j < PoseSchema.JointCount; j++)
            {
                var data = person.joints[j];
                Vector3 pos = new Vector3(
                    data.x * positionScale.x,
                    data.z * positionScale.z,
                    data.y * positionScale.y
                ) + positionOffset;

                // �@�ߨϥ� SkeletonParent �����a�y��
                vis.joints[j].localPosition = pos;

                // ���/����
                var r = vis.renderers[j];
                if (r != null)
                {
                    if (hideWhenLowConfidence)
                        r.enabled = (data.conf > minConfidence);
                    else
                        r.enabled = true;
                }

                if (enableConsoleLog)
                {
                    string name = ((JointId)j).ToString();
                    sb.AppendLine($"  {name,-14} => x={data.x:F3}, y={data.y:F3}, z={data.z:F3}, conf={data.conf:F2}");
                }
            }

            // if (enableConsoleLog)
            //     Debug.Log(sb.ToString());
        }

        // �����������H
        PruneMissingPersons(seen);

        // �p�⩵��
        float delay = (Time.realtimeSinceStartup - frame.recvTime) * 1000f;
        DateTime now = DateTime.Now;
        string timeStr = $"{now:HH:mm}:{now.Second + now.Millisecond / 1000.0:F6}";
        //Debug.Log($"[Latency] Frame {frame.frameIndex} ��ܩ��� = {delay:F1} ms\n[Time] ����ɶ� {timeStr}");

        if (enableConsoleLog && !anyPerson && !logOnlyWhenSomeonePresent)
        {
            Debug.Log($"[Pose] frame={frame.frameIndex} �L�H����ơC");
        }
    }

    private SkeletonVisual CreateVisualForPerson(int personId)
    {
        var vis = new SkeletonVisual { personId = personId };

        vis.root = new GameObject($"Person_{personId}");
        if (skeletonParent != null)
            vis.root.transform.SetParent(skeletonParent, worldPositionStays: false);

        for (int j = 0; j < PoseSchema.JointCount; j++)
        {
            string jointName = ((JointId)j).ToString();
            GameObject go;

            if (jointPrefab != null)
                go = Instantiate(jointPrefab, vis.root.transform);
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.SetParent(vis.root.transform, worldPositionStays: false);
            }

            go.name = $"j_{j}_{jointName}";
            go.transform.localScale = jointScale;

            vis.joints[j] = go.transform;
            vis.renderers[j] = go.GetComponent<Renderer>();
        }

        return vis;
    }

    private void PruneMissingPersons(HashSet<int> seen)
    {
        _tmpToRemove.Clear();
        foreach (var kv in visuals)
            if (!seen.Contains(kv.Key)) _tmpToRemove.Add(kv.Key);

        foreach (var id in _tmpToRemove)
        {
            var vis = visuals[id];
            if (vis != null && vis.root != null)
                Destroy(vis.root);
            visuals.Remove(id);
        }
    }
}
