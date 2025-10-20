Shader "Custom/SimpleHole_MultiFreq_Amps"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HoleCenter ("Hole Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _HoleRadius ("Hole Radius", Float) = 0.25

        _EdgeSpeed ("Edge Speed", Float) = 1.0
        _Feather   ("Edge Feather", Range(0, 0.2)) = 0.02
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

            float  _EdgeSpeed;
            float  _Feather;

            // 外部輸入的陣列
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

                // 混頻波疊加（使用個別振幅）
                float wave = 0.0;
                [unroll]
                for (int j = 0; j < 8; j++)
                {
                    if (j >= _EdgeCount) break;
                    wave += sin(ang * _EdgeFreqs[j] + _Time.y * _EdgeSpeed) * _EdgeAmps[j];
                }

                float edgeRadius = _HoleRadius + wave;

                // 邊緣柔化
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
