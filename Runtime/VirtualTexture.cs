using System.Collections.Generic;

namespace VirtualTexture.Runtime
{
    using System;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Experimental.Rendering;
    using UnityEngine.Assertions;
    
    public class VirtualTexture : IDisposable
    {
        /// <summary>
        /// Construct a Virtual Texture.
        /// </summary>
        public VirtualTexture(int tileWidth, int tileHeight, int pageCountX, int pageCountY, int tileCapacity, int mipCount, GraphicsFormat graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm)
        {
            Assert.IsTrue(Mathf.IsPowerOfTwo(tileWidth));
            Assert.IsTrue(Mathf.IsPowerOfTwo(tileHeight));
            Assert.IsTrue(Mathf.IsPowerOfTwo(pageCountX));
            Assert.IsTrue(Mathf.IsPowerOfTwo(pageCountY));
            Assert.IsTrue(tileCapacity < SystemInfo.maxTextureArraySlices);
            
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.pageCountX = pageCountX;
            this.pageCountY = pageCountY;
            this.tileCapacity = tileCapacity;
            this.graphicsFormat = graphicsFormat;
            this.virtualWidth = this.tileWidth * this.pageCountX;
            this.virtualHeight = this.tileHeight * this.pageCountY;
            
            // the minimum mip level should be one page. 
            int pageCountMin = Mathf.Min(pageCountX, pageCountY);
            int mipCountMax = Mathf.RoundToInt(Mathf.Log(pageCountMin, 2)) + 1;
            this.mipmapCount = Mathf.Clamp(mipCount, 1, mipCountMax);
            this.useMipMap = this.mipmapCount > 1;

            if (GraphicsFormatUtility.IsCompressedFormat(graphicsFormat))
            {
                this.supportsForDirectRendering = false;
            }
            else
            {
                RenderTextureFormat renderTextureFormat = GraphicsFormatUtility.GetRenderTextureFormat(graphicsFormat);
                this.supportsForDirectRendering = SystemInfo.SupportsRenderTextureFormat(renderTextureFormat);
            }

            _pageTable = new PageTable(this.pageCountX, this.pageCountY);
            _activeTiles = new Dictionary<int, ActiveTileInfo>(this.tileCapacity);
            _lruCache = new LRUCache(this.tileCapacity);
            InitializePageTexture();
            InitializePhysicsTexture();
        }

        /// <summary>
        /// Construct a Virtual Texture.
        /// </summary>
        public VirtualTexture(int tileSize, int pageLength, int tileCapacity, int mipCount,
            GraphicsFormat graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm) : this(tileSize, tileSize, pageLength,
            pageLength, tileCapacity, mipCount, graphicsFormat)
        {

        }

        /// <summary>
        /// Width of the whole Virtual Texture in pixels (Read Only).
        /// </summary>
        public readonly int virtualWidth;
        
        /// <summary>
        /// Height of the whole Virtual Texture in pixels (Read Only).
        /// </summary>
        public readonly int virtualHeight;

        /// <summary>
        /// Width of a tile in pixels (Read Only).
        /// </summary>
        public readonly int tileWidth;

        /// <summary>
        /// Height of a tile in pixels (Read Only).
        /// </summary>
        public readonly int tileHeight;

        /// <summary>
        /// Count of tiles in X direction on page table (Read Only).
        /// </summary>
        public readonly int pageCountX;

        /// <summary>
        /// Count of tiles in Y direction on page table (Read Only).
        /// </summary>
        public readonly int pageCountY;

        /// <summary>
        /// tile capacity of the virtual texture (Read Only).
        /// </summary>
        public readonly int tileCapacity;
        
        /// <summary>
        /// Mipmap count (Read Only).
        /// </summary>
        public readonly int mipmapCount;

        /// <summary>
        /// Virtual texture has mipmaps when this flag is set. (Read Only).
        /// </summary>
        public readonly bool useMipMap;

        /// <summary>
        /// Physics texture supports for direct rendering when this flag is set. (Read Only).
        /// </summary>
        public readonly bool supportsForDirectRendering;
        
        /// <summary>
        /// Dimensionality (type) of the Texture (Read Only).
        /// </summary>
        public TextureDimension dimension => TextureDimension.Tex2D;

        /// <summary>
        /// Returns the GraphicsFormat of Virtual Texture (Read Only).
        /// </summary>
        public readonly GraphicsFormat graphicsFormat;

        private string _name = Constants.DefaultName;
        private Texture2DArray _physicsTextureArray;
        private RenderTexture _physicsRenderTextureArray;
        private PageTable _pageTable;
        private LRUCache _lruCache;
        private Dictionary<int, ActiveTileInfo> _activeTiles;
        
        /// <summary>
        /// Whether Unity stores an additional copy of this texture's pixel data in CPU-addressable memory.
        /// </summary>
        public bool isReadable => false;
        
        /// <summary>
        /// The name of the object.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                Assert.IsTrue(_name.All(Char.IsLetter), "Virtual Texture name must be letters only.");
                this._name = value;
                PageTexture.name = _name + Constants.PageTextureSuffix;
                PhysicsTexture.name = _name + Constants.PhysicsTextureSuffix;
                PageTextureID = Shader.PropertyToID(PageTexture.name);
                PageTextureInfoID = Shader.PropertyToID(_name + Constants.PageTextureInfoSuffix);
                PhysicsTextureID = Shader.PropertyToID(PhysicsTexture.name);
                PhysicsTextureInfoID = Shader.PropertyToID(_name + Constants.PhysicsTextureInfoSuffix);
            }
        }
        
        internal int PageTextureID { get; private set; } = Shader.PropertyToID(Constants.DefaultName + Constants.PageTextureSuffix);
        internal int PageTextureInfoID { get; private set; } = Shader.PropertyToID(Constants.DefaultName + Constants.PageTextureInfoSuffix);
        internal int PhysicsTextureID { get; private set; } = Shader.PropertyToID(Constants.DefaultName + Constants.PhysicsTextureSuffix);
        internal int PhysicsTextureInfoID { get; private set; } = Shader.PropertyToID(Constants.DefaultName + Constants.PhysicsTextureInfoSuffix);
        internal RenderTexture PageTexture { get; private set; }
        internal Vector4 PageTextureInfo { get; private set; }
        internal Texture PhysicsTexture => supportsForDirectRendering ? _physicsRenderTextureArray : _physicsTextureArray;
        internal Vector4 PhysicsTextureInfo { get; private set; }

        public void Active(int x, int y, int mip)
        {
            int tileIndex = _pageTable.Get(x, y, mip);
            if (tileIndex == PageTable.InvalidTileIndex)
            {
                tileIndex = _lruCache.Require();
                if (_activeTiles.TryGetValue(tileIndex, out var info))
                {
                    _pageTable.Deactive(info.x, info.y, info.mip);
                }
                
                Active(tileIndex, x, y, mip);
            }
            else
            {
                _lruCache.Touch(tileIndex);
            }
        }
        
        public void Deactive(int x, int y, int mip)
        {
            int tileIndex = _pageTable.Get(x, y, mip);
            if (tileIndex != PageTable.InvalidTileIndex)
            {
                _pageTable.Deactive(x, y, mip);
                _activeTiles.Remove(tileIndex);
            }
        }

        public void DeactiveAll()
        {
            foreach (var info in _activeTiles.Values)
            {
                _pageTable.Deactive(info.x, info.y, info.mip);
            }
            
            _activeTiles.Clear();
            _lruCache.Reset();
        }
        
        private void Active(int tileIndex, int x, int y, int mip)
        {
            _activeTiles[tileIndex] = new ActiveTileInfo
            {
                x = x,
                y = y,
                mip = mip,
                tileIndex = tileIndex
            };
            
            _pageTable.Active(x, y, mip, tileIndex);
        }
        
        /// <summary>
        /// Count of tiles for a given mip level in X direction (Read Only).
        /// </summary>
        public int GetPageCountX(int mipLevel)
        {
            if (mipLevel >= mipmapCount) return 1;
            return pageCountX >> mipLevel;
        }
        
        /// <summary>
        /// Count of tiles for a given mip level in Y direction (Read Only).
        /// </summary>
        public int GetPageCountY(int mipLevel)
        {
            if (mipLevel >= mipmapCount) return 1;
            return pageCountY >> mipLevel;
        }
        
        /// <summary>
        /// Dispose the Virtual Texture.
        /// </summary>
        public void Dispose()
        {
            DestroyPageTexture();
            DestroyPhysicsTexture();
        }
        
        private void InitializePageTexture()
        {
            DestroyPageTexture();
            PageTexture = new RenderTexture(pageCountX, pageCountX, 0, GraphicsFormat.R8G8B8A8_UNorm, mipmapCount)
            {
                volumeDepth = 1,
                enableRandomWrite = true,
                useMipMap = true,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0,
                name = $"{_name}{Constants.PageTextureSuffix}"
            };
            
            PageTexture.useMipMap = true;
            PageTextureInfo = new Vector4(pageCountX, pageCountX, mipmapCount, 0);
        }

        private void InitializePhysicsTexture()
        {
            DestroyPhysicsTexture();
            int mipCount = useMipMap ? 2 : 1;
            FilterMode filterMode = useMipMap ? FilterMode.Trilinear : FilterMode.Bilinear;
            if (supportsForDirectRendering)
            {
                _physicsRenderTextureArray = new RenderTexture(tileWidth, tileHeight, 0, graphicsFormat, mipCount)
                {
                    dimension = TextureDimension.Tex2DArray,
                    volumeDepth = tileCapacity,
                    enableRandomWrite = false,
                    useMipMap = useMipMap,
                    autoGenerateMips = false,
                    filterMode = filterMode,
                    wrapMode = TextureWrapMode.Clamp,
                    anisoLevel = 0,
                    name = $"{_name}{Constants.PhysicsTextureSuffix}"
                };
            }
            else
            {
                _physicsTextureArray = new Texture2DArray(tileWidth, tileHeight, tileCapacity, graphicsFormat, TextureCreationFlags.DontInitializePixels, mipCount)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = filterMode,
                    anisoLevel = 0,
                    name = $"{_name}{Constants.PhysicsTextureSuffix}"
                };
            }
            
            PhysicsTextureInfo = new Vector4(tileWidth, tileHeight, tileCapacity, 0);
        }
        
        private void DestroyPageTexture()
        {
            if (PageTexture != null)
            {
                PageTexture.Release();
                PageTexture = null;
            }
        }
        
        private void DestroyPhysicsTexture()
        {
            if (PhysicsTexture != null)
            {
                if (supportsForDirectRendering)
                {
                    _physicsRenderTextureArray.Release();
                    _physicsRenderTextureArray = null;
                }
                else
                {
                    CoreUtils.Destroy(_physicsTextureArray);
                    _physicsTextureArray = null;
                }
            }
        }

        private struct ActiveTileInfo
        {
            public int x;
            public int y;
            public int mip;
            public int tileIndex;
        }
        
        private static class Constants
        {
            public const string DefaultName = "VirtualTexture";
            public const string PageTextureSuffix = "_Page";
            public const string PageTextureInfoSuffix = "_PageInfo";
            public const string PhysicsTextureSuffix = "_Physics";
            public const string PhysicsTextureInfoSuffix = "_PhysicsInfo";
        }
    }
}
