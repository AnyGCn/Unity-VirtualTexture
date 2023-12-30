using UnityEngine.Assertions;

namespace VirtualTexture.Runtime
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PageTable
    {
        public PageTable(int pageCountX, int pageCountY)
        {
            Assert.IsTrue(Mathf.IsPowerOfTwo(pageCountX));
            Assert.IsTrue(Mathf.IsPowerOfTwo(pageCountY));
            this.pageCountX = pageCountX;
            this.pageCountY = pageCountY;
            this.mipmapCount = Mathf.RoundToInt(Mathf.Log(Mathf.Min(pageCountX, pageCountY), 2));
            
            tables = new Color32[mipmapCount][][];
            for (int i = 0; i < mipmapCount; i++)
            {
                tables[i] = new Color32[pageCountY >> i][];
                for (int j = 0; j < pageCountY >> i; j++)
                {
                    tables[i][j] = new Color32[pageCountX >> i];
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
        
        private readonly Color32[][][] tables;
    }
}
