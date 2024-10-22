using System.Collections.Generic;
using UnityEngine;

public class ArtilleryMech : Mech
{
    protected override int MaxHealth() => 2;
    protected override int MoveSpeed() => 1;
    public override AnimE StateAnimation() => AnimE.Idle;
    public override bool SyncAnimation() => false;

    public virtual int AttackRange() => 7;
    public override int ImpactDamage() => 3;

    public override Entity Initialize(HexVector position)
    {
        base.Initialize(position);

        return this;
    }
    protected override List<UnitAction> GetUnitActions(){
        List<UnitAction> unitActions = base.GetUnitActions();
        unitActions.Add(new("Attack1", Resources.Load<Sprite>("UI/Button1"), (MonoDelta monoDelta) => new TileSelectDeltaMaker(monoDelta, Color.red, this, (UserDeltaEntity userDeltaMaker) => RadiusGridInitializer(userDeltaMaker, 3, AttackRange(), false))));
        unitActions.Add(new("Attack2", Resources.Load<Sprite>("UI/Button2"), (MonoDelta monoDelta) => new TileSelectDeltaMaker(monoDelta, Color.red, this, (UserDeltaEntity userDeltaMaker) => RadiusGridInitializer(userDeltaMaker, 5, AttackRange() + 2, false))));
        return unitActions; 
    }

	// Do (hasMoved ? 2 : 4) damage to a unit max 12 tiles away, min 2 tiles away
    // Pull all entities around the target tile towards the tile
    protected override void ResolveActionDelta(MonoDelta monoDelta, int action, int readIdx){
        if (action == 1){
            HexVector target = monoDelta.ReadHexVector(ref readIdx);
            RealDeltaEntity targetEntity = BoardState.GetEntity<RealDeltaEntity>(target);
            if (targetEntity != null){
                int damage = HasMoved() ? 1 : 2;
                targetEntity.TakeDamage(damage);
            }
        } else if (action == 2){
            HexVector target = monoDelta.ReadHexVector(ref readIdx);
            List<HexVector> hexRadius = HexVector.HexRing(target, 1);
            foreach(HexVector hex in hexRadius)
                BoardState.GetEntity<RealDeltaEntity>(hex)?.Push(target - hex);
        }
    }
}
