// SkeletonDataProcessor.cs (安全修正版 - 多人獨立 HandSmoother 版本)
using PoseTypes; // JointId / FrameSample / PersonSkeleton
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SkeletonDataProcessor : MonoBehaviour
{
    [Header("可視化")]
    public GameObject jointPrefab;
    public Transform skeletonParent;
    public Vector3 jointScale = Vector3.one;

    [Header("座標轉換（資料 -> 本地座標）")]
    public Vector3 positionScale = Vector3.one;
    public Vector3 positionOffset = Vector3.zero;

    [Header("顯示條件")]
    public bool hideWhenLowConfidence = false;
    public float minConfidence = 0f;

    [Header("Console 列印")]
    public bool enableConsoleLog = true;
    public bool logOnlyWhenSomeonePresent = true;

    [Header("Quad 分類（擇一或並用）")]
    [SerializeField] private string leftTag = "LeftQuad";
    [SerializeField] private string rightTag = "RightQuad";
    [SerializeField] private Collider leftCollider;
    [SerializeField] private Collider rightCollider;
    private enum QuadType { None, left, right }

    [SerializeField] private LayerMask canvasLayer;
    [SerializeField] private float rayLength;

    [Header("骨架頻率統計")]
    [SerializeField] private bool logFpsEachSecond = true;
    [SerializeField] private bool logOnlyWhenValid = true;

    private int _recvFramesThisSec = 0;
    private int _validFramesThisSec = 0;
    private float _fpsWindowStart = 0f;
    private float _lastValidFrameTime = -1f;
    private readonly List<float> _validIntervals = new List<float>();

    // 【舊】共用 Smoother（多人會混合）
    // private HandSmoother leftHandSmoother = new HandSmoother(0.2f, 0.002f);
    // private HandSmoother rightHandSmoother = new HandSmoother(0.2f, 0.002f);

    // 【新增】每人左右手獨立平滑器
    private readonly Dictionary<int, HandSmoother> leftSmoothers = new();
    private readonly Dictionary<int, HandSmoother> rightSmoothers = new();

    [SerializeField] private MultiHoleController holeController;
    [SerializeField] private SimpleHoleController simpleHoleController;

    class SkeletonVisual
    {
        public int personId;
        public GameObject root;
        public Transform[] joints = new Transform[PoseSchema.JointCount];
        public Renderer[] renderers = new Renderer[PoseSchema.JointCount];
    }

    private readonly Dictionary<int, SkeletonVisual> visuals = new Dictionary<int, SkeletonVisual>();
    private readonly List<int> _tmpToRemove = new List<int>();

    public void HandleSkeletonFrame(FrameSample frame)
    {
        if (frame == null || frame.persons == null)
            return;

        var seen = new HashSet<int>();
        var hitList = new List<Vector2>();

        _recvFramesThisSec++;
        bool anyPerson = frame.persons.Count > 0;
        if (anyPerson)
        {
            _validFramesThisSec++;
            if (_lastValidFrameTime > 0f)
                _validIntervals.Add(Time.time - _lastValidFrameTime);
            _lastValidFrameTime = Time.time;
        }

        if (logFpsEachSecond && Time.time - _fpsWindowStart >= 1f)
        {
            _validIntervals.Clear();
            _fpsWindowStart += 1f;
            _recvFramesThisSec = 0;
            _validFramesThisSec = 0;
        }

        for (int p = 0; p < frame.persons.Count; p++)
        {
            var person = frame.persons[p];
            if (person == null || person.joints == null || person.joints.Length < PoseSchema.JointCount)
                continue;

            seen.Add(p);

            if (!visuals.TryGetValue(p, out var vis))
            {
                vis = CreateVisualForPerson(p);
                visuals.Add(p, vis);
            }

            // 【新增】確保每個人都有獨立 smoother
            if (!leftSmoothers.TryGetValue(p, out var smootherL))
                leftSmoothers[p] = smootherL = new HandSmoother(0.2f, 0.002f);
            if (!rightSmoothers.TryGetValue(p, out var smootherR))
                rightSmoothers[p] = smootherR = new HandSmoother(0.2f, 0.002f);

            for (int j = 0; j < PoseSchema.JointCount; j++)
            {
                var data = person.joints[j];
                Vector3 pos = new Vector3(
                    data.x * positionScale.x,
                    data.z * positionScale.z,
                    data.y * positionScale.y
                ) + positionOffset;
                vis.joints[j].localPosition = pos;

                var r = vis.renderers[j];
                if (r != null)
                    r.enabled = !hideWhenLowConfidence || data.conf > minConfidence;
            }

            if (vis != null)
            {
                if (person.TryGet(JointId.LeftHip, out var leftHip) &&
                    person.TryGet(JointId.RightHip, out var rightHip))
                {
                    float hipZ = ((leftHip.z + rightHip.z) / 2f) * 0f;
                    var lw = person.joints[(int)JointId.LeftWrist];
                    var rw = person.joints[(int)JointId.RightWrist];

                    // 左手
                    if (lw.z > hipZ)
                    {
                        List<Vector2> uvResults = new();
                        List<QuadType> quadResults = new();
                        int hits = TryGetWristUVs(vis.joints[(int)JointId.LeftWrist], uvResults, quadResults);
                        for (int i = 0; i < hits; i++)
                        {
                            var quad = quadResults[i];
                            if (!TryConvertToFullUV(uvResults[i], quad, out var fullUV))
                                continue;

                            // 【舊】共用平滑器（會混合）
                            // var uv = leftHandSmoother.Smooth(fullUV);

                            // 【新】使用該人物專屬 smoother
                            var uv = smootherL.Smooth(fullUV);

                            hitList.Add(uv);
                        }
                    }

                    // 右手
                    if (rw.z > hipZ)
                    {
                        List<Vector2> uvResults = new();
                        List<QuadType> quadResults = new();
                        int hits = TryGetWristUVs(vis.joints[(int)JointId.RightWrist], uvResults, quadResults);
                        for (int i = 0; i < hits; i++)
                        {
                            var quad = quadResults[i];
                            if (!TryConvertToFullUV(uvResults[i], quad, out var fullUV))
                                continue;

                            // 【舊】共用平滑器（會混合）
                            // var uv = rightHandSmoother.Smooth(fullUV);

                            // 【新】使用該人物專屬 smoother
                            var uv = smootherR.Smooth(fullUV);

                            hitList.Add(uv);
                        }
                    }
                }
            }
        }

        PruneMissingPersons(seen);

        if (hitList.Count > 0)
        {
            hitList.Sort((a, b) => a.x.CompareTo(b.x));
            simpleHoleController.UpdateHoleCenters(hitList);

            StringBuilder sb = new StringBuilder("[HitList Sorted] ");
            foreach (var uv in hitList)
                sb.Append($"({uv.x:F3},{uv.y:F3}) ");
            Debug.Log(sb.ToString());
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
            GameObject go = jointPrefab != null
                ? Instantiate(jointPrefab, vis.root.transform)
                : GameObject.CreatePrimitive(PrimitiveType.Sphere);

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

            // 【新增】人物離場時清除其 smoother
            leftSmoothers.Remove(id);
            rightSmoothers.Remove(id);
        }
    }

    // 射線相關函式維持不變
    private int TryGetWristUVs(Transform wrist, List<Vector2> uvs, List<QuadType> quads)
    {
        if (wrist == null) return 0;
        int hitCount = 0;
        RaycastHit hit;
        Vector3 dirNegX, dirNegZ;

        if (skeletonParent != null)
        {
            dirNegX = skeletonParent.TransformDirection(Vector3.left);
            dirNegZ = skeletonParent.TransformDirection(Vector3.back);
        }
        else
        {
            dirNegX = -wrist.right;
            dirNegZ = -wrist.forward;
        }

        Ray rayZ = new Ray(wrist.position, dirNegZ.normalized);
        if (Physics.Raycast(rayZ, out hit, rayLength))
        {
            uvs.Add(hit.textureCoord);
            quads.Add(ClassifyQuad(hit.collider));
            Debug.DrawRay(rayZ.origin, rayZ.direction * hit.distance, Color.cyan, 0.1f);
            hitCount++;
        }

        Ray rayX = new Ray(wrist.position, dirNegX.normalized);
        if (Physics.Raycast(rayX, out hit, rayLength))
        {
            uvs.Add(hit.textureCoord);
            quads.Add(ClassifyQuad(hit.collider));
            Debug.DrawRay(rayX.origin, rayX.direction * hit.distance, Color.green, 0.1f);
            hitCount++;
        }

        if (hitCount == 0)
        {
            Debug.DrawRay(wrist.position, dirNegZ.normalized * rayLength, Color.red, 0.1f);
            Debug.DrawRay(wrist.position, dirNegX.normalized * rayLength, Color.red, 0.1f);
        }

        return hitCount;
    }

    private QuadType ClassifyQuad(Collider col)
    {
        if (col == null) return QuadType.None;
        if (leftCollider && col == leftCollider) return QuadType.left;
        if (rightCollider && col == rightCollider) return QuadType.right;
        return QuadType.None;
    }

    private bool TryConvertToFullUV(Vector2 uv, QuadType quad, out Vector2 fullUV)
    {
        switch (quad)
        {
            case QuadType.left:
                fullUV = new Vector2(uv.x * 0.5f, uv.y);
                return true;
            case QuadType.right:
                fullUV = new Vector2(0.5f + uv.x * 0.5f, uv.y);
                return true;
            default:
                fullUV = default;
                return false;
        }
    }
}
