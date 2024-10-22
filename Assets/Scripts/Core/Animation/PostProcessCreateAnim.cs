using UnityEngine;
public static class PostProcessCreateAnim{
    public static void CreateCrashAnimation(Sprite[,][] sprites){
        foreach(EntityEnum entity in System.Enum.GetValues(typeof(EntityEnum))){
            // if entityE type is a deltaEntity, 
            if (entity == EntityEnum.Null)
                continue;
            System.Type entityType = Entity.EnumToType[entity];
            
            if(!typeof(ICrashLander).IsAssignableFrom(entityType))
                continue;

            Sprite idle = sprites[(int)entity, (int)AnimE.Idle][0];

            int numFrames = 50;
            int terminalVelocity = 80;
            int extendNum = (int)(terminalVelocity * 3f);
            Sprite[] crash = new Sprite[numFrames];
            for(int i = 0; i < numFrames; i++){
                Texture2D tex = idle.texture;
                Color[] pixels = tex.GetPixels();
                int newWidth = tex.width;
                int newHeight = tex.height + (int)((numFrames - i - 1) * terminalVelocity + extendNum) * 2;
                Color[] newPixels = new Color[newWidth * newHeight];
                for(int y = 0; y < tex.height; y++){
                    for(int x = 0; x < tex.width; x++){
                        int index = (newHeight - extendNum - tex.height + y) * newWidth + x;
                        newPixels[index] = pixels[y * tex.width + x];
                    }
                }

                for(int x = 0; x < newWidth; x++){
                    int maxY = 0;
                    for(int y = newHeight - 1; y >= 0; y--){
                        int index = y * newWidth + x;
                        if(newPixels[index].a > 0){
                            maxY = y;
                            break;
                        }
                    }

                    for(int y = 0; y < extendNum; y++){
                        int index = (maxY + y) * newWidth + x;
                        if (maxY + y < newHeight){
                            Color c = newPixels[maxY * newWidth + x];
                            c.a = 1 - (float)y / extendNum;
                            c.a /= 1.5f;
                            newPixels[index] = c;
                        }
                    }
                }

                Texture2D newTex = new(newWidth, newHeight)
                {
                    filterMode = FilterMode.Point
                };
                newTex.SetPixels(newPixels);
                newTex.Apply();

                crash[i] = Sprite.Create(newTex, new Rect(0, 0, newWidth, newHeight), new Vector2(0.5f, 0.5f), 100);
            }
            sprites[(int)entity, (int)AnimE.Crash] = crash;
        }
    }
}
