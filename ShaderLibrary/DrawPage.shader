Shader "Universal Render Pipeline/Virtual Texture/DrawPage"
{
    Properties
    {
        [HideInInspector] _StartIndex("Start Index", Float) = 0
    }
    
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
            "UniversalMaterialType" = "Unlit"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Cull Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 4.5

            // -------------------------------------
            // Shader Stages
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #pragma enable_d3d11_debug_symbols

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            #define PageSize 256
            
            uint _StartIndex;
            CBUFFER_START(TileInfo)
                float4 _TileInfo[256];
            CBUFFER_END
            
            static const half2 VERTEX_POS[6] =
            {
                0,0,
                1,0,
                1,1,
                0,0,
                1,1,
                0,1
            };
            
            struct Varyings
            {
                half4 positionCS : SV_POSITION;
                uint instanceID : SV_InstanceID;
            };

            Varyings UnlitPassVertex(uint vertexID: SV_VertexID, uint instanceID : SV_InstanceID)
            {
                instanceID = instanceID + _StartIndex;
                Varyings output = (Varyings)0;
                float2 pos = _TileInfo[instanceID].xy;
                uint mip = round(_TileInfo[instanceID].z);
                float scale = 1 << mip;
                pos = (pos + VERTEX_POS[vertexID]) * scale / PageSize * 2.0f - 1.0f;
#if UNITY_UV_STARTS_AT_TOP
	            pos = pos * half2(1.0, -1.0);
#endif

                // lower mip can block higher mip
#if UNITY_REVERSED_Z
                float depth = 1.0f - 0.1f * mip;
#else
                float depth = 0.1f * mip;
#endif

                output.positionCS = float4(pos, depth, 1);
                output.instanceID = instanceID;
                return output;
            }

            void UnlitPassFragment(
                Varyings input
                , out half4 outColor : SV_Target0
            )
            {
                outColor = _TileInfo[input.instanceID] / 255.0f;
            }
            
            ENDHLSL
        }
    }
}
