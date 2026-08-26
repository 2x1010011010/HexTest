Shader "HexaSort/MetaTileReveal"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (1, 1, 1, 1)
        _BlackColor ("Locked Color", Color) = (0, 0, 0, 1)
        _EdgeColor ("Edge Glow Color", Color) = (0.35, 0.85, 1, 1)
        _EdgeWidth ("Edge Width", Range(0.001, 0.3)) = 0.05
        _Progress ("Progress", Range(0, 1)) = 0
        _MinY ("Local Min Y", Float) = 0
        _MaxY ("Local Max Y", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float2 uv         : TEXCOORD1;
                float  localY     : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainColor;
                float4 _BlackColor;
                float4 _EdgeColor;
                float _EdgeWidth;
                float _Progress;
                float _MinY;
                float _MaxY;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = positionInputs.positionCS;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.localY = IN.positionOS.y;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float span = max(0.0001, _MaxY - _MinY);
                float heightT = saturate((IN.localY - _MinY) / span);
                float revealed = step(heightT, _Progress);
                float distToLine = abs(heightT - _Progress);
                float edgeMask = 1.0 - smoothstep(0.0, _EdgeWidth, distToLine);
                float midReveal = step(0.0005, _Progress) * (1.0 - step(0.9995, _Progress));
                edgeMask *= midReveal;

                half4 texSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half3 mainAlbedo = texSample.rgb * _MainColor.rgb;

                Light mainLight = GetMainLight();
                float ndotl = saturate(dot(normalize(IN.normalWS), mainLight.direction));

                half3 litMain = mainAlbedo * (ndotl * mainLight.color.rgb) + mainAlbedo * 0.15;
                half3 litBlack = _BlackColor.rgb * (ndotl * 0.5 + 0.15);

                half3 color = lerp(litBlack, litMain, revealed);
                color = lerp(color, _EdgeColor.rgb, edgeMask);

                return half4(color, texSample.a * _MainColor.a);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
