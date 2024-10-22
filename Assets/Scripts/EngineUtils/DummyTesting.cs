using UnityEngine;
using System;

public class DummyTesting
{
    public static bool Test()
    {
        bool status = true;
        #if UNITY_EDITOR

        // Check if all EntityE have a Entity Class Associated with them
        foreach (EntityEnum entityE in System.Enum.GetValues(typeof(EntityEnum)))
        {
            if (entityE == EntityEnum.Null)
                continue;

            Type type = Type.GetType(entityE.ToString());
            if (type == null){
                Debug.LogError("EntityE " + entityE + " does not have a Entity Class Associated with it");
                status = false;
            }
        }

        // Vise Versa
        foreach (Type type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.IsSubclassOf(typeof(Entity)) && !type.IsAbstract)
            {
                EntityEnum entityE;
                bool success = Enum.TryParse(type.Name, out entityE);

                if (!success){
                    Debug.LogError("Entity " + type.Name + " does not have a EntityE Associated with it");
                    status = false;
                }
            }
        }
        #endif
        return status;
    }
}


// #if UNITY_EDITOR
// using UnityEditor;
// #endif
// #if UNITY_EDITOR
// class SpritePointFilter : AssetPostprocessor
// {
//     void OnPreprocessTexture()
//     {
//         if (assetPath.Contains("Resources"))
//         {
//             TextureImporter textureImporter  = (TextureImporter)assetImporter;
//             textureImporter.spriteImportMode = SpriteImportMode.Single;
//             textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
//             textureImporter.filterMode = FilterMode.Point;
//         }
//     }
// }
//     #endif

