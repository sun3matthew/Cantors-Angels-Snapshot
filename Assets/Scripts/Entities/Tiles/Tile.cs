using System.Collections.Generic;
using UnityEngine;

public abstract class Tile : Entity
{
    public abstract bool IsWalkable();
    public override bool SyncAnimation() => true;
    public override int CurrentFrame() => 0;
    public override AnimE StateAnimation() => AnimE.Idle;
    private float Shade;
    public override Entity Initialize(HexVector position)
    {
        base.Initialize(position);
        Shade = Random.Range(0.8f, 1.0f);
        return this;
    }
    protected override Entity Initialize(Entity clone)
    {
        base.Initialize(clone);
        Shade = ((Tile)clone).Shade;
        return this;
    }
    public override string ToString(){
        string entityString = base.ToString();
        Board board = Board.Instance;
        GridVector position = (GridVector)Position;
        entityString += "Mass Number: " + board.MassNumber[position.x, position.y] + "\n";
        entityString += "Elevation: " + board.Elevation[position.x, position.y] + "\n";
        entityString += "Shade: " + Shade + "\n";
        return entityString;
    }
    public override void UniversalRendererInit(UniversalRenderer UniversalRenderer, int idx){
        base.UniversalRendererInit(UniversalRenderer, idx);
        UniversalRenderer.SetColor(new Color(Shade, Shade, Shade));
    }
    public int SpiceLevel(){
        if (EntityEnum == EntityEnum.TundraTile)
            return 3;
        if (EntityEnum == EntityEnum.TemperateDesertTile)
            return 2;
        if (EntityEnum == EntityEnum.GinkgoForestTile)
            return 1;
        return 0;
    }
}
