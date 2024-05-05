using System;
using UnityEngine.Experimental.Rendering;

namespace VirtualTexture.Sample
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using VirtualTexture.Runtime;
    using Unity.Mathematics;

    public class PageUpdatedByWorldPosition : MonoBehaviour
    {
        public Renderer vtRenderer;
        public Vector2 Center = Vector2.zero;
        public Vector2 Scale = Vector2.one;
        public int TileSize = 256;
        public int PageSize = 256;
        public int PageMipmapCount = 9;
        public int TileCapacity = 256;
        public int PageTextureMipmapCount = 5;
        
        [NonSerialized]
        public Vector2 RelativePostion;
        
            // => new Vector2(this.transform.localPosition.x, this.transform.localPosition.z) - Center;
        private VirtualTexture virtualTexture;
        
        // Start is called before the first frame update
        void Start()
        {
            virtualTexture = new VirtualTexture(TileSize, PageSize, TileCapacity, PageTextureMipmapCount, GraphicsFormat.R8G8B8A8_UNorm);
            if (vtRenderer != null)
            {
                Material vtMaterial = vtRenderer.material;
                vtMaterial.SetTexture("Simple_Page", virtualTexture.PageTexture);
                vtMaterial.SetVector("Simple_PageInfo", virtualTexture.PageTextureInfo);
            }
        }

        // Update is called once per frame
        void Update()
        {
            var localPosition = this.transform.localPosition;
            RelativePostion = new Vector2(localPosition.x, localPosition.z) - Center;
            UpdatePageTexture();
        }

        void UpdatePageTexture()
        {
            Vector2 relativePosition = RelativePostion / Scale * PageSize + PageSize * Vector2.one / 2;
            Vector2Int relativeIndexPP = new Vector2Int(
                    Mathf.RoundToInt(relativePosition.x + 0.5f),
                    Mathf.RoundToInt(relativePosition.y + 0.5f));
            
            Vector2Int relativeIndexNN = new Vector2Int(
                Mathf.RoundToInt(relativePosition.x - 0.5f),
                Mathf.RoundToInt(relativePosition.y - 0.5f));
            
            Vector2Int relativeIndexNP = new Vector2Int(
                Mathf.RoundToInt(relativePosition.x - 0.5f),
                Mathf.RoundToInt(relativePosition.y + 0.5f));
            
            Vector2Int relativeIndexPN = new Vector2Int(
                Mathf.RoundToInt(relativePosition.x + 0.5f),
                Mathf.RoundToInt(relativePosition.y - 0.5f));
            
            for (int mip = 0; mip < PageMipmapCount; ++mip)
            {
                virtualTexture.Active(relativeIndexPP.x, relativeIndexPP.y, mip);
                virtualTexture.Active(relativeIndexNN.x, relativeIndexNN.y, mip);
                virtualTexture.Active(relativeIndexNP.x, relativeIndexNP.y, mip);
                virtualTexture.Active(relativeIndexPN.x, relativeIndexPN.y, mip);
            }

            virtualTexture.BuildPageTexture();
        }
        
        private void OnDestroy()
        {
            virtualTexture.Dispose();
        }
    }

}
