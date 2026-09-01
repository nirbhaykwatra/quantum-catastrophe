Shader "Custom/EntanglementIndicator"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0.4, 0.9, 1, 1)
        _OutlineThickness ("Outline Thickness (px)", Range(0, 4)) = 1.5
        _PulseSpeed ("Pulse Speed", Float) = 3.0
        _PulseStrength ("Pulse Strength", Range(0,1)) = 0.5
        _ActivationT ("Activation (0 = inactive, 1 = active)", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

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
            float4 _MainTex_TexelSize;

            float4 _Color;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _PulseSpeed;
            float _PulseStrength;
            float _ActivationT;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float4 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color * IN.color;

                // Edge-detect outline: sample 4 neighbors, see if any has alpha where center doesn't.
                float2 texel = _MainTex_TexelSize.xy * _OutlineThickness;
                float aUp    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(0, texel.y)).a;
                float aDown  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - float2(0, texel.y)).a;
                float aLeft  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - float2(texel.x, 0)).a;
                float aRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(texel.x, 0)).a;
                float neighborMax = max(max(aUp, aDown), max(aLeft, aRight));
                float outlineMask = saturate(neighborMax - baseCol.a);

                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseStrength;

                float4 outline = _OutlineColor * outlineMask * _ActivationT * pulse;
                // Also tint the sprite body itself faintly during activation so it doesn't only glow at edges.
                float3 tintedBody = baseCol.rgb + _OutlineColor.rgb * baseCol.a * _ActivationT * 0.25 * pulse;

                float3 finalRGB = tintedBody + outline.rgb;
                float finalA = saturate(baseCol.a + outline.a);

                return float4(finalRGB, finalA);
            }
            ENDHLSL
        }
    }
}