using UnityEngine;
using System;

public static class SpriteManager
{
    private static string[] SubFolders = new string[] { "Entities", "Tiles", "Misc" };
    private static Sprite[,][] sprites; //! faster, but could be pretty memory heavy, try to lower the number of AnimEs, reuse them.
    public static void Initialize(){
        sprites = new Sprite[Enum.GetValues(typeof(EntityEnum)).Length, Enum.GetValues(typeof(AnimE)).Length][];
        foreach(EntityEnum entity in Enum.GetValues(typeof(EntityEnum))){
            foreach(AnimE anim in Enum.GetValues(typeof(AnimE))){
                Sprite[] animation = null;
                foreach(string folder in SubFolders){
                    animation = Resources.LoadAll<Sprite>("Sprites/" + folder + "/" + entity.ToString() + "/" + anim.ToString());
                    if(animation.Length > 0)
                        break;
                }
                if(animation != null && animation.Length > 0)
                    sprites[(int)entity, (int)anim] = animation;
            }
        }

        PostProcess();
    }

    public static void PostProcess(){
        SpritePostProcess.ExpandDeltaEntities(sprites, 3);
        SpritePostProcess.HexTileLengthen(sprites, 256);
        PostProcessCreateAnim.CreateCrashAnimation(sprites);
    }

    public static Sprite[] GetAnimation(EntityEnum entityEnum, AnimE animEnum) => sprites[(int)entityEnum, (int)animEnum];
    public static Sprite GetSprite(EntityEnum entityEnum, AnimE animEnum, int frame){
        Sprite[] anim = sprites[(int)entityEnum, (int)animEnum];
        if (anim == null)
            Debug.LogError("No animation found for " + entityEnum + " " + animEnum);
        if(frame < anim.Length)
            return anim[frame];
        return null;
    }
}
