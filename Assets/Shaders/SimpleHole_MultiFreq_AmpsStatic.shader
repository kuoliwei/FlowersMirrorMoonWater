Shader "Custom/SimpleHole_MultiFreq_AmpsStatic"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HoleCenter ("Hole Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _HoleRadius ("Hole Radius", Float) = 0.25

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

            float4 _HoleCenter;
            float  _HoleRadius;
            float  _Feather;

            float  _EdgeSpeed;
            float  _EdgeFlow;   // 流動開關 (0 or 1)

            float _EdgeFreqs[8];
            float _EdgeAmps[8];
            int   _EdgeCount;

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

                float wave = 0.0;
                [unroll]
                for (int j = 0; j < 8; j++)
                {
                    if (j >= _EdgeCount) break;

                    // 根據 _EdgeFlow 決定是否加入時間流動
                    float phase = ang * _EdgeFreqs[j];
                    if (_EdgeFlow > 0.5)   // 勾選時才會動
                        phase += _Time.y * _EdgeSpeed;

                    wave += sin(phase) * _EdgeAmps[j];
                }
                // 弦波乘數：1 + A * sin(2π f t + φ)
                // 讓乘數圍繞 1 上下擺動，避免乘出負值
                const float TAU = 6.2831853;

                if (ang < 0) ang += 6.28318;
                // 振幅隨角度變化（在 0.1x ~ 2.0x 範圍內）
                float ampVar = 0.1 + 0.95 * (sin(ang * 3.0) + 1.0);

                float edgeRadius = _HoleRadius + wave * ampVar;

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
