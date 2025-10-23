Shader "Custom/SinEdgeHole_MultiHole"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HoleCenter ("Hole Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _HoleRadius ("Hole Radius", Float) = 0.25
        _Ratio ("Ratio (width/height)", Float) = 0

        _PrevMaskTex ("Previous Mask (read-only)", 2D) = "white" {}
        _MaskBlend   ("Mask Blend 0.01..0.1", Range(0.01, 0.1)) = 0.1   // ����ΨӲV�X�G�¾B�n vs ���V�s�B�n

        _Feather ("Edge Feather", Range(0, 0.2)) = 0.02
        _EdgeFlow ("Enable Flow (1=On,0=Off)", Float) = 0    // �s�W�}��
        _EdgeSpeed ("Edge Flow Speed", Float) = 1.0           // �O�d�i�ճt�v
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _PrevMaskTex;   // �� �s�W
            float _MaskBlend;         // �� �s�W
            float4 _HoleCenter;
            float4 _HoleCenters[32];
            float  _HoleRadius;
            float  _Ratio;
            float  _Feather;

            float  _EdgeSpeed;
            float  _EdgeFlow;   // �y�ʶ}�� (0 or 1)

            float _EdgeFreqs[8];
            float _EdgeAmps[8];
            int   _EdgeCount;

            // �^�� v �b [a,b] �϶�������ҡ]���ĩ� Unity �� Mathf.InverseLerp�^
            float invlerp(float a, float b, float v)
            {
                return saturate((v - a) / (b - a));
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                float2 d = i.uv - _HoleCenter.xy;
                d.x *= _Ratio;
                float dist = length(d);
                float ang = atan2(d.y, d.x);

                float wave = 0.0;
                [unroll]

                const float TAU = 6.2831853;

                for (int j = 0; j < 8; j++)
                {
                    if (j >= _EdgeCount) break;
                    if (_EdgeFlow > 0.5)
                    {
                        wave += _EdgeAmps[j] * sin(_EdgeFreqs[j] * TAU + _Time.y * _EdgeSpeed);
                    }
                    else
                    {
                        wave += _EdgeAmps[j] * sin(_EdgeFreqs[j] * ang);
                    }
                }

                //float edgeRadius = _HoleRadius + wave * pow(mul, 2.0);
                float edgeRadius = _HoleRadius + wave;

                float alphaMask;

                float featherRange = min(_Feather, edgeRadius * 0.9);
                alphaMask = smoothstep(edgeRadius - featherRange, edgeRadius + featherRange, dist);

                // ======================================================
                // �s�W�Ϭq�G�P�W�@�V�B�n�V�X�A�Φ����ܮĪG
                // ======================================================
                float prevAlpha = tex2D(_PrevMaskTex, i.uv).a;                 // ���ˤW�@�V�z����

                // �[�w��
                float resultAlpha = 0;
                float diff = alphaMask - prevAlpha;
                if(abs(diff) > _MaskBlend)
                {
                    resultAlpha = clamp(prevAlpha + sign(diff) * _MaskBlend, 0.0, 1.0);
                }
                else
                {
                    resultAlpha = alphaMask;
                }

                // if (_Feather <= 1e-5)
                //     alphaMask = (dist < edgeRadius) ? 0.0 : 1.0;
                // else
                //     alphaMask = smoothstep(edgeRadius - _Feather, edgeRadius + _Feather, dist);

                col.a = resultAlpha;
                return col;
            }
            ENDCG
        }
    }
}
