Shader "Custom/SimpleHole_MultiFreq"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HoleCenter ("Hole Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _HoleRadius ("Hole Radius", Float) = 0.25

        _EdgeAmp    ("Edge Amplitude", Range(0, 0.15)) = 0.03
        _EdgeSpeed  ("Edge Speed", Float) = 1.0
        _Feather    ("Edge Feather", Range(0, 0.2)) = 0.02
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

            float4 _HoleCenter;
            float  _HoleRadius;
            float  _EdgeAmp;
            float  _EdgeSpeed;
            float  _Feather;

            // 外部傳入的頻率陣列
            // 注意：長度要固定，例如 8。未使用的元素可在 C# 填 0。
            float _EdgeFreqs[8];
            int   _EdgeFreqCount; // 外部告訴 shader 實際有幾個頻率

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
                float dist = length(d);
                float ang = atan2(d.y, d.x);

                // 混頻：迴圈累加多個頻率的 sin 波
                float wave = 0;
                for (int j = 0; j < _EdgeFreqCount; j++)
                {
                    wave += sin(ang * _EdgeFreqs[j] + _Time.y * _EdgeSpeed);
                }
                wave *= (_EdgeAmp / max(1, _EdgeFreqCount)); // 平均化，避免越多越強

                float edgeRadius = _HoleRadius + wave;

                // 平滑邊界
                float alphaMask;
                if (_Feather <= 1e-5)
                    alphaMask = (dist < edgeRadius) ? 0.0 : 1.0;
                else
                    alphaMask = smoothstep(edgeRadius - _Feather, edgeRadius + _Feather, dist);

                col.a *= alphaMask;
                return col;
            }
            ENDCG
        }
    }
}
