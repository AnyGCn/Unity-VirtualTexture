namespace VirtualTexture.Runtime
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class PageTable
    {
        public const int InvalidTileIndex = 0;
        public PageTable(int pageCountX, int pageCountY)
        {
            Assert.IsTrue(Mathf.IsPowerOfTwo(pageCountX));
            Assert.IsTrue(Mathf.IsPowerOfTwo(pageCountY));
            this.pageCountX = pageCountX;
            this.pageCountY = pageCountY;
            this.mipmapCount = Mathf.RoundToInt(Mathf.Log(Mathf.Min(pageCountX, pageCountY), 2)) + 1;
            
            tables = new int[mipmapCount][][];
            for (int i = 0; i < mipmapCount; i++)
            {
                tables[i] = new int[pageCountY >> i][];
                for (int j = 0; j < pageCountY >> i; j++)
                {
                    tables[i][j] = new int[pageCountX >> i];
                }
            }
        }

        /// <summary>
        /// Count of pages in X axis (Read Only).
        /// </summary>
        public readonly int pageCountX;

        /// <summary>
        /// Count of pages in Y axis (Read Only).
        /// </summary>
        public readonly int pageCountY;

        public readonly int mipmapCount;
        
        private readonly int[][][] tables;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Active(int x, int y, int mip, int tileIndex)
        {
            tables[mip][y][x] = tileIndex;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deactive(int x, int y, int mip)
        {
            tables[mip][y][x] = InvalidTileIndex;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Get(int x, int y, int mip)
        {
            return tables[mip][y][x];
        }
    }
}
