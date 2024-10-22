using System.Collections.Generic;
using UnityEngine;

public class HealthGrid : IPoolable
{
    private static UniversalPool<HealthGrid> Pool;
    private static Transform PoolParent;
    public static void InitializePool(){
        PoolParent = new GameObject("HealthGrids").transform;
        Pool = new UniversalPool<HealthGrid>();
    }

    public static HealthGrid Get(int size, Transform parent, RealDeltaEntity boundEntity){
        HealthGrid healthGrid = Pool.Get();
        healthGrid.SetSize(size, parent, boundEntity);
        return healthGrid;
    }
    public static void Return(HealthGrid healthGrid) => Pool.Return(healthGrid);

    private int Size; // number of grids
    private List<SpriteRenderer> GridRenderers;
    private SpriteRenderer[] UsedRenders;
    private Transform Parent;
    private RealDeltaEntity BoundEntity;

    public void Instantiate(){
        Size = 0;
        GridRenderers = new List<SpriteRenderer>();
        UsedRenders = new SpriteRenderer[0];
        GameObject go = new("HealthGrid");
        go.transform.SetParent(PoolParent);
        Parent = go.transform;
    }


    private void SetSize(int size, Transform parent, RealDeltaEntity boundEntity){
        Size = size;

        BoundEntity = boundEntity;

        Parent.SetParent(parent);
        Parent.localPosition = new Vector3(0, 0, 0);
        Parent.localScale = new Vector3(1, 1, 1);

        if(size > GridRenderers.Count){
            for(int i = GridRenderers.Count; i < size; i++){
                GameObject grid = new("HealthGrid");
                grid.transform.SetParent(Parent);
                SpriteRenderer sr = grid.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "UI";
                sr.sortingOrder = 2;
                sr.sprite = Resources.Load<Sprite>("Misc/HealthGrid");

                GameObject fill = new("HealthFill");
                fill.transform.SetParent(grid.transform);
                fill.transform.localPosition = new Vector3(0, 0, 0);
                SpriteRenderer fillSr = fill.AddComponent<SpriteRenderer>();
                fillSr.sortingLayerName = "UI";
                fillSr.sortingOrder = 1;
                fillSr.sprite = Resources.Load<Sprite>("Misc/HealthFill");

                GridRenderers.Add(fillSr);
            }
        }
        UsedRenders = new SpriteRenderer[size];
        for (int i = 0; i < size; i++){
            UsedRenders[i] = GridRenderers[i];
            UsedRenders[i].transform.parent.gameObject.SetActive(true);
            UsedRenders[i].transform.parent.localPosition = new Vector3((i - size / 2) * 0.04f, 0, 0);
            UsedRenders[i].transform.parent.localScale = new Vector3(1, 1, 1);
        }
        UpdateRender();
    }
    public void ClearGrid(){
        for(int i = 0; i < Size; i++)
            UsedRenders[i].transform.parent.gameObject.SetActive(false);
    }

    public void UpdateRender(){
        if (BoundEntity.Health == BoundEntity.Stats[StatE.MaxHealth]){
            ClearGrid();
            return;
        }

        float health = (float)BoundEntity.Health / BoundEntity.Stats[StatE.MaxHealth];
        health *= Size;
        for(int i = 0; i < Size; i++){
            if(i < health){
                UsedRenders[i].enabled = true;
                if (i == Mathf.Floor(health))
                    UsedRenders[i].color = new Color(1, 0, 0, health - Mathf.Floor(health));
                else
                    UsedRenders[i].color = new Color(0, 1, 0, 1);
            }else{
                UsedRenders[i].color = new Color(1, 0, 0, 1);
            }
        }
    }
    public void Activate() {}
    public void Deactivate(){
        ClearGrid();
        Parent.SetParent(PoolParent);
    }
}
