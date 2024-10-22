using UnityEngine;

public abstract class AutoBuilding : Building
{
    protected override void ResolveMonoDelta(MonoDelta monoDelta) {}
    public override MonoDelta InitActionTree() => null;
}
