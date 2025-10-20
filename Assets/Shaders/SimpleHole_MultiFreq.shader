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

            // �~���ǤJ���W�v�}�C
            // �`�N�G���׭n�T�w�A�Ҧp 8�C���ϥΪ������i�b C# �� 0�C
            float _EdgeFreqs[8];
            int   _EdgeFreqCount; // �~���i�D shader ��ڦ��X���W�v

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

                // �V�W�G�j��֥[�h���W�v�� sin �i
                float wave = 0;
                for (int j = 0; j < _EdgeFreqCount; j++)
                {
                    wave += sin(ang * _EdgeFreqs[j] + _Time.y * _EdgeSpeed);
                }
                wave *= (_EdgeAmp / max(1, _EdgeFreqCount)); // �����ơA�קK�V�h�V�j

                float edgeRadius = _HoleRadius + wave;

                // �������
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
