Shader "FiveGenCar/BackScreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TexWidth ("TexWidth", Float) = 0
        _TexHeight ("TexHeight", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TexWidth;
            float _TexHeight;

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld,v.vertex); // unity_ObjectToWorld = UNITY_MATRIX_M
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //o.worldPos = mul(unity_ObjectToWorld,v.uv); 
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float ratio = _TexWidth / _TexHeight;
                float2 horizontalProjection = float2(i.worldPos.x / ratio,i.worldPos.y);
                horizontalProjection += float2(0.5, 0.5);
                fixed4 col = tex2D(_MainTex, horizontalProjection );
                return col;
            }
            ENDCG
        }
    }
}
