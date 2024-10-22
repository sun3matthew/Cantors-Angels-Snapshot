using System.Collections.Generic;
using UnityEngine;

public class ScoutMech : Mech
{
    protected override int MaxHealth() => 3;
    protected override int MoveSpeed() => 4;
    public override AnimE StateAnimation() => AnimE.Idle;
    public override bool SyncAnimation() => false;
    public override int ImpactDamage() => 1;
    protected override List<UnitAction> GetUnitActions(){
        List<UnitAction> unitActions = base.GetUnitActions();
        unitActions.Add(new("Attack1", Resources.Load<Sprite>("UI/Button1"), (MonoDelta monoDelta) => new TileSelectDeltaMaker(monoDelta, Color.red, this, (UserDeltaEntity userDeltaMaker) => RadiusGridInitializer(userDeltaMaker, 1, false))));
        unitActions.Add(new("Attack2", Resources.Load<Sprite>("UI/Button2"), (MonoDelta monoDelta) => new TileSelectDeltaMaker(monoDelta, Color.red, this, (UserDeltaEntity userDeltaMaker) => RadiusGridInitializer(userDeltaMaker, 1, false))));
        return unitActions; 
    }
    // Do 1 damage to a unit
	// Push back target 4 tiles.
    protected override void ResolveActionDelta(MonoDelta monoDelta, int action, int readIdx){
        if (action == 1){
            HexVector target = monoDelta.ReadHexVector(ref readIdx);
            BoardState.GetEntity<RealDeltaEntity>(target)?.TakeDamage(1);
        } else if (action == 2){
            HexVector target = monoDelta.ReadHexVector(ref readIdx);
            HexVector direction = target - Position;
            RealDeltaEntity targetEntity = BoardState.GetEntity<RealDeltaEntity>(target);
            if (targetEntity != null){
                for (int i = 0; i < 4; i++){
                    if(!targetEntity.Push(direction))
                        break;
                    target += direction;
                    targetEntity = BoardState.GetEntity<RealDeltaEntity>(target);
                    if (targetEntity == null)
                        break;
                }
            }
        }
    }
}
