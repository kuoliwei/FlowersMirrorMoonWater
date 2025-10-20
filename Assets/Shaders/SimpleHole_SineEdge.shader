Shader "Custom/SimpleHole_SineEdge"
{
    Properties
    {
        _MainTex    ("Texture", 2D) = "white" {}
        _HoleCenter ("Hole Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _HoleRadius ("Hole Radius", Float) = 0.25

        _EdgeAmp    ("Edge Amplitude", Range(0, 0.15)) = 0.035   // 波浪起伏幅度
        _EdgeFreq   ("Edge Frequency", Float) = 8.0               // 每圈幾個波峰
        _EdgeSpeed  ("Edge Speed", Float) = 1.0                   // 角度方向流動速度

        _Feather    ("Edge Feather (soft)", Range(0, 0.2)) = 0.02 // 邊緣柔化（0=硬切）
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _HoleCenter;
            float  _HoleRadius;

            float  _EdgeAmp;
            float  _EdgeFreq;
            float  _EdgeSpeed;

            float  _Feather;     // 邊緣柔化寬度
            // 注意：_Time 為 Unity 內建，不要自行宣告

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

                // 到中心的向量 & 距離
                float2 d    = i.uv - _HoleCenter.xy;
                float  dist = length(d);

                // 角度（-PI ~ PI）。HLSL/CG 的 atan2 參數順序為 atan2(y, x)
                float ang = atan2(d.y, d.x);

                // 以角度為相位的弦波：在圓周方向產生規律起伏
                float wave = sin(ang * _EdgeFreq + _Time.y * _EdgeSpeed) * _EdgeAmp;

                // 此角度下的「邊界半徑」：基礎半徑 + 波動
                float edgeRadius = _HoleRadius + wave;

                // 透明度決策 —— 推薦用 smoothstep 做柔化，避免硬鋸齒
                // feather = 0 時相當於硬切（與 if(dist < edgeRadius) 等價）
                float alphaMask;
                if (_Feather <= 1e-5)
                {
                    // 硬邊：洞內透明，洞外維持原 alpha
                    alphaMask = (dist < edgeRadius) ? 0.0 : 1.0;
                }
                else
                {
                    // 軟邊：在 [edgeRadius - feather, edgeRadius + feather] 之間漸變
                    // 這裡我們想要「洞內透明」，所以把 smoothstep 反相
                    float a = smoothstep(edgeRadius - _Feather, edgeRadius + _Feather, dist);
                    alphaMask = a; // a=0 → 透明；a=1 → 不透明
                }

                col.a *= alphaMask;
                return col;
            }
            ENDCG
        }
    }
}
