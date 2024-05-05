using System.Globalization;

namespace VirtualTexture.Sample
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class VTUnlitShaderGUI : BaseShaderGUI
    {
        private VTUnlitShaderProperties VTProperties;

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            VTProperties = new VTUnlitShaderProperties(properties);
        }
    
        public override void ValidateMaterial(Material material)
        {
            base.ValidateMaterial(material);
            if (VTProperties == default) return;
            if (VTProperties.physicsTexture != null && VTProperties.physicsTextureInfo != null)
            {
                Texture2DArray physicsTexArray = VTProperties.physicsTexture.textureValue as Texture2DArray;
                if (physicsTexArray != null)
                {
                    material.SetVector(VTProperties.physicsTextureInfo.name, new Vector4(physicsTexArray.width, physicsTexArray.height, physicsTexArray.depth, 0));
                }
            }

            if (VTProperties.pageTexture != null && VTProperties.pageTextureInfo != null)
            {
                if (VTProperties.pageTexture.textureValue != null)
                {
                    material.SetVector(VTProperties.pageTextureInfo.name, new Vector4(VTProperties.pageTexture.textureValue.width, VTProperties.pageTexture.textureValue.height, VTProperties.pageTexture.textureValue.mipmapCount, 0));
                }
            }
        }

        public override void DrawAdvancedOptions(Material material)
        {
            if (VTProperties.physicsTexture != null && VTProperties.physicsTextureInfo != null)
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("物理贴图数组"), VTProperties.physicsTexture);
                EditorGUILayout.Vector3Field(new GUIContent("宽度,高度,数量"), VTProperties.physicsTextureInfo.vectorValue);
            }

            if (VTProperties.pageTexture != null && VTProperties.pageTextureInfo != null)
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("虚拟纹理页表"), VTProperties.pageTexture);
                EditorGUILayout.Vector3Field(new GUIContent("宽度,高度,层数"), VTProperties.pageTextureInfo.vectorValue);
            }
            
            base.DrawAdvancedOptions(material);
        }
        
        private class VTUnlitShaderProperties
        {
            public MaterialProperty physicsTexture;
        
            public MaterialProperty physicsTextureInfo;

            public MaterialProperty pageTexture;

            public MaterialProperty pageTextureInfo;

            public VTUnlitShaderProperties(MaterialProperty[] properties)
            {
                physicsTexture = ShaderGUI.FindProperty("Simple_Physics", properties, false);
                physicsTextureInfo = ShaderGUI.FindProperty("Simple_PhysicsInfo", properties, false);
                pageTexture = ShaderGUI.FindProperty("Simple_Page", properties, false);
                pageTextureInfo = ShaderGUI.FindProperty("Simple_PageInfo", properties, false);
            }
        }
    }
}