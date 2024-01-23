#ifndef UNIVERSAL_UNLIT_INPUT_INCLUDED
#define UNIVERSAL_UNLIT_INPUT_INCLUDED

#include "Assets/Unity-VirtualTexture/ShaderLibrary/VirtualTextureCore.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
    half4 Simple_PageInfo;
    half4 Simple_PhysicsInfo;
CBUFFER_END

VIRTUAL_TEXTURE(Simple);

#endif
