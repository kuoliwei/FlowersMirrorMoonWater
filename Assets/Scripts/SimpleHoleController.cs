using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SimpleHoleController : MonoBehaviour
{
    [Header("Shader 材質")]
    [SerializeField] private Material mat;

    [Header("邊緣波形設定")]
    [SerializeField] private float[] freqs = { 8f, 12.3f, 21.7f, 33.1f };
    [SerializeField] private float[] amps = { 0.03f, 0.015f, 0.007f, 0.01f };

    [SerializeField] private bool autoPulse = false;   // 是否自動脈動

    [Header("圓洞半徑控制")]
    [Range(0.0f, 0.2f)]
    //[SerializeField] private float radius = 0.02f;     // 洞的半徑大小（滑桿控制）
    [SerializeField] private float maxRadius = 0.2f;


    [Range(0.0f, 10.0f)]
    [SerializeField] private float radiusSpeed = 1.5f; // 自動脈動速度（滑桿控制）

    [Range(0.0f, 1.0f)]
    [SerializeField] private float holeCenterX = 0.5f; // 圓洞中心位置 X軸（滑桿控制）
    [Range(0.0f, 1.0f)]
    [SerializeField] private float holeCenterY = 0.5f; // 圓洞中心位置 Y軸（滑桿控制）

    //[Header("震幅控制")]
    //[Range(0.0f, 1.0f)]
    //[SerializeField] private float amplitude = 0.4f;   // 振幅
    //[Range(0.0f, 5.0f)]
    //[SerializeField] private float frequency = 1.5f;   // 頻率（每秒循環次數）
    //[Range(0.0f, 6.28318f)]
    //[SerializeField] private float phase = 0f;         // 相位偏移（例如加 π/2 會讓波從頂點開始）
    
    [Header("半徑變化因子")]
    private float radiusFactor = 0;

    [Header("半徑變化因子變化速度")]
    [Range(0.1f, 1.0f)]
    [SerializeField] private float radiusFactorSpeed = 0.5f;

    [Header("外擴速度變因控制")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float amplitude = 0.4f;   // 振幅
    [Range(0.0f, 5.0f)]
    [SerializeField] private float frequency = 1.5f;   // 頻率（每秒循環次數）
    [Range(0.0f, 6.28318f)]
    [SerializeField] private float phase = 0f;         // 相位偏移
    void Start()
    {
        SetMatParameter(Mathf.Lerp(0.0f, maxRadius, radiusFactor));
        //baseRadius = radius;
    }

    void Update()
    {
        ////// 更新圓洞中心位置（滑鼠位置轉換為 UV）
        ////Vector3 mousePos = Input.mousePosition;
        ////Vector2 uv = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
        ////mat.SetVector("_HoleCenter", new Vector4(uv.x, uv.y, 0, 0));
        //float tValueForRadiu = Mathf.Clamp01(Mathf.InverseLerp(0.0f, 0.1f, radius));
        ////float wave = amplitude * Mathf.Sin(Time.time * frequency * Mathf.PI * 2f + phase);
        //List<float> finalAmps = new List<float>();
        //{
        //    foreach(float amp in amps)
        //    {
        //        finalAmps.Add(amp * Mathf.Lerp(0f, 0.4f, tValueForRadiu));
        //    }
        //}
        //mat.SetVector("_HoleCenter", new Vector4(holeCenterX, holeCenterY, 0, 0));

        //// 傳入混頻參數
        //mat.SetInt("_EdgeCount", Mathf.Min(freqs.Length, amps.Length));
        //mat.SetFloatArray("_EdgeFreqs", freqs);
        //mat.SetFloatArray("_EdgeAmps", finalAmps);

        //// 控制圓洞半徑
        //if (autoPulse)
        //{
        //    // 使用正弦波讓半徑在 [0,1] 範圍內上下擺動
        //    float t = Mathf.Sin(Time.time * radiusSpeed) * 0.5f + 0.5f;
        //    radius = t;
        //}

        //mat.SetFloat("_HoleRadius", radius);

        if (Input.GetKey(KeyCode.KeypadPlus))
        {
            radiusFactor = Mathf.Clamp01(radiusFactor + radiusFactorSpeed * Time.deltaTime);
            SetMatParameter(Mathf.Lerp(0.0f,0.2f, radiusFactor));
        }

        if (Input.GetKey(KeyCode.KeypadMinus))
        {
            radiusFactor = Mathf.Clamp01(radiusFactor - radiusFactorSpeed * Time.deltaTime);
            SetMatParameter(Mathf.Lerp(0.0f, 0.2f, radiusFactor));
        }
    }
    public void SetMatParameter(float radius)
    {
        //// 更新圓洞中心位置（滑鼠位置轉換為 UV）
        //Vector3 mousePos = Input.mousePosition;
        //Vector2 uv = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
        //mat.SetVector("_HoleCenter", new Vector4(uv.x, uv.y, 0, 0));
        float tValueForRadiu = Mathf.Clamp01(Mathf.InverseLerp(0.0f, 0.1f, radius));
        //float wave = amplitude * Mathf.Sin(Time.time * frequency * Mathf.PI * 2f + phase);
        List<float> finalAmps = new List<float>();
        {
            foreach (float amp in amps)
            {
                finalAmps.Add(amp * Mathf.Lerp(0f, 0.4f, tValueForRadiu));
            }
        }
        mat.SetVector("_HoleCenter", new Vector4(holeCenterX, holeCenterY, 0, 0));

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
