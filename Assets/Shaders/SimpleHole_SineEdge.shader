Shader "Custom/SimpleHole_SineEdge"
{
    Properties
    {
        _MainTex    ("Texture", 2D) = "white" {}
        _HoleCenter ("Hole Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _HoleRadius ("Hole Radius", Float) = 0.25

        _EdgeAmp    ("Edge Amplitude", Range(0, 0.15)) = 0.035   // �i���_��T��
        _EdgeFreq   ("Edge Frequency", Float) = 8.0               // �C��X�Ӫi�p
        _EdgeSpeed  ("Edge Speed", Float) = 1.0                   // ���פ�V�y�ʳt��

        _Feather    ("Edge Feather (soft)", Range(0, 0.2)) = 0.02 // ��t�X�ơ]0=�w���^
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

            float  _Feather;     // ��t�X�Ƽe��
            // �`�N�G_Time �� Unity ���ءA���n�ۦ�ŧi

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

                // �줤�ߪ��V�q & �Z��
                float2 d    = i.uv - _HoleCenter.xy;
                float  dist = length(d);

                // ���ס]-PI ~ PI�^�CHLSL/CG �� atan2 �Ѽƶ��Ǭ� atan2(y, x)
                float ang = atan2(d.y, d.x);

                // �H���׬��ۦ쪺���i�G�b��P��V���ͳW�߰_��
                float wave = sin(ang * _EdgeFreq + _Time.y * _EdgeSpeed) * _EdgeAmp;

                // �����פU���u��ɥb�|�v�G��¦�b�| + �i��
                float edgeRadius = _HoleRadius + wave;

                // �z���רM�� �X�X ���˥� smoothstep ���X�ơA�קK�w����
                // feather = 0 �ɬ۷��w���]�P if(dist < edgeRadius) �����^
                float alphaMask;
                if (_Feather <= 1e-5)
                {
                    // �w��G�}���z���A�}�~������ alpha
                    alphaMask = (dist < edgeRadius) ? 0.0 : 1.0;
                }
                else
                {
                    // �n��G�b [edgeRadius - feather, edgeRadius + feather] ��������
                    // �o�̧ڭ̷Q�n�u�}���z���v�A�ҥH�� smoothstep �Ϭ�
                    float a = smoothstep(edgeRadius - _Feather, edgeRadius + _Feather, dist);
                    alphaMask = a; // a=0 �� �z���Fa=1 �� ���z��
                }

                col.a *= alphaMask;
                return col;
            }
            ENDCG
        }
    }
}
