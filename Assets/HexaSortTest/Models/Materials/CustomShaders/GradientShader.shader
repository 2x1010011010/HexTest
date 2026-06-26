Shader "Custom/GradientLitShader"
{
    Properties
    {
        _Angle("Gradient Angle (Degrees)", Range(0, 360)) = 0
        _ColorCount("Color Count", Range(2, 8)) = 2

        [Header(Gradient Colors)]
        _Color1("Color 1", Color) = (1, 0, 0, 1)
        _Color2("Color 2", Color) = (0, 0, 1, 1)
        _Color3("Color 3", Color) = (1, 1, 1, 1)
        _Color4("Color 4", Color) = (1, 1, 1, 1)
        _Color5("Color 5", Color) = (1, 1, 1, 1)
        _Color6("Color 6", Color) = (1, 1, 1, 1)
        _Color7("Color 7", Color) = (1, 1, 1, 1)
        _Color8("Color 8", Color) = (1, 1, 1, 1)

        [Header(PBR Properties)]
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType" = "Opaque" 
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float2 uv           : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Angle;
                float _ColorCount;
                float4 _Color1;
                float4 _Color2;
                float4 _Color3;
                float4 _Color4;
                float4 _Color5;
                float4 _Color6;
                float4 _Color7;
                float4 _Color8;
                float _Smoothness;
                float _Metallic;
            CBUFFER_END

            float3 EvaluateGradient(float2 uv)
            {
                float rad = radians(_Angle);
                float c = cos(rad);
                float s = sin(rad);
                
                float t = (uv.x - 0.5) * c - (uv.y - 0.5) * s + 0.5;
                t = saturate(t);

                float4 colors[8] = { _Color1, _Color2, _Color3, _Color4, _Color5, _Color6, _Color7, _Color8 };
                
                float intervals = max(1.0, _ColorCount - 1.0);
                float scaledT = t * intervals;
                
                int index = (int)floor(scaledT);
                index = clamp(index, 0, 6);
                
                float lerpFactor = frac(scaledT);
                if (scaledT >= intervals) lerpFactor = 1.0;

                float3 colorA = colors[index].rgb;
                float3 colorB = colors[index + 1].rgb;

                return lerp(colorA, colorB, lerpFactor);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                normalInput.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
                output.normalWS = normalInput.normalWS;
                
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 gradientColor = EvaluateGradient(input.uv);

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirectionWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                
                SurfaceData surfaceData = (SurfaceData)0;
                InputData inputData = (InputData)0;

                surfaceData.albedo = gradientColor;
                surfaceData.alpha = 1.0;
                surfaceData.metallic = _Metallic;
                surfaceData.specular = float3(0.0, 0.0, 0.0);
                surfaceData.smoothness = _Smoothness;
                surfaceData.occlusion = 1.0;
                surfaceData.emission = float3(0.0, 0.0, 0.0);
                surfaceData.normalTS = float3(0.0, 0.0, 1.0);

                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirectionWS;
                inputData.shadowCoord = shadowCoord;
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = 0;
                inputData.shadowMask = 0;

                float4 finalColor = UniversalFragmentPBR(inputData, surfaceData);
                return finalColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_shadowcaster

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                float3 positionWS = ApplyShadowBias(vertexInput.positionWS, normalInput.normalWS, _MainLightPosition.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}
