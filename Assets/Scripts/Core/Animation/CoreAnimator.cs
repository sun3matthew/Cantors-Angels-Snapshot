using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreAnimator : MonoBehaviour
{
    //private const float SrScale = 1.804221f * 2;
    public const float SrScale = 1.805f * 2;
    public const int Fps = 8;

    public SpriteRenderer Sr { get; private set; }
    private SpriteRenderer SrOutline;
    private Sprite[] Sprites;
    public EntityEnum EntityEnum { private get; set; }


    [SerializeField]
    private float TimeCounter;
    public int Frame { get; private set; }
    public AnimE CurrAnimEnum { get; private set; }

    public bool ToggleGrayScale { get; private set; }
    public bool CurrentAnimationSync;
    private AnimationSyncFloat CurrentAnimationSyncFloat;

    private Material DefaultMaterial;
    private Material GrayScaleMaterial;


    public void Instantiate(Entity entity){
        enabled = false;

        EntityEnum = entity.EntityEnum;
        Sprites = null;

        transform.localScale = new Vector3(SrScale, SrScale, 1);
        TimeCounter = 0;
        Frame = 0;
        CurrAnimEnum = AnimE.Null;
        CurrentAnimationSync = false;
        CurrentAnimationSyncFloat = null;
        
        if(Sr == null){
            Sr = gameObject.AddComponent<SpriteRenderer>();
            GameObject outline = new("Outline");
            outline.transform.SetParent(transform);
            outline.transform.localPosition = new Vector3(0, 0, 0);
            outline.transform.localScale = new Vector3(1, 1, 1);
            SrOutline = outline.AddComponent<SpriteRenderer>();
            SrOutline.sortingLayerName = "UI_Unit";
            SrOutline.material = Resources.Load<Material>("Misc/SpriteOutline");

            DefaultMaterial = Sr.material;
            GrayScaleMaterial = Resources.Load<Material>("Misc/GrayScaleShader");
        }
        SrOutline.sprite = null;

        Sr.color = Color.white;
        Sr.sprite = null;
        Sr.sortingLayerName = "Tile";
        Sr.sortingOrder = 1;
        SetGrayScale(false);
        SetSpriteOutline(false);
        SetOutlineColor(Color.white);
    }
    void Update()
    {
        if(Sprites == null)
            return;

        TimeCounter = CurrentAnimationSync ? CurrentAnimationSyncFloat.Counter : TimeCounter + Time.deltaTime;

        SetSprite();
        if(Sr.sprite == null)
        {
            TimeCounter = 0;
            SetSprite();
        }
    }
    // public void PlayAnim(AnimE newCurrentAnimEnum) => PlayAnim(newCurrentAnimEnum, AnimE.Null);
    public void PlayAnim(AnimE animE){
        if(CurrAnimEnum == animE)
            return;
        CurrAnimEnum = animE;
        Sprites = SpriteManager.GetAnimation(EntityEnum, CurrAnimEnum);
        TimeCounter = 0;
        enabled = true;
        CurrentAnimationSyncFloat = CurrentAnimationSync ? AnimationSync.GetCounter(EntityEnum, CurrAnimEnum) : null;
        SetSprite();
    }
    
    public void SampleAnim(AnimE animE, int frame){ //! bad hack, manually setted only the sr sprite...
        if (CurrAnimEnum == animE && Frame == frame)
            return;
        Sr.sprite = SpriteManager.GetSprite(EntityEnum, animE, frame);
        SrOutline.sprite = Sr.sprite;
        Frame = frame;
        CurrAnimEnum = animE;
    }
    public void SampleAnim(AnimE animE) => SampleAnim(animE, 0);
    public int GetAnimLength(AnimE animE) => SpriteManager.GetAnimation(EntityEnum, animE).Length;
    private void SetSprite(){
        Frame = (int)(Fps*TimeCounter);
        if(Frame >= Sprites.Length){
            Sr.sprite = null;
            SrOutline.sprite = null;
            return;
        }
        Sr.sprite = Sprites[Frame];
        SrOutline.sprite = Sprites[Frame];
    }

    public void SetGrayScale(bool grayScale){
        ToggleGrayScale = grayScale;
        Sr.material = ToggleGrayScale ? GrayScaleMaterial : DefaultMaterial;
    }
    public void SetSpriteOutline(bool outline) => SrOutline.enabled = outline;
    public void SetOutlineColor(Color color) => SrOutline.material.SetColor("_Color", color);
}