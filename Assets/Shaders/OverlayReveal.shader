Shader "Custom/OverlayReveal"
{
    Properties
    {
        _OverlayTex ("Overlay / Fog Texture (遮罩圖)", 2D) = "white" {}
        _Tint       ("Overlay Tint", Color) = (1,1,1,1)
        _BaseAlpha  ("Overlay Base Opacity", Range(0,1)) = 1.0

        _HandPos    ("Hand Position (Viewport XY)", Vector) = (0.5, 0.5, 0, 0)
        _Radius     ("Hole Radius", Float) = 0.18
        _Softness   ("Edge Softness", Float) = 0.12

        _FlowSpeed  ("Edge Flow Speed", Float) = 0.15
        _NoiseScale ("Noise Scale", Float) = 2.5
        _NoisePower ("Noise Power", Range(0,0.08)) = 0.02
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

            sampler2D _OverlayTex;
            float4 _OverlayTex_ST;
            float4 _Tint;
            float  _BaseAlpha;

            float4 _HandPos;
            float  _Radius;
            float  _Softness;

            float  _FlowSpeed;
            float  _NoiseScale;
            float  _NoisePower;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float2 suv : TEXCOORD1; // screen-space UV (0~1)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _OverlayTex);

                // 以 NDC -> viewport 近似：用clip space除以w交給片元再算也可，簡化用 SV_POSITION 後在片元用 i.pos
                // 這裡改用 ComputeScreenPos 取得螢幕uv
                float4 sp = ComputeScreenPos(o.pos);
                o.suv = sp.xy / sp.w; // 0~1 螢幕UV
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 遮罩本體顏色（可用雲霧貼圖或一張半透明圖）
                fixed4 overlayCol = tex2D(_OverlayTex, i.uv) * _Tint;

                // 基礎遮罩不透明度（保持畫面被蓋住的程度）
                float overlayAlpha = saturate(_BaseAlpha * overlayCol.a);

                // 手洞：距離 + 雲霧流動邊緣
                float2 handUV = _HandPos.xy;     // Viewport 0~1
                float2 uvScreen = i.suv;         // 這個片元的螢幕UV 0~1

                float dist = distance(uvScreen, handUV);

                // 邊緣流動噪聲（使用遮罩圖本身當作 noise 也可）
                float2 flowUV = i.uv * _NoiseScale + float2(_Time.y, _Time.y * 0.5) * _FlowSpeed;
                float noise = tex2D(_OverlayTex, flowUV).r; // 簡單取紅通道當 noise
                float distWithNoise = dist - (noise - 0.5) * 2.0 * _NoisePower;

                // 從中心透明（0）到外圍不透明（1）
                float edge = smoothstep(_Radius, _Radius + _Softness, distWithNoise);

                // 最終 alpha：遮罩不透明度 * 邊緣函數
                float finalAlpha = overlayAlpha * edge;

                // 顏色用遮罩顏色（可做微亮/灰霧）；透明後會看到後方
                return fixed4(overlayCol.rgb, finalAlpha);
            }
            ENDCG
        }
    }
}
