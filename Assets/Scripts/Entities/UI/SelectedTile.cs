using UnityEngine;

public class SelectedTile : Entity
{
    public UniversalRenderer UniversalRenderer;
    public void RemoveUniversalRenderer(){
        UniversalRenderer.Pool.Return(UniversalRenderer);
        UniversalRenderer = null;
    }
    public Entity Initialize(HexVector position, Color color)
    {
        base.Initialize(position, 0);

        UniversalRenderer = UniversalRenderer.Pool.Get();
        UniversalRenderer.BindTo(this);

        UniversalRenderer.SetOpacity(color.a);
        UniversalRenderer.SetColor(color);
        UniversalRenderer.UpdateRender();
        UniversalRenderer.SetPosition(1);
        return this;
    }
    public override bool SyncAnimation() => true;
    public override AnimE StateAnimation() => AnimE.Idle;
}
