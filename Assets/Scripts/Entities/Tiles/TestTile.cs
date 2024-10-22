using UnityEngine;

public class TestTile : LandTile
{
    public Color Color { get; private set; }
    private bool Walkable;
    public Entity Initialize(HexVector position, Color color, bool walkable)
    {
        base.Initialize(position);
        Color = color;
        Walkable = walkable;
        return this;
    }
    protected override Entity Initialize(Entity clone){
        base.Initialize(clone);
        Color = ((TestTile)clone).Color;
        Walkable = ((TestTile)clone).Walkable;
        return this;
    }

    public override bool IsWalkable() => Walkable; //! only for the purpose of testing
    public override AnimE StateAnimation() => AnimE.Idle;
    public override void UniversalRendererInit(UniversalRenderer UniversalRenderer, int idx){
        base.UniversalRendererInit(UniversalRenderer, idx);
        UniversalRenderer.SetColor(Color);
    }
    public void SetColor(Color color) => Color = color;

    public override string ToString(){
        string entityString = base.ToString();
        entityString += "Color: " + Color + "\n";
        entityString += "Walkable: " + Walkable + "\n";
        return entityString;
    }
}

