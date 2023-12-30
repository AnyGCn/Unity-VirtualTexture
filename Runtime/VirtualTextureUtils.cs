namespace VirtualTexture.Runtime
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class VirtualTextureUtils
    {
        /// <summary>
        /// Set virtual texture to material using the name of virtualTexture.
        /// </summary>
        public static void SetVirtualTexture(this Material material, VirtualTexture virtualTexture)
        {
            material.SetTexture(virtualTexture.PageTextureID, virtualTexture.PageTexture);
            material.SetTexture(virtualTexture.PhysicsTextureID, virtualTexture.PhysicsTexture);
            material.SetVector(virtualTexture.PageTextureInfoID, virtualTexture.PageTextureInfo);
            material.SetVector(virtualTexture.PhysicsTextureInfoID, virtualTexture.PhysicsTextureInfo);
        }
        
        /// <summary>
        /// Set virtual texture using the name of virtualTexture globally.
        /// </summary>
        public static void SetGlobalVirtualTexture(VirtualTexture virtualTexture)
        {
            Shader.SetGlobalTexture(virtualTexture.PageTextureID, virtualTexture.PageTexture);
            Shader.SetGlobalTexture(virtualTexture.PhysicsTextureID, virtualTexture.PhysicsTexture);
            Shader.SetGlobalVector(virtualTexture.PageTextureInfoID, virtualTexture.PageTextureInfo);
            Shader.SetGlobalVector(virtualTexture.PhysicsTextureInfoID, virtualTexture.PhysicsTextureInfo);
        }
    }
}
