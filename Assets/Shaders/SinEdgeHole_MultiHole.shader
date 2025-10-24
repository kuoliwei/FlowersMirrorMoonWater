Shader "Custom/SinEdgeHole_MultiHole"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            sampler2D _PrevMaskTex;
            float _MaskBlend;

            float4 _HoleCenters[32]; // xy �����߮y�� (UV)�Azw �i�O�d�Ω���
            float _HoleRadii[32];    // �����C�Ӭ}���b�|
            int _HoleCount;                 // ��ڬ}���ƶq�]�� C# �ǤJ�^

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

                    // �C�Ӭ}����t�i�ʲզX
                    for (int k = 0; k< 8; k++)
                    {
                        if (k >= _EdgeCount) break;

                        if (_EdgeFlow > 0.5)
                        {
                            // �y�ʪ����]���̨��ס^
                            wave[x] += _EdgeAmps[k] * _HoleRadii[x] * sin(_EdgeFreqs[k] * TAU + _Time.y * _EdgeSpeed);
                        }
                        else
                        {
                            // �T�w���ת����]�ϥθӬ}������ ang[xk]�^
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
                    // �}���z���A�~�򤣳z��
                    alphaMasks[j] = smoothstep(edgeRadius[j] - featherRange[j], edgeRadius[j], dist[j]);
                }
                

                float alphaMaskTotal = 1.0;

                for (int c = 0; c < _HoleCount; c++)
                {
                    alphaMaskTotal = min(alphaMaskTotal, alphaMasks[c]);
                }

                // ======================================================
                // �s�W�Ϭq�G�P�W�@�V�B�n�V�X�A�Φ����ܮĪG
                // ======================================================
                float prevAlpha = tex2D(_PrevMaskTex, i.uv).a;                 // ���ˤW�@�V�z����

                // �[�w��
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
