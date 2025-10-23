using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 多洞版本控制器：可同時顯示多個圓洞
/// 接收 SkeletonDataProcessor 的 hitList (List<Vector2>)。
/// </summary>
public class MultiHoleController : MonoBehaviour
{
    [Header("Shader 材質")]
    [SerializeField] private Material mat;

    [Header("邊緣波形設定")]
    [SerializeField] private float[] freqs;
    [SerializeField] private float[] amps;
    [SerializeField] private float[] currentAmps;

    [Header("震幅變化速度")]
    [SerializeField] private float[] ampVarSpeeds;

    [Header("震幅變化速度因子")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float ampVarSpeedFactor;

    [SerializeField] private bool autoPulse = false;   // 是否自動脈動

    [Header("圓洞半徑控制")]
    [Range(0.0f, 0.5f)]
    [SerializeField] private float maxRadius;    // 洞的半徑最大值 (滑桿控制）
    [SerializeField] private float currentRadius;     // 當前洞的半徑大小


    [Range(0.0f, 10.0f)]
    [SerializeField] private float radiusSpeed; // 自動脈動速度（滑桿控制）

    [Range(0.0f, 1.0f)]
    [SerializeField] private float holeCenterX; // 圓洞中心位置 X軸（滑桿控制）
    [Range(0.0f, 1.0f)]
    [SerializeField] private float holeCenterY; // 圓洞中心位置 Y軸（滑桿控制）

    [SerializeField] private List<float> holeCenterList; // 圓洞中心位置 Y軸（滑桿控制）

    [Header("半徑變化因子")]
    private float radiusFactor;
    [Header("半徑變化因子列表")]
    private float radiusFactorList;

    [Header("半徑變化因子變化速度")]
    [Range(0.1f, 1.0f)]
    [SerializeField] private float radiusFactorSpeed;

    [Header("外擴速度變因控制")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float amplitude = 0.4f;   // 振幅
    [Range(0.0f, 5.0f)]
    [SerializeField] private float frequency = 1.5f;   // 頻率（每秒循環次數）
    [Range(0.0f, 6.28318f)]
    [SerializeField] private float phase = 0f;         // 相位偏移

    // -----------------------------
    // 新增：上一幀遮罩暫存欄位
    // -----------------------------
    [Header("RenderTexture 暫存設定")]
    [SerializeField] private int textureWidth = 1920;
    [SerializeField] private int textureHeight = 1080;

    [SerializeField] private RenderTexture maskA; // 本幀寫入
    [SerializeField] private RenderTexture maskB; // 上一幀暫存
    private bool initialized = false;

    void Start()
    {
        InitializeRenderTextures_AB(); // 確保建立 RenderTexture
        SetMatParameter(Mathf.Lerp(0.0f, maxRadius, radiusFactor));
        //baseRadius = radius;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            radiusFactor = Mathf.Clamp01(radiusFactor + Mathf.Lerp(1.0f, 0.0f, radiusFactor) * radiusFactorSpeed * Time.deltaTime);

        }

        if (!Input.GetMouseButton(0))
        {
            radiusFactor = Mathf.Clamp01(radiusFactor - Mathf.Lerp(0.0f, 5.0f, radiusFactor) * radiusFactorSpeed * Time.deltaTime);
        }

        //Graphics.Blit(null, renderTexture_PreMask, mat);
        // 傳入上一幀遮罩給 Shader
        mat.SetTexture("_PrevMaskTex", maskB);

        // 把目前的洞結果寫進 maskA
        Graphics.Blit(null, maskA, mat);

        // 交換 maskA、maskB（Ping-Pong）
        var temp = maskA;
        maskA = maskB;
        maskB = temp;

        SetMatParameter(Mathf.Lerp(0.0f, maxRadius, radiusFactor));
    }
    // 初始化雙 RenderTexture
    private void InitializeRenderTextures_AB()
    {
        if (initialized) return;

        if (maskA == null)
        {
            maskA = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
            maskA.filterMode = FilterMode.Bilinear;
            maskA.wrapMode = TextureWrapMode.Clamp;
            maskA.Create();
        }
        if (maskB == null)
        {
            maskB = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
            maskB.filterMode = FilterMode.Bilinear;
            maskB.wrapMode = TextureWrapMode.Clamp;
            maskB.Create();
        }

        // 清成全白（表示沒有洞）
        Graphics.SetRenderTarget(maskA);
        GL.Clear(true, true, Color.white);
        Graphics.SetRenderTarget(maskB);
        GL.Clear(true, true, Color.white);
        Graphics.SetRenderTarget(null);

        initialized = true;
    }
    public void HandleHoleData(Vector2[] holeCenters)
    {

    }
    public void SetMatParameter(float radius)
    {
        //// 更新圓洞中心位置（滑鼠位置轉換為 UV）
        Vector3 mousePos = Input.mousePosition;
        Vector2 uv = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
        mat.SetVector("_HoleCenter", new Vector4(uv.x, uv.y, 0, 0));
        float ampMul = Mathf.Clamp01(Mathf.InverseLerp(0.0f, maxRadius, radius));

        List<float> finalAmps = new List<float>();
        for (int i = 0; i < amps.Length; i++)
        {
            //float finalAmp = amps[i] * Mathf.Sin(1 * 2 * Mathf.PI * Time.time * ampVarSpeeds[i] * Mathf.Sin(ampVarSpeedFactor * 2 * Mathf.PI * Time.time * ampVarSpeedFactor));
            float finalAmp = amps[i] * Mathf.Sin(1 * 2 * Mathf.PI * Time.time * ampVarSpeeds[i]);
            finalAmps.Add(finalAmp * radius);
        }
        currentAmps = finalAmps.ToArray();
        //List<float> finalFreqs = new List<float>();
        //{
        //    foreach (float freq in freqs)
        //    {
        //        float finalFreq = freq + Time.deltaTime * freq * Mathf.Sin(1 * 2 * Mathf.PI * Time.time * Time.deltaTime);
        //        finalFreqs.Add(finalFreq);
        //    }
        //}
        //mat.SetVector("_HoleCenter", new Vector4(holeCenterX, holeCenterY, 0, 0));

        // 傳入混頻參數
        mat.SetInt("_EdgeCount", Mathf.Min(freqs.Length, amps.Length));
        mat.SetFloatArray("_EdgeFreqs", freqs);
        mat.SetFloatArray("_EdgeAmps", finalAmps);

        // 控制圓洞半徑
        if (autoPulse)
        {
            // 使用正弦波讓半徑在 [0,1] 範圍內上下擺動
            float t = Mathf.Sin(Time.time * radiusSpeed) * 0.5f + 0.5f;
            radius = t;
        }

        mat.SetFloat("_HoleRadius", radius);
        currentRadius = radius;
    }
    private void OnDestroy()
    {
        if (maskA != null) maskA.Release();
        if (maskB != null) maskB.Release();
    }
    //// 手動設定半徑
    //public void SetRadius(float r)
    //{
    //    radius = Mathf.Clamp01(r);
    //    mat.SetFloat("_HoleRadius", radius);
    //}

    //public float GetRadius()
    //{
    //    return radius;
    //}
}
