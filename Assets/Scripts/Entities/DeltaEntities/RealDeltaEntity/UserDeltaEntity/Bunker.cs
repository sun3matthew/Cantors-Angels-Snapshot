using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bunker : AutoBuilding, IAutoDeltaEntity, ICrashLander
{
    protected override int MaxHealth() => 4;
    public override AnimE StateAnimation() => AnimE.Idle;
    public override bool SyncAnimation() => false;
    public override Entity Initialize(HexVector position)
    {
        base.Initialize(position);

        List<HexVector> hexRadius = HexVector.HexRadius(Position, 1);
        foreach(HexVector hex in hexRadius)
            BoardState.GetEntity<RealDeltaEntity>(hex)?.TakeDamage(2 - HexVector.Distance(Position, hex));

        ((ICrashLander)this).CrashLand(this, () => BoardRender.Instance.BounceGrid.AddBounceForce((GridVector)Position, -0.2f, 1, 2, 0.08f));
        return this;
    }
    public void ResolveAutoDelta()
    {
        Hadic closestEnemy = BoardState.GetClosestEntity<Hadic>(Position, 5);
        if(closestEnemy != null){
            BoardState.InjectMonoDelta(null, this);
            DeltaWrapper(MethodInfoUtil.GetMethodInfo<Hadic>(_ResolveAutoDelta), closestEnemy);
        }
    }
    public void _ResolveAutoDelta(Hadic closestEnemy) => closestEnemy?.TakeDamage(2);
}
