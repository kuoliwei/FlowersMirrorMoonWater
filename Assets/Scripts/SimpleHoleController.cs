using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SimpleHoleController : MonoBehaviour
{
    [Header("Shader ����")]
    [SerializeField] private Material mat;

    [Header("��t�i�γ]�w")]
    [SerializeField] private float[] freqs = { 8f, 12.3f, 21.7f, 33.1f };
    [SerializeField] private float[] amps = { 0.03f, 0.015f, 0.007f, 0.01f };

    [SerializeField] private bool autoPulse = false;   // �O�_�۰ʯ߰�

    [Header("��}�b�|����")]
    [Range(0.0f, 0.2f)]
    //[SerializeField] private float radius = 0.02f;     // �}���b�|�j�p�]�Ʊ챱��^
    [SerializeField] private float maxRadius = 0.2f;


    [Range(0.0f, 10.0f)]
    [SerializeField] private float radiusSpeed = 1.5f; // �۰ʯ߰ʳt�ס]�Ʊ챱��^

    [Range(0.0f, 1.0f)]
    [SerializeField] private float holeCenterX = 0.5f; // ��}���ߦ�m X�b�]�Ʊ챱��^
    [Range(0.0f, 1.0f)]
    [SerializeField] private float holeCenterY = 0.5f; // ��}���ߦ�m Y�b�]�Ʊ챱��^

    //[Header("�_�T����")]
    //[Range(0.0f, 1.0f)]
    //[SerializeField] private float amplitude = 0.4f;   // ���T
    //[Range(0.0f, 5.0f)]
    //[SerializeField] private float frequency = 1.5f;   // �W�v�]�C��`�����ơ^
    //[Range(0.0f, 6.28318f)]
    //[SerializeField] private float phase = 0f;         // �ۦ찾���]�Ҧp�[ �k/2 �|���i�q���I�}�l�^
    
    [Header("�b�|�ܤƦ]�l")]
    private float radiusFactor = 0;

    [Header("�b�|�ܤƦ]�l�ܤƳt��")]
    [Range(0.1f, 1.0f)]
    [SerializeField] private float radiusFactorSpeed = 0.5f;

    [Header("�~�X�t���ܦ]����")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float amplitude = 0.4f;   // ���T
    [Range(0.0f, 5.0f)]
    [SerializeField] private float frequency = 1.5f;   // �W�v�]�C��`�����ơ^
    [Range(0.0f, 6.28318f)]
    [SerializeField] private float phase = 0f;         // �ۦ찾��
    void Start()
    {
        SetMatParameter(Mathf.Lerp(0.0f, maxRadius, radiusFactor));
        //baseRadius = radius;
    }

    void Update()
    {
        ////// ��s��}���ߦ�m�]�ƹ���m�ഫ�� UV�^
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

        //// �ǤJ�V�W�Ѽ�
        //mat.SetInt("_EdgeCount", Mathf.Min(freqs.Length, amps.Length));
        //mat.SetFloatArray("_EdgeFreqs", freqs);
        //mat.SetFloatArray("_EdgeAmps", finalAmps);

        //// �����}�b�|
        //if (autoPulse)
        //{
        //    // �ϥΥ����i���b�|�b [0,1] �d�򤺤W�U�\��
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
        //// ��s��}���ߦ�m�]�ƹ���m�ഫ�� UV�^
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
