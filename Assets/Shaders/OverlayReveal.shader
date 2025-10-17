Shader "Custom/OverlayReveal"
{
    Properties
    {
        _OverlayTex ("Overlay / Fog Texture (�B�n��)", 2D) = "white" {}
        _Tint       ("Overlay Tint", Color) = (1,1,1,1)
        _BaseAlpha  ("Overlay Base Opacity", Range(0,1)) = 1.0

        _HandPos    ("Hand Position (Viewport XY)", Vector) = (0.5, 0.5, 0, 0)
        _Radius     ("Hole Radius", Float) = 0.18
        _Softness   ("Edge Softness", Float) = 0.12

        _FlowSpeed  ("Edge Flow Speed", Float) = 0.15
        _NoiseScale ("Noise Scale", Float) = 2.5
        _NoisePower ("Noise Power", Range(0,0.08)) = 0.02
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _OverlayTex;
            float4 _OverlayTex_ST;
            float4 _Tint;
            float  _BaseAlpha;

            float4 _HandPos;
            float  _Radius;
            float  _Softness;

            float  _FlowSpeed;
            float  _NoiseScale;
            float  _NoisePower;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float2 suv : TEXCOORD1; // screen-space UV (0~1)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _OverlayTex);

                // �H NDC -> viewport ����G��clip space���Hw�浹�����A��]�i�A²�ƥ� SV_POSITION ��b������ i.pos
                // �o�̧�� ComputeScreenPos ���o�ù�uv
                float4 sp = ComputeScreenPos(o.pos);
                o.suv = sp.xy / sp.w; // 0~1 �ù�UV
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // �B�n�����C��]�i�ζ����K�ϩΤ@�i�b�z���ϡ^
                fixed4 overlayCol = tex2D(_OverlayTex, i.uv) * _Tint;

                // ��¦�B�n���z���ס]�O���e���Q�\���{�ס^
                float overlayAlpha = saturate(_BaseAlpha * overlayCol.a);

                // ��}�G�Z�� + �����y����t
                float2 handUV = _HandPos.xy;     // Viewport 0~1
                float2 uvScreen = i.suv;         // �o�Ӥ������ù�UV 0~1

                float dist = distance(uvScreen, handUV);

                // ��t�y�ʾ��n�]�ϥξB�n�ϥ�����@ noise �]�i�^
                float2 flowUV = i.uv * _NoiseScale + float2(_Time.y, _Time.y * 0.5) * _FlowSpeed;
                float noise = tex2D(_OverlayTex, flowUV).r; // ²������q�D�� noise
                float distWithNoise = dist - (noise - 0.5) * 2.0 * _NoisePower;

                // �q���߳z���]0�^��~�򤣳z���]1�^
                float edge = smoothstep(_Radius, _Radius + _Softness, distWithNoise);

                // �̲� alpha�G�B�n���z���� * ��t���
                float finalAlpha = overlayAlpha * edge;

                // �C��ξB�n�C��]�i���L�G/�����^�F�z����|�ݨ���
                return fixed4(overlayCol.rgb, finalAlpha);
            }
            ENDCG
        }
    }
}
