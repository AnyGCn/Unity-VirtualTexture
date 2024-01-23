
namespace VirtualTexture.Runtime
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;

    public static class CopyTextureHelper
    {
        private static ProfilingSampler _profilingSampler = new ProfilingSampler(nameof(CopyTextureHelper));
        
        public static void CopyTexture(
            Texture src,
            int srcElement,
            int srcMip,
            int srcX,
            int srcY,
            int srcWidth,
            int srcHeight,
            Texture2DArray dst,
            int dstElement,
            int dstMip,
            int dstX,
            int dstY)
        {
            using (var cmd = CommandBufferPool.Get())
            {
                RenderTexture rtTmp = RenderTexture.GetTemporary(srcWidth, srcHeight, 0, RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Default);
                using (new ProfilingScope(cmd, _profilingSampler))
                {
                    cmd.Blit(src, rtTmp, Vector2.one, Vector2.zero);
                    cmd.RequestAsyncReadback(rtTmp, (request) =>
                    {
                        if (request.hasError)
                        {
                            Debug.LogError("CopyTextureHelper: AsyncReadback has error");
                        }
                        else
                        {
                            var data = request.GetData<Color32>();
                            dst.SetPixelData(data, 0, dstElement);
                        }

                        RenderTexture.ReleaseTemporary(rtTmp);
                    });
                }
                
                Graphics.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

        }
    }
}
