namespace VirtualTexture.Sample
{
    using System;
    using UnityEngine;

    [ExecuteAlways]
    public class VTUnlitTest : MonoBehaviour
    {
        private static readonly int SimplePhysicsInfo = Shader.PropertyToID("Simple_PhysicsInfo");
        private static readonly int SimplePageInfo = Shader.PropertyToID("Simple_PageInfo");
        private static readonly int SimplePhysics = Shader.PropertyToID("Simple_Physics");
        private static readonly int SimplePage = Shader.PropertyToID("Simple_Page");

        private Texture2D testPageTexture;
        public void Start()
        {
            if (TryGetComponent<Renderer>(out var rvtRenderer))
            {
                Material material;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    material = rvtRenderer.sharedMaterial;
                }
                else
#endif
                {
                    material = rvtRenderer.material;
                }

                if (material != null && material.shader.name == "Universal Render Pipeline/Virtual Texture/Unlit")
                {
                    Texture2DArray physicsTexture = material.GetTexture(SimplePhysics) as Texture2DArray;
                    if (physicsTexture == null) return;
                    
                    int width = Mathf.RoundToInt(Mathf.Sqrt(physicsTexture.depth));
                    material.SetVector(SimplePhysicsInfo, new Vector4(physicsTexture.width, physicsTexture.height, width, 0));
                    if (Application.isPlaying)
                    {
                        testPageTexture = new Texture2D(width, width, TextureFormat.RGBA32, 4, true)
                        {
                            filterMode = FilterMode.Point,
                            wrapMode = TextureWrapMode.Clamp,
                        };
                        
                        for (int mip = 0; mip < testPageTexture.mipmapCount; ++mip)
                        {
                            int mipWidth = width >> mip;
                            for (int x = 0; x < mipWidth; ++x)
                            {
                                for (int y = 0; y < mipWidth; ++y)
                                {
                                    testPageTexture.SetPixel(x, y, new Color(x / 255.0f, y / 255.0f, mip / 255.0f, 1), mip);
                                }
                            }
                        }
                        
                        testPageTexture.Apply(false, false);
                        material.SetTexture(SimplePage, testPageTexture);
                        material.SetVector(SimplePageInfo, new Vector4(testPageTexture.width, testPageTexture.height, testPageTexture.mipmapCount - 1, 0));
                    }
                }
            }
        }

        public void OnDestroy()
        {
            if (testPageTexture != null)
            {
                DestroyImmediate(testPageTexture);
            }
        }
    }
}
