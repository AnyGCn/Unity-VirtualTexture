#ifndef VirtualTextureCore
#define VirtualTextureCore

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

#define VIRTUALTEXTURE(name)    TEXTURE2D(name##_Page); \
                                SAMPLER(sampler_##name##_Page); \
                                TEXTURE2D_ARRAY(name##_Physics); \
                                SAMPLER(sampler_##name##_Physics); \
                                float4 name##_PageInfo; \
                                float4 name##_PhysicsInfo; \

#endif