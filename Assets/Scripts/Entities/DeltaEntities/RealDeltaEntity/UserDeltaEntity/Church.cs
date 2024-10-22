using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Church : AutoBuilding, IAutoDeltaEntity, ICrashLander
{
    protected override int MaxHealth() => 20;
    public override AnimE StateAnimation() => AnimE.Idle;
    public override bool SyncAnimation() => false;
    public int HealCounter { get; private set; }
    public override Entity Initialize(HexVector position)
    {
        base.Initialize(position);

        List<HexVector> hexRadius = HexVector.HexRadius(Position, 8);
        foreach(HexVector hex in hexRadius)
            BoardState.GetEntity<RealDeltaEntity>(hex)?.TakeDamage(9 - HexVector.Distance(Position, hex));
        HealCounter = 0;

        ((ICrashLander)this).CrashLand(this, () => BoardRender.Instance.BounceGrid.AddBounceImpact((GridVector)Position, 1.2f, 3, 7, 30, 0.06f));
        return this;
    }
    protected override Entity Initialize(Entity clone){
        base.Initialize(clone);
        HealCounter = ((Church)clone).HealCounter;
        return this;
    }
    public void ResolveAutoDelta()
    {
        if (Health >= MaxHealth())
            return;
        // Debug.Log("Heal");
        BoardState.InjectMonoDelta(null, this);
        DeltaWrapper(MethodInfoUtil.GetMethodInfo(_ResolveAutoDelta));
    }
    public void _ResolveAutoDelta()
    {
        HealCounter++;
        if (HealCounter == 8){
            HealCounter = 0;
            _Heal(1);
        }
    }
}
