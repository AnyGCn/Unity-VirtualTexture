#ifndef VirtualTextureCore
#define VirtualTextureCore

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

#define VIRTUAL_TEXTURE(name)   TEXTURE2D(name##_Page); \
                                SAMPLER(sampler_##name##_Page); \
                                TEXTURE2D_ARRAY(name##_Physics); \
                                SAMPLER(sampler_##name##_Physics); \
                                float4 name##_PageInfo;                 /* x: pageWidth, y: pageHeight, z: offsetX, w: offsetY  */          \
                                float4 name##_PhysicsInfo;              /* x: tileWidth, y: tileHeight, z: tileCountX, w: tileCountY  */    \

#define SAMPLE_VIRTUAL_TEXTURE(name, uv)    sampleVirtualTexture(TEXTURE2D_ARGS(name##_Page, sampler_##name##_Page), TEXTURE2D_ARRAY_ARGS(name##_Physics, sampler_##name##_Physics), name##_PageInfo, name##_PhysicsInfo, uv)

float GetMipMapLevel(float2 nonNormalizedUVCoordinate)
{
    // The OpenGL Graphics System: A Specification 4.2
    //  - chapter 3.9.11, equation 3.21

    float2  dx_vtc = ddx(nonNormalizedUVCoordinate);
    float2  dy_vtc = ddy(nonNormalizedUVCoordinate);
    float delta_max_sqr = max(dot(dx_vtc, dx_vtc), dot(dy_vtc, dy_vtc));

    return 0.5 * log2(delta_max_sqr);
}

// TODO: It seems that the physics texture still need padding to eliminate the border artifacts.
half4 sampleVirtualTexture(TEXTURE2D_PARAM(pageTexture, sampler_pageTexture), TEXTURE2D_ARRAY_PARAM(physicsTexture, sampler_physicsTexture), float4 pageInfo, float4 physicsInfo, float2 uv)
{
    const float2 virtualTextureSize = pageInfo.xy * physicsInfo.xy;
    const float mipLevel = GetMipMapLevel(uv * virtualTextureSize);
    half4 pageColor = SAMPLE_TEXTURE2D_LOD(pageTexture, sampler_pageTexture, uv, uint(mipLevel));         // x: tileIndexX, y: tileIndexY, z: mipCount, w: offsetY

    const uint tileIndex = round(pageColor.x + pageColor.y * physicsInfo.z);
    float2 tileUV = uv * (uint2(pageInfo.xy) >> uint(pageColor.z));
    float4 physicsColor = SAMPLE_TEXTURE2D_ARRAY_LOD(physicsTexture, sampler_physicsTexture, tileUV, tileIndex, frac(mipLevel));
    return physicsColor;
}

#endif