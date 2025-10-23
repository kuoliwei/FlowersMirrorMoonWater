using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �h�}��������G�i�P����ܦh�Ӷ�}
/// ���� SkeletonDataProcessor �� hitList (List<Vector2>)�C
/// </summary>
public class MultiHoleController : MonoBehaviour
{
    [Header("Shader ����")]
    [SerializeField] private Material mat;

    [Header("��t�i�γ]�w")]
    [SerializeField] private float[] freqs;
    [SerializeField] private float[] amps;
    [SerializeField] private float[] currentAmps;

    [Header("�_�T�ܤƳt��")]
    [SerializeField] private float[] ampVarSpeeds;

    [Header("�_�T�ܤƳt�צ]�l")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float ampVarSpeedFactor;

    [SerializeField] private bool autoPulse = false;   // �O�_�۰ʯ߰�

    [Header("��}�b�|����")]
    [Range(0.0f, 0.5f)]
    [SerializeField] private float maxRadius;    // �}���b�|�̤j�� (�Ʊ챱��^
    [SerializeField] private float currentRadius;     // ��e�}���b�|�j�p


    [Range(0.0f, 10.0f)]
    [SerializeField] private float radiusSpeed; // �۰ʯ߰ʳt�ס]�Ʊ챱��^

    [Range(0.0f, 1.0f)]
    [SerializeField] private float holeCenterX; // ��}���ߦ�m X�b�]�Ʊ챱��^
    [Range(0.0f, 1.0f)]
    [SerializeField] private float holeCenterY; // ��}���ߦ�m Y�b�]�Ʊ챱��^

    [SerializeField] private List<float> holeCenterList; // ��}���ߦ�m Y�b�]�Ʊ챱��^

    [Header("�b�|�ܤƦ]�l")]
    private float radiusFactor;
    [Header("�b�|�ܤƦ]�l�C��")]
    private float radiusFactorList;

    [Header("�b�|�ܤƦ]�l�ܤƳt��")]
    [Range(0.1f, 1.0f)]
    [SerializeField] private float radiusFactorSpeed;

    [Header("�~�X�t���ܦ]����")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float amplitude = 0.4f;   // ���T
    [Range(0.0f, 5.0f)]
    [SerializeField] private float frequency = 1.5f;   // �W�v�]�C��`�����ơ^
    [Range(0.0f, 6.28318f)]
    [SerializeField] private float phase = 0f;         // �ۦ찾��

    // -----------------------------
    // �s�W�G�W�@�V�B�n�Ȧs���
    // -----------------------------
    [Header("RenderTexture �Ȧs�]�w")]
    [SerializeField] private int textureWidth = 1920;
    [SerializeField] private int textureHeight = 1080;

    [SerializeField] private RenderTexture maskA; // ���V�g�J
    [SerializeField] private RenderTexture maskB; // �W�@�V�Ȧs
    private bool initialized = false;

    void Start()
    {
        InitializeRenderTextures_AB(); // �T�O�إ� RenderTexture
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
        // �ǤJ�W�@�V�B�n�� Shader
        mat.SetTexture("_PrevMaskTex", maskB);

        // ��ثe���}���G�g�i maskA
        Graphics.Blit(null, maskA, mat);

        // �洫 maskA�BmaskB�]Ping-Pong�^
        var temp = maskA;
        maskA = maskB;
        maskB = temp;

        SetMatParameter(Mathf.Lerp(0.0f, maxRadius, radiusFactor));
    }
    // ��l���� RenderTexture
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

        // �M�����ա]��ܨS���}�^
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
        //// ��s��}���ߦ�m�]�ƹ���m�ഫ�� UV�^
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

        // �ǤJ�V�W�Ѽ�
        mat.SetInt("_EdgeCount", Mathf.Min(freqs.Length, amps.Length));
        mat.SetFloatArray("_EdgeFreqs", freqs);
        mat.SetFloatArray("_EdgeAmps", finalAmps);

        // �����}�b�|
        if (autoPulse)
        {
            // �ϥΥ����i���b�|�b [0,1] �d�򤺤W�U�\��
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
    //// ��ʳ]�w�b�|
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
