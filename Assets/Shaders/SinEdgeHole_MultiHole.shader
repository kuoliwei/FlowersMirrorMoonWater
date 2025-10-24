Shader "Custom/SinEdgeHole_MultiHole"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Ratio ("Ratio (width/height)", Float) = 0

        _PrevMaskTex ("Previous Mask (read-only)", 2D) = "white" {}
        _MaskBlend   ("Mask Blend 0.01..0.1", Range(0.01, 0.1)) = 0.1   // 之後用來混合：舊遮罩 vs 本幀新遮罩

        _Feather ("Edge Feather", Range(0, 0.2)) = 0.02
        _EdgeFlow ("Enable Flow (1=On,0=Off)", Float) = 0    // 新增開關
        _EdgeSpeed ("Edge Flow Speed", Float) = 1.0           // 保留可調速率
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
            sampler2D _PrevMaskTex;
            float _MaskBlend;

            float4 _HoleCenters[32]; // xy 為中心座標 (UV)，zw 可保留或忽略
            float _HoleRadii[32];    // 對應每個洞的半徑
            int _HoleCount;                 // 實際洞的數量（由 C# 傳入）

            float  _Ratio;
            float  _Feather;

            float  _EdgeSpeed;
            float  _EdgeFlow;   // 流動開關 (0 or 1)

            float _EdgeFreqs[8];
            float _EdgeAmps[8];
            int   _EdgeCount;

            // 回傳 v 在 [a,b] 區間中的比例（等效於 Unity 的 Mathf.InverseLerp）
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

                float2 d[32];
                float dist[32];
                float ang[32];

                for (int j = 0; j < _HoleCount; j++)
                {
                    d[j] = i.uv - _HoleCenters[j].xy;
                    d[j].x *= _Ratio;
                    dist[j] = length(d[j]);
                    ang[j] = atan2(d[j].y, d[j].x);
                }

                float wave[32];


                const float TAU = 6.2831853;
                [unroll]
                for (int x = 0; x < _HoleCount; x++)
                {
                    wave[x] = 0.0;

                    // 每個洞的邊緣波動組合
                    for (int k = 0; k< 8; k++)
                    {
                        if (k >= _EdgeCount) break;

                        if (_EdgeFlow > 0.5)
                        {
                            // 流動版本（不依角度）
                            wave[x] += _EdgeAmps[k] * _HoleRadii[x] * sin(_EdgeFreqs[k] * TAU + _Time.y * _EdgeSpeed);
                        }
                        else
                        {
                            // 固定角度版本（使用該洞的角度 ang[xk]）
                            wave[x] += _EdgeAmps[k] * _HoleRadii[x] * sin(_EdgeFreqs[k] * ang[x]);
                        }
                    }
                }

                float edgeRadius[32];

                float featherRange[32];

                for (int a = 0; a < _HoleCount; a++)
                {
                    edgeRadius[a] = _HoleRadii[a] + wave[a];
                    //edgeRadius[a] = _HoleRadii[a];
                    featherRange[a] = edgeRadius[a] * _Feather;
                }

                float alphaMasks[32];
                for (int j = 0; j < _HoleCount; j++)
                {
                    // 洞內透明，外圍不透明
                    alphaMasks[j] = smoothstep(edgeRadius[j] - featherRange[j], edgeRadius[j], dist[j]);
                }
                

                float alphaMaskTotal = 1.0;

                for (int c = 0; c < _HoleCount; c++)
                {
                    alphaMaskTotal = min(alphaMaskTotal, alphaMasks[c]);
                }

                // ======================================================
                // 新增區段：與上一幀遮罩混合，形成漸變效果
                // ======================================================
                float prevAlpha = tex2D(_PrevMaskTex, i.uv).a;                 // 取樣上一幀透明度

                // 加定值
                float resultAlpha = 0;
                float diff = 0;
                diff = alphaMaskTotal - prevAlpha;
                //float varValue = clamp(_MaskBlend * ((1.0 + _MaskBlend) / (abs(diff) + _MaskBlend)), 0.0, abs(diff) * 0.1);
                resultAlpha = clamp(prevAlpha + sign(diff) * _MaskBlend, 0.0, 1.0);

                col.a = resultAlpha;
                //col.a = alphaMaskTotal;
                return col;
            }
            ENDCG
        }
    }
}
