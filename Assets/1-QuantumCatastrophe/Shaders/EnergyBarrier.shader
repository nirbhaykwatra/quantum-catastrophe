Shader "Custom/EnergyBarrier"
{
    Properties
    {
        // Required by SpriteRenderer — it feeds the sprite's own texture/UVs in here.
        // Leave the default White sprite assigned if you just want a plain rectangle/shape mask.
        _MainTex      ("Sprite Texture (required by SpriteRenderer)", 2D) = "white" {}
        [HideInInspector] _BaseMap ("Base Map (alias)", 2D) = "white" {}
        _Color        ("Sprite Tint (SpriteRenderer default tint hook)", Color) = (1,1,1,1)

        _MainColor    ("Barrier Color", Color) = (0.2, 0.6, 1.0, 0.5)
        _EdgeColor    ("Edge Glow Color", Color) = (0.6, 0.9, 1.0, 1.0)
        _NoiseTex     ("Noise Texture", 2D) = "white" {}
        _ScrollSpeed  ("Noise Scroll Speed", Vector) = (0.0, 0.5, 0, 0)
        _NoiseTiling  ("Noise Tiling", Vector) = (1, 3, 0, 0)
        _EdgeWidth    ("Edge Width", Range(0.01, 0.5)) = 0.15
        _EdgeIntensity("Edge Intensity", Range(0, 5)) = 2.0
        _PulseSpeed   ("Pulse Speed", Float) = 2.0
        _PulseAmount  ("Pulse Amount", Range(0, 1)) = 0.25
        _BaseAlpha    ("Base Alpha", Range(0, 1)) = 0.45
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
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
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            // IMPORTANT: all per-material properties must live in this exact CBUFFER for the
            // URP SRP Batcher to work correctly. Properties declared outside of it will often
            // fail to respond to MaterialPropertyBlock overrides at runtime (silently — no error),
            // which is why color changes weren't showing up.
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _MainColor;
                float4 _EdgeColor;
                float4 _ScrollSpeed;
                float4 _NoiseTiling;
                float  _EdgeWidth;
                float  _EdgeIntensity;
                float  _PulseSpeed;
                float  _PulseAmount;
                float  _BaseAlpha;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sprite's own texture (usually a plain white square or a soft rounded-rect mask).
                // If you assign a shaped sprite (e.g. rounded panel), its alpha channel masks the barrier.
                half4 spriteTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;

                // Scroll noise UVs over time to sell the "energy churn" look
                float2 noiseUV = IN.uv * _NoiseTiling.xy + _Time.y * _ScrollSpeed.xy;
                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;

                // Fake fresnel: distance from left/right edges of the barrier (0 = center, 1 = edge)
                float edgeDist = abs(IN.uv.x - 0.5) * 2.0;
                float edgeMask = smoothstep(1.0 - _EdgeWidth, 1.0, edgeDist);

                // Slow global pulse (breathing brightness/alpha)
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

                half3 color = lerp(_MainColor.rgb, _EdgeColor.rgb, edgeMask * _EdgeIntensity);
                color *= pulse;
                color += noise * 0.15; // subtle noise-driven brightness variation

                half alpha = _BaseAlpha * pulse;
                alpha += edgeMask * _EdgeColor.a * 0.5;
                alpha *= saturate(noise + 0.5); // noise gently eats into alpha for a "shimmer" edge
                alpha *= spriteTex.a; // respect the sprite's own shape/mask + SpriteRenderer tint alpha

                return half4(color, saturate(alpha));
            }
            ENDHLSL
        }
    }
}
