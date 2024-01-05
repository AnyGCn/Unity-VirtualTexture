#ifndef UNIVERSAL_UNLIT_INPUT_INCLUDED
#define UNIVERSAL_UNLIT_INPUT_INCLUDED

#include "Assets/Unity-VirtualTexture/ShaderLibrary/VirtualTextureCore.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half _Cutoff;
    half _Surface;
CBUFFER_END

VIRTUAL_TEXTURE(Simple);

#endif
