Shader "UI/DialogueSoftNoiseVignette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.1, 0.1, 0.15, 0.85)
        _NoiseScale ("Noise Scale", Float) = 4.0
        _NoiseSpeedX ("Noise Speed X", Float) = 0.08
        _NoiseSpeedY ("Noise Speed Y", Float) = 0.04
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.25
        _VignetteSize ("Vignette Size", Range(0, 1.5)) = 0.85
        _VignetteSoftness ("Vignette Softness", Range(0, 1)) = 0.45
        _RevealProgress ("Reveal Progress", Range(0, 1)) = 1.0
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float4 _BaseColor;
            float _NoiseScale;
            float _NoiseSpeedX;
            float _NoiseSpeedY;
            float _NoiseStrength;
            float _VignetteSize;
            float _VignetteSoftness;
            float _RevealProgress;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                return o;
            }

            float hash(float2 p)
            {
                p = frac(p * 0.3183099 + .1);
                p *= 17.0;
                return frac(p.x * p.y * (p.x + p.y));
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 mainTex = tex2D(_MainTex, i.texcoord);
                
                float2 uv = i.texcoord;
                float2 speed = float2(_NoiseSpeedX, _NoiseSpeedY);
                float n = noise(uv * _NoiseScale + _Time.y * speed);
                
                float2 center = uv - 0.5;
                float dist = length(center);
                float vig = smoothstep(_VignetteSize, _VignetteSize - _VignetteSoftness, dist);
                
                float proceduralAlpha = vig * (1.0 - (n * _NoiseStrength)) * _BaseColor.a * _RevealProgress;
                
                float finalAlpha = mainTex.a * proceduralAlpha * i.color.a;
                
                return fixed4(_BaseColor.rgb * i.color.rgb, finalAlpha);
            }
            ENDCG
        }
    }
}