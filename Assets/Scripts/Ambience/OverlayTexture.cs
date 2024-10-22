using UnityEngine;
// using UnityEngine.UI

public class OverlayTexture : MonoBehaviour
{
    public static RenderTexture Texture;
    private static Material Material;

    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        if (Material == null)
            Material = new Material(Shader.Find("Sprites/Default"));
        // Debug.Log(Texture.width + " " + Texture.height + " => " + source.width + " " + source.height);
        Graphics.Blit(source, destination);
        Graphics.Blit(Texture, destination, Material);
    }
}
