using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_EDITOR
class SpritePointFilter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (assetPath.Contains("Resources") && !assetPath.Contains("NP"))
        {
            TextureImporter textureImporter  = (TextureImporter)assetImporter;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.filterMode = FilterMode.Point;

            //enable read/write
            textureImporter.isReadable = true;


        }
    }
}
    #endif
