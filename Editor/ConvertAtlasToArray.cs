
using UnityEngine.Experimental.Rendering;

namespace VirtualTexture.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEditor;
    using Unity.EditorCoroutines.Editor;

    public class ConvertAtlasToArray : EditorWindow
    {
        public Texture2D atlas;
        public int row;
        public int col;
        
        [MenuItem("Assets/VirtualTexture/Convert Atlas To Array")]
        static void Init()
        {
            Texture2D atlas = Selection.activeObject as Texture2D;
            if (atlas == null)
            {
                Debug.LogError("Please select a texture2D");
                return;
            }
            
            ConvertAtlasToArray window = (ConvertAtlasToArray)EditorWindow.GetWindow(typeof(ConvertAtlasToArray));
            window.Show();
            window.atlas = atlas;
            window.row = 1;
            window.col = 1;
        }
        
        void OnGUI()
        {
            atlas = (Texture2D)EditorGUILayout.ObjectField("原图集", atlas, typeof(Texture2D), false);
            row = EditorGUILayout.IntField("图集行数", row);
            col = EditorGUILayout.IntField("图集列数", col);
            if (GUILayout.Button("Convert"))
            {
                string assetPath = AssetDatabase.GetAssetPath(atlas);
                if (assetPath != null)
                {
                    assetPath = assetPath.Substring(0, assetPath.LastIndexOf('/'));
                    string textureArrayName = $"{assetPath}/{atlas.name}AtlasArray.asset";
                    EditorCoroutineUtility.StartCoroutine(SplitAtlasToArray(atlas, row, col, textureArrayName), this);
                }
            }
        }

        public IEnumerator SplitAtlasToArray(Texture2D src, int row, int col, string path)
        {
            int tileWidth = atlas.width / col;
            int tileHeight = atlas.height / row;
            int result = 0;
            TextureFormat compressFormat = Application.isMobilePlatform ? TextureFormat.ASTC_6x6 : TextureFormat.BC7;
            Texture2DArray array = new Texture2DArray(atlas.width / col, atlas.height / row, row * col, compressFormat, true);
            var cmd = CommandBufferPool.Get("SplitAtlasToArray");
            for (int x = 0; x < col; ++x)
            {
                for (int y = 0; y < row; ++y)
                {
                    RenderTexture rtTmp = RenderTexture.GetTemporary(tileWidth, tileHeight, 0, RenderTextureFormat.ARGB32,
                        RenderTextureReadWrite.Default);

                    int elementIndex = y * col + x;
                    cmd.Blit(src, rtTmp, new Vector2(1.0f / col, 1.0f / row), new Vector2(1.0f * x / col, 1.0f * y / row));
                    cmd.RequestAsyncReadback(rtTmp, (request) =>
                    {
                        if (request.hasError)
                        {
                            Debug.LogError("CopyTextureHelper: AsyncReadback has error");
                        }
                        else
                        {
                            var data = request.GetData<Color32>();
                            Texture2D tmpTexture2D = new Texture2D(tileWidth, tileHeight, TextureFormat.RGBA32, true);
                            tmpTexture2D.SetPixelData(data, 0);
                            tmpTexture2D.Apply(true, false);
                            EditorUtility.CompressTexture(tmpTexture2D, compressFormat, TextureCompressionQuality.Best);
                            Graphics.CopyTexture(tmpTexture2D, 0, array, elementIndex);
                            GameObject.DestroyImmediate(tmpTexture2D);
                        }

                        ++result;
                        RenderTexture.ReleaseTemporary(rtTmp);
                    });
                }
            }
            
            Graphics.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            while (result < row * col) yield return null;
            // array.Apply(true, false);
            AssetDatabase.CreateAsset(array, path);
        }
    }
}
