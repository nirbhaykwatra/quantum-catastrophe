Shader "Custom/EntanglementLink"
{
    Properties
    {
        _Color ("Color", Color) = (0.4, 0.8, 1, 1)
        _MainTex ("Pattern (optional)", 2D) = "white" {}
        _ScrollSpeed ("Scroll Speed", Float) = 1.0
        _PulseSpeed ("Pulse Speed", Float) = 2.0
        _PulseStrength ("Pulse Strength", Range(0,1)) = 0.3
        _Glow ("Glow Intensity", Float) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha One   // additive-ish glow; use Blend SrcAlpha OneMinusSrcAlpha for a solid line
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;
            float4 _MainTex_ST;
            float _ScrollSpeed;
            float _PulseSpeed;
            float _PulseStrength;
            float _Glow;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                uv.x -= _Time.y * _ScrollSpeed;

                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseStrength;

                // fade edges along the width (v axis) for a soft beam look
                float edgeFade = smoothstep(0.0, 0.15, IN.uv.y) * smoothstep(1.0, 0.85, IN.uv.y);

                float4 col = _Color * tex * IN.color;
                col.rgb *= _Glow * pulse;
                col.a *= edgeFade * pulse;

                return col;
            }
            ENDHLSL
        }
    }
}