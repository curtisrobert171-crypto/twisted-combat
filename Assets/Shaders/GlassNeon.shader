Shader "EmpireOfGlass/GlassNeon"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)
        _EmissionColor ("Neon Color", Color) = (0,1,1,1)
        _EmissionIntensity ("Neon Intensity", Range(0, 10)) = 2.0
        _Transparency ("Transparency", Range(0, 1)) = 0.3
        _Refraction ("Refraction", Range(0, 1)) = 0.1
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 3.0
        _RimColor ("Rim Color", Color) = (0, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.1, 10)) = 4.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "GlassNeonPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float fogCoord : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _EmissionColor;
                half _EmissionIntensity;
                half _Transparency;
                half _Refraction;
                half _FresnelPower;
                half4 _RimColor;
                half _RimPower;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // Sample base texture
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;

                // Normalize directions
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(input.viewDirWS);

                // Fresnel effect for glass edges
                half fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);

                // Rim lighting for neon glow
                half rim = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _RimPower);
                half3 rimEmission = _RimColor.rgb * rim;

                // Combine base color with neon emission
                half3 emission = _EmissionColor.rgb * _EmissionIntensity + rimEmission;
                half3 finalColor = lerp(baseColor.rgb, emission, fresnel);

                // Calculate transparency
                half alpha = lerp(_Transparency, 1.0, fresnel);
                alpha *= baseColor.a;

                // Apply fog
                finalColor = MixFog(finalColor, input.fogCoord);

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
