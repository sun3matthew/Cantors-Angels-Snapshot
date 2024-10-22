using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TileSelectDeltaMaker : DeltaMakerUI
{
    private Color TileColor;
    private List<SelectedTile> SelectedTiles;
    private UserDeltaEntity UserDeltaEntity;
    private Func<UserDeltaEntity, bool[,]> GridInitializer;
    public TileSelectDeltaMaker(MonoDelta monoDelta, Color tileColor, UserDeltaEntity userDeltaEntity, Func<UserDeltaEntity, bool[,]> gridInitializer) : base(monoDelta){
        TileColor = new Color(tileColor.r, tileColor.g, tileColor.b, 0.15f);
        SelectedTiles = new List<SelectedTile>();
        GridInitializer = gridInitializer;
        UserDeltaEntity = userDeltaEntity;
    }
    public override void CreateDeltaUI(){
        bool[,] grid = GridInitializer.Invoke(UserDeltaEntity);
        for(int x = 0; x < grid.GetLength(0); x++)
            for(int y = 0; y < grid.GetLength(1); y++)
                if(grid[x, y])
                    SelectedTiles.Add(Entity.Get<SelectedTile>().Initialize((HexVector)new GridVector(x, y), TileColor) as SelectedTile);
    }
    public override void Destroy(){
        foreach(SelectedTile selectedTile in SelectedTiles)
            selectedTile.RemoveUniversalRenderer();
    }
    public override bool Resolve(){
        for (int i = 0; i < SelectedTiles.Count; i++)
            SelectedTiles[i].UniversalRenderer.UpdateRender();
        if(Input.GetMouseButtonDown(0)){
            HexVector mousePos = CoreLoop.MouseHexPos();
            foreach(SelectedTile selectedTile in SelectedTiles){
                if(selectedTile.Position == mousePos){
                    MonoDelta.Write(selectedTile.Position);
                    return true;
                }
            }
        }
        return false;
    }
}
