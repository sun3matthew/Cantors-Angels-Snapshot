using UnityEngine;


public static class SpritePostProcess{

    //!!! BRO, (0,0) is bottom left
    public static void ExpandDeltaEntities(Sprite[,][] sprites, int nPixels){
        foreach(EntityEnum entity in System.Enum.GetValues(typeof(EntityEnum))){
            // if entityE type is a deltaEntity, 
            if (entity == EntityEnum.Null)
                continue;
            System.Type entityType = Entity.EnumToType[entity];
            if(!entityType.IsSubclassOf(typeof(DeltaEntity)))
                continue;

            foreach(AnimE anim in System.Enum.GetValues(typeof(AnimE))){
                Sprite[] animation = sprites[(int)entity, (int)anim];
                if(animation != null){
                    for(int i = 0; i < animation.Length; i++){
                        Texture2D tex = animation[i].texture;
                        Color[] pixels = tex.GetPixels();
                        int newWidth = tex.width + 2 * nPixels;
                        int newHeight = tex.height + 2 * nPixels;
                        Color[] newPixels = new Color[newWidth * newHeight];
                        // fill in the new pixels with the old ones
                        for(int x = 0; x < tex.width; x++){
                            for(int y = 0; y < tex.height; y++){
                                newPixels[(y + nPixels) * newWidth + x + nPixels] = pixels[y * tex.width + x];
                            }
                        }

                        Texture2D newTex = new(newWidth, newHeight)
                        {
                            filterMode = FilterMode.Point
                        };
                        newTex.SetPixels(newPixels);
                        newTex.Apply();

                        animation[i] = Sprite.Create(newTex, new Rect(0, 0, newWidth, newHeight), new Vector2(0.5f, 0.5f), 100);
                    }
                }
            }
        }
    }
    public static void HexTileLengthen(Sprite[,][] sprites, int nPixels){
        foreach(EntityEnum entity in System.Enum.GetValues(typeof(EntityEnum))){
            if (entity == EntityEnum.Null)
                continue;
            System.Type entityType = Entity.EnumToType[entity];
            if(!entityType.IsSubclassOf(typeof(Tile)))
                continue;
                
            foreach(AnimE anim in System.Enum.GetValues(typeof(AnimE))){
                Sprite[] animation = sprites[(int)entity, (int)anim];
                if(animation != null){
                    for(int i = 0; i < animation.Length; i++){
                        Texture2D tex = animation[i].texture;
                        Color[] pixels = tex.GetPixels();
                        int newHeight = tex.height + nPixels * 2;
                        Color[] newPixels = new Color[tex.width * newHeight];

                        // fill in the new pixels with the old ones
                        for(int x = 0; x < tex.width; x++){
                            for(int y = 0; y < tex.height; y++){
                                newPixels[(y + nPixels) * tex.width + x] = pixels[y * tex.width + x];
                            }
                        }

                        // hue shift
                        float shiftTowards = 0.05f;
                        for(int x = 0; x < tex.width; x++){
                            for(int y = 0; y < newHeight; y++){
                                if (newPixels[y * tex.width + x].a != 0){
                                    // Shift towards 0.1f
                                    float[] hsv = ToHSV(newPixels[y * tex.width + x]);
                                    if (hsv[0] > shiftTowards){
                                        float deltaHue = (shiftTowards - hsv[0]) / 1.07f;
                                        hsv[0] = (hsv[0] + deltaHue) % 1;
                                        newPixels[y * tex.width + x] = FromHSV(hsv);
                                    }
                                }
                            }
                        }

                        // add pixels to the bottom of the same color of the bottom most non null pixel or non transparent pixel
                        for(int x = 0; x < tex.width; x++){
                            int bottomRow = 0;
                            for(int y = 0; y < newHeight; y++){
                                if(newPixels[y * tex.width + x].a != 0){
                                    bottomRow = y;
                                    break;
                                }
                            }
                            int bottom = bottomRow - nPixels;
                            for(int y = bottom; y < bottomRow; y++){
                                // if its in the left half, then its should be slightly darker, if its in the right half, then it should be more darker
                                // darken more as it down the row
                                float darkenFactor = (float)(bottomRow - y) / nPixels;
                                darkenFactor *= 4;
                                darkenFactor = 1 - darkenFactor;
                                // hard cutoffs every 0.05f steps
                                darkenFactor = Mathf.Ceil(darkenFactor * 14) / 14;
                                float deltaHue;
                                if(x < tex.width / 2){
                                    darkenFactor = Mathf.Max(0.4f, 0.9f * darkenFactor);
                                    deltaHue = 0.03f;
                                }else{
                                    darkenFactor = Mathf.Max(0.3f, 0.8f * darkenFactor);
                                    deltaHue = -0.03f;
                                }

                                newPixels[y * tex.width + x] = Darken(newPixels[bottomRow * tex.width + x], darkenFactor);

                                float[] hsv = ToHSV(newPixels[y * tex.width + x]);
                                hsv[0] = hsv[0] + deltaHue;
                                if (hsv[0] < 0)
                                    hsv[0] = 1 + hsv[0];
                                if (hsv[0] > 1)
                                    hsv[0] = hsv[0] - 1;

                                newPixels[y * tex.width + x] = FromHSV(hsv);
                            }
                        }

                        // increase saturation
                        for(int x = 0; x < tex.width; x++){
                            for(int y = 0; y < newHeight; y++){
                                if (newPixels[y * tex.width + x].a != 0){
                                    float[] hsv = ToHSV(newPixels[y * tex.width + x]);
                                    hsv[1] = Mathf.Min(1, hsv[1] * 1.2f);
                                    // hsv[1] = hsv[1] * 0.8f;
                                    newPixels[y * tex.width + x] = FromHSV(hsv);
                                }
                            }
                        }

                        // export
                        Texture2D newTex = new(tex.width, newHeight)
                        {
                            filterMode = FilterMode.Point
                        };
                        newTex.SetPixels(newPixels);
                        newTex.Apply();

                        animation[i] = Sprite.Create(newTex, new Rect(0, 0, tex.width, newHeight), new Vector2(0.5f, 0.5f), 100);
                    }
                }
            }
        }
    }
    public static Sprite ScaleBy(Sprite sprite, float xScale, float yScale){
        Texture2D tex = sprite.texture;
        Color[] pixels = tex.GetPixels();
        int newWidth = (int)(tex.width * xScale);
        int newHeight = (int)(tex.height * yScale);
        Color[] newPixels = new Color[newWidth * newHeight];

        for(int x = 0; x < newWidth; x++)
            for(int y = 0; y < newHeight; y++)
                newPixels[y * newWidth + x] = pixels[(int)(y / yScale) * tex.width + (int)(x / xScale)];

        Texture2D newTex = new(newWidth, newHeight)
        {
            filterMode = FilterMode.Point
        };
        newTex.SetPixels(newPixels);
        newTex.Apply();

        return Sprite.Create(newTex, new Rect(0, 0, newWidth, newHeight), new Vector2(0.5f, 0.5f), 100);
    }
    private static float[] ToHSV(Color color){
        float H = 0;
        float S = 0;
        float V = 0;
        Color.RGBToHSV(color, out H, out S, out V);
        return new float[]{H, S, V};
    }
    private static Color FromHSV(float[] hsv){
        return Color.HSVToRGB(hsv[0], hsv[1], hsv[2]);
    }
    private static Color Darken(Color color, float factor) => new(color.r * factor, color.g * factor, color.b * factor, color.a);
}