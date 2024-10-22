using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class UniversalRenderer : IPoolable
{

    private static Transform Parent;
    public static UniversalPool<UniversalRenderer> Pool;
    public static void InitializePool(){
        if(Parent == null)
            Parent = new GameObject("UniversalRenderers").transform;
        Pool = new UniversalPool<UniversalRenderer>();
        HealthGrid.InitializePool();
    }
    public Entity DataObject { get; private set; }
    private CoreAnimator Ca;
    private GameObject GameObject;
    private Transform Transform;

    private HealthGrid HealthGrid; //! I really don't like this design........
    
    public Func<UniversalRenderer, float, float> OverrideAnimation { get; private set;}
    public float OverrideCounter { get; private set;}

    public float Opacity { get; private set;}
    public int ShiftOffset { get; private set; }
    public float ZOffset { get; private set; }

    public const float max = 5;
    public void Instantiate(){
        GameObject = new GameObject(GetType().FullName);
        Transform = GameObject.transform;
        Transform.SetParent(Parent);

        DataObject = null;

        Ca = GameObject.AddComponent<CoreAnimator>();
    }

    public void BindTo(Entity dataObject){
        GameObject.name = dataObject.GetType().FullName;
        DataObject = dataObject;
        Ca.Instantiate(dataObject);
        SetPosition(ShiftOffset);

        Ca.CurrentAnimationSync = DataObject.SyncAnimation();
    }

    public void UpdateRender(){
        // Task<Vector3> task = CalculatePositionAsync();

        HealthGrid?.UpdateRender();

        if(OverrideAnimation != null){
            OverrideCounter = OverrideAnimation(this, OverrideCounter);
            if(OverrideCounter <= 0){
                OverrideAnimation = null;
                OverrideCounter = -1;
            }
        }else{
            if(DataObject.CurrentFrame() != -1){
                Ca.SampleAnim(DataObject.StateAnimation(), DataObject.CurrentFrame());
            }else if(Opacity < 1){//! Bad Hack{
                Ca.SampleAnim(DataObject.StateAnimation(), 0);
            }else{
                Ca.PlayAnim(DataObject.StateAnimation());
                // Ca.SetSpriteOutline(true);
            }
        }

        // task.Wait();
        Transform.position = CalculatePosition(DataObject, ShiftOffset);
    }

    public void SetGrayScale(bool grayScale){
        Ca.SetGrayScale(grayScale);
        Ca.Sr.material.SetColor("_Color", Color.white);
    }
    public void SetColor(Color color){
        color.a = Opacity;
        if (Ca.ToggleGrayScale){
            Ca.Sr.material.SetColor("_Color", color);
            return;
        }
        Ca.Sr.color = color;
    }
    public void SetOpacity(float opacity){
        Opacity = opacity;
        SetColor(Ca.Sr.color);
    }
    public void Activate(){
        Opacity = 1;
        ShiftOffset = 0;
        if (Ca != null && Ca.Sr != null)
            Ca.Sr.color = Color.white;
        GameObject.SetActive(true);
    }
    public void Deactivate(){
        GameObject.SetActive(false);

        if (HealthGrid != null){
            HealthGrid.Deactivate();
            HealthGrid = null;
        }

        OverrideAnimation = null;
        OverrideCounter = 0;
    }
    public bool IsActive() => GameObject.activeSelf;
    public void SetPosition(int offset){
        ShiftOffset = offset;   
        Transform.position = CalculatePosition(DataObject, offset);
    }
    public static Vector3 CalculatePosition(Entity dataObject, int offset){
        Vector2 pixelPosition = (Vector2)dataObject.Position;
        float bouncePosition = BoardRender.Instance.BounceGrid.GetBouncePosition(dataObject.GridPosition);
        return new Vector3(pixelPosition.x, pixelPosition.y + GetZOffset(dataObject.GridPosition) + bouncePosition, pixelPosition.y - offset * 0.01f);
    }
    public float GetZOffset() => GetZOffset(DataObject.GridPosition);
    public static float GetZOffset(GridVector gridPosition){
        float z = Time.time * 0.01f;
        return Board.Instance.Elevation[gridPosition.x, gridPosition.y] * 0.5f + (max * Mathf.PerlinNoise(gridPosition.x * 0.04f + z + Mathf.Sin(z), gridPosition.y * 0.04f + z + Mathf.Cos(z)) - max * 0.5f);
    }
    public void AddHealthGrid(int size, RealDeltaEntity boundEntity){
        HealthGrid = HealthGrid.Get(size, Transform, boundEntity);
    }
    public CoreAnimator GetCoreAnimator() => Ca;
    public void AddOverrideAnimation(Func<UniversalRenderer, float, float> overrideAnimation) => AddOverrideAnimation(overrideAnimation, 0);
    public void AddOverrideAnimation(Func<UniversalRenderer, float, float> overrideAnimation, float counter){
        OverrideAnimation = overrideAnimation;
        OverrideCounter = counter;
        UpdateRender();
    }
}
