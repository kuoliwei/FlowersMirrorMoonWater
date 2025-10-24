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

    // ===========================
    // [NEW] Quad 分類設定
    // 你可以用「Tag」來分辨是哪一塊 Quad（推薦）；
    // 若專案尚未設 Tag，也可在 Inspector 指定對應的 Collider 作為備援。
    // ===========================
    [Header("Quad 分類（擇一或並用）")]
    [SerializeField] private string leftTag = "LeftQuad";
    [SerializeField] private string rightTag = "RightQuad";
    [SerializeField] private Collider leftCollider;
    [SerializeField] private Collider rightCollider;
    // [NEW] 命中哪一塊的列舉
    private enum QuadType { None, left, right }

    [SerializeField] private LayerMask canvasLayer; // 指定 Quad (畫布) 的 layer
    [SerializeField] private float rayLength;  // Ray 最長距離

    [Header("骨架頻率統計")]
    [SerializeField] private bool logFpsEachSecond = true;
    [SerializeField] private bool logOnlyWhenValid = true;

    // 內部統計
    private int _recvFramesThisSec = 0;
    private int _validFramesThisSec = 0;
    private float _fpsWindowStart = 0f;
    private float _lastValidFrameTime = -1f;
    private readonly List<float> _validIntervals = new List<float>();

    private HandSmoother leftHandSmoother = new HandSmoother(0.2f, 0.002f);
    private HandSmoother rightHandSmoother = new HandSmoother(0.2f, 0.002f);

    [SerializeField] private MultiHoleController holeController;
    [SerializeField] private SimpleHoleController simpleHoleController;

    // 可視化資料
    class SkeletonVisual
    {
        public int personId;
        public GameObject root;
        public Transform[] joints = new Transform[PoseSchema.JointCount];
        public Renderer[] renderers = new Renderer[PoseSchema.JointCount];
    }

    private readonly Dictionary<int, SkeletonVisual> visuals = new Dictionary<int, SkeletonVisual>();
    private readonly List<int> _tmpToRemove = new List<int>();

    [SerializeField] private Vector2[] holeCentersForTest;
    private void Update()
    {
        //if (Input.GetMouseButton(0))
        //{
        //    simpleHoleController.UpdateHoleCenters(holeCentersForTest.ToList<Vector2>());
        //}
    }
    /// <summary>
    /// 接收一幀骨架資料：更新/建立/刪除可視化，並可選擇列印資訊。
    /// </summary>
    public void HandleSkeletonFrame(FrameSample frame)
    {
        if (frame == null || frame.persons == null)
            return;

        var seen = new HashSet<int>();
        var hitList = new List<Vector2>(); // 統一的 32:10 UV 集合

        // 統計接收幀
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

        // 每秒印出 FPS 資訊
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

        // 更新骨架顯示
        for (int p = 0; p < frame.persons.Count; p++)
        {
            var person = frame.persons[p];
            if (person == null || person.joints == null || person.joints.Length < PoseSchema.JointCount)
                continue;

            seen.Add(p);

            // 沒有就建立可視化
            if (!visuals.TryGetValue(p, out var vis))
            {
                vis = CreateVisualForPerson(p);
                visuals.Add(p, vis);
            }

            StringBuilder sb = enableConsoleLog ? new StringBuilder() : null;
            if (enableConsoleLog)
                sb.AppendLine($"[Pose] frame={frame.frameIndex} person={p} joints:");

            // 逐關節更新位置
            for (int j = 0; j < PoseSchema.JointCount; j++)
            {
                var data = person.joints[j];
                Vector3 pos = new Vector3(
                    data.x * positionScale.x,
                    data.z * positionScale.z,
                    data.y * positionScale.y
                ) + positionOffset;

                // 一律使用 SkeletonParent 的本地座標
                vis.joints[j].localPosition = pos;

                // 顯示/隱藏
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
            if (vis != null)
            {
                // 先取出左右髖關節，計算腰部高度
                if (person.TryGet(JointId.LeftHip, out var leftHip) &&
                    person.TryGet(JointId.RightHip, out var rightHip))
                {
                    //float hipY = (leftHip.y + rightHip.y) / 2f; // 腰部基準高度
                    float hipZ = ((leftHip.z + rightHip.z) / 2f) * 0f;// 基準高度上調 1.2 倍

                    // 取左右手腕
                    var lw = person.joints[(int)JointId.LeftWrist];
                    var rw = person.joints[(int)JointId.RightWrist];
                    Debug.Log($"左手高度：{lw.z}，右手高度：{rw.z}，骨盆高度{hipZ}");
                    // 只有手腕高於腰部才允許射線流程
                    if (lw.z > hipZ)
                    {
                        List<Vector2> uvResults = new List<Vector2>();
                        List<QuadType> quadResults = new List<QuadType>();

                        int hits = TryGetWristUVs(vis.joints[(int)JointId.LeftWrist], uvResults, quadResults);
                        for (int i = 0; i < hits; i++)
                        {
                            var quad = quadResults[i];
                            // 只接受 left / right，其他全部略過
                            if (!TryConvertToFullUV(uvResults[i], quad, out var fullUV))
                                continue;

                            var uv = leftHandSmoother.Smooth(fullUV);
                            hitList.Add(uv);
                            //hitList.Add(ConvertToFullUV(uv, quadResults[i]));
                        }
                        //if (TryGetWristUV(vis.joints[(int)JointId.LeftWrist], out var uvL, out var quadL))
                        //{
                        //    uvL = leftHandSmoother.Smooth(uvL);   // 平滑處理
                        //    hitList.Add(ConvertToFullUV(uvL, quadL));
                        //}
                    }
                    if (rw.z > hipZ)
                    {
                        List<Vector2> uvResults = new List<Vector2>();
                        List<QuadType> quadResults = new List<QuadType>();

                        int hits = TryGetWristUVs(vis.joints[(int)JointId.RightWrist], uvResults, quadResults);
                        for (int i = 0; i < hits; i++)
                        {
                            var quad = quadResults[i];
                            // 只接受 left / right，其他全部略過
                            if (!TryConvertToFullUV(uvResults[i], quad, out var fullUV))
                                continue;

                            var uv = leftHandSmoother.Smooth(fullUV);
                            hitList.Add(uv);
                            //hitList.Add(ConvertToFullUV(uv, quadResults[i]));
                        }
                        //if (TryGetWristUV(vis.joints[(int)JointId.RightWrist], out var uvR, out var quadR))
                        //{
                        //    uvR = rightHandSmoother.Smooth(uvR);  // 平滑處理
                        //    hitList.Add(ConvertToFullUV(uvR, quadR));
                        //}
                    }
                }
            }
            // if (enableConsoleLog)
            //     Debug.Log(sb.ToString());
        }

        // 移除消失的人
        PruneMissingPersons(seen);

        // 輸出測試：顯示整合後的 hitList
        if (hitList.Count > 0)
        {
            //holeController.HandleHoleData(hitList.ToArray());
            //simpleHoleController.SetHolePosition(hitList[0]);
            simpleHoleController.UpdateHoleCenters(hitList);
            StringBuilder sb = new StringBuilder("[HitList] ");
            foreach (var uv in hitList)
                sb.Append($"({uv.x:F3},{uv.y:F3}) ");
            Debug.Log(sb.ToString());
        }
        // 計算延遲
        float delay = (Time.realtimeSinceStartup - frame.recvTime) * 1000f;
        DateTime now = DateTime.Now;
        string timeStr = $"{now:HH:mm}:{now.Second + now.Millisecond / 1000.0:F6}";
        //Debug.Log($"[Latency] Frame {frame.frameIndex} 顯示延遲 = {delay:F1} ms\n[Time] 收到時間 {timeStr}");

        if (enableConsoleLog && !anyPerson && !logOnlyWhenSomeonePresent)
        {
            Debug.Log($"[Pose] frame={frame.frameIndex} 無人物資料。");
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
    // ===========================
    // 命中 → 取得 UV + 分類（left / right）
    // ===========================
    private bool TryGetWristUV(Transform wrist, out Vector2 uv, out QuadType quad) // [signature]
    {
        uv = default;
        quad = QuadType.None;
        if (wrist == null) return false;

        Ray ray = new Ray(wrist.position, wrist.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, canvasLayer))
        {
            uv = hit.textureCoord; // 0..1，左下為原點（Unity 的 Texture 座標系）
            quad = ClassifyQuad(hit.collider);
            Debug.DrawRay(ray.origin, ray.direction * hit.distance,
                          quad == QuadType.left ? Color.green :
                          quad == QuadType.right ? Color.cyan : Color.yellow, 0.1f);
            return true;
        }
        Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red, 0.1f);
        return false;
    }
    // ============================================================
    // 命中偵測 (雙方向) - 同時射出 +Z 與 -X 兩條射線
    // 若兩條皆命中會輸出兩組 UV
    // ============================================================
    // 命中偵測 (雙方向) - 方向以 SkeletonParent 為基準: -X 與 -Z
    private int TryGetWristUVs(Transform wrist, List<Vector2> uvs, List<QuadType> quads)
    {
        if (wrist == null) return 0;

        int hitCount = 0;
        RaycastHit hit;

        // 以 SkeletonParent 為座標基準取方向
        Vector3 dirNegX, dirNegZ;
        if (skeletonParent != null)
        {
            // -X 與 -Z 方向皆以 skeletonParent 為基準
            dirNegX = skeletonParent.TransformDirection(Vector3.left);   // -X
            dirNegZ = skeletonParent.TransformDirection(Vector3.back);   // -Z
        }
        else
        {
            // 備援: 若未指定 skeletonParent, 退回 wrist 本身的軸向
            dirNegX = -wrist.right;
            dirNegZ = -wrist.forward;
        }

        // 射線 1: 朝 -Z
        Ray rayZ = new Ray(wrist.position, dirNegZ.normalized);
        if (Physics.Raycast(rayZ, out hit, rayLength))
        {
            uvs.Add(hit.textureCoord);
            quads.Add(ClassifyQuad(hit.collider));
            Debug.DrawRay(rayZ.origin, rayZ.direction * hit.distance, Color.cyan, 0.1f);
            hitCount++;
        }

        // 射線 2: 朝 -X
        Ray rayX = new Ray(wrist.position, dirNegX.normalized);
        if (Physics.Raycast(rayX, out hit, rayLength))
        {
            uvs.Add(hit.textureCoord);
            quads.Add(ClassifyQuad(hit.collider));
            Debug.DrawRay(rayX.origin, rayX.direction * hit.distance, Color.green, 0.1f);
            hitCount++;
        }

        // 未命中時畫紅線以利除錯
        if (hitCount == 0)
        {
            Debug.DrawRay(wrist.position, dirNegZ.normalized * rayLength, Color.red, 0.1f);
            Debug.DrawRay(wrist.position, dirNegX.normalized * rayLength, Color.red, 0.1f);
        }

        return hitCount;
    }


    // 僅使用指定 Collider 進行分類
    private QuadType ClassifyQuad(Collider col)
    {
        if (col == null) return QuadType.None;

        if (leftCollider && col == leftCollider) return QuadType.left;
        if (rightCollider && col == rightCollider) return QuadType.right;

        return QuadType.None;
    }
    private Vector2 ConvertToFullUV(Vector2 uv, QuadType quad) // [NEW]
    {
        switch (quad)
        {
            case QuadType.left:
                return new Vector2(uv.x * 0.5f, uv.y);
            case QuadType.right:
                return new Vector2(0.5f + uv.x * 0.5f, uv.y);
            default:
                return uv;
        }
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
