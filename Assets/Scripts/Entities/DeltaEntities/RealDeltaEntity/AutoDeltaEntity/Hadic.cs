using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public abstract class Hadic : RealDeltaEntity, IAutoDeltaEntity
{
    public override AnimE StateAnimation() => AnimE.Idle;
    public override bool SyncAnimation() => false;
    public HexVector? Target { get; private set; } // offset from target to get more variation, could result in splits.
    public HexVector? LandingZone { get; private set; }
    public override Entity Initialize(HexVector positionH){
        base.Initialize(positionH);
        Target = null;
        return this;
    }
    protected override Entity Initialize(Entity clone){
        base.Initialize(clone);
        Target = ((Hadic)clone).Target;
        LandingZone = ((Hadic)clone).LandingZone;
        return this;
    }
    public void ResolveAutoDelta()
    {
        BoardState.InjectMonoDelta(null, this);
        DeltaWrapper(MethodInfoUtil.GetMethodInfo(_ResolveAutoDelta));
    }
    private void _ResolveAutoDelta()
    {
        int moveSpeed = Stats[StatE.MoveSpeed];
        int attackRange = Stats[StatE.AttackRange];

        float startTime = Time.realtimeSinceStartup;

        // if target has moved or disappeared
        if (Target == null || BoardState.GetEntity<UserDeltaEntity>(Target.Value) == null){
            RemoveTarget();
            Target = GetWalkTo();
            if (Target == null)
                return;
        }

        // override target if there is a target in proximity
        if(!TunnelVision()){
            UserDeltaEntity closest = BoardState.GetClosestEntity<UserDeltaEntity>(Position, moveSpeed + 1);
            if (closest != null && closest.Position != Target){
                RemoveTarget();
                Target = closest.Position;
            }
        }

        if (CanAttack()){
            BoardState.GetEntity<RealDeltaEntity>(Target.Value)?.TakeDamage(Stats[StatE.AttackDamage]);
        }else{
            if (LandingZone == null){
                LandingZone = PathFinding.LandingZone(BoardState, Position, Target.Value);
                if (LandingZone != null){
                    LandingZone = PathFinding.NextUnoccupiedLandingZone(BoardState, BoardState.GetEntity<LandTile>(LandingZone.Value).LandingZone, 0);
                    BoardState.GetEntity<LandTile>(LandingZone.Value).UpdateLaunchPointOccupation(ID);
                }
            }
            PathFindTraverse();
            if (CanAttack())
                BoardState.GetEntity<RealDeltaEntity>(Target.Value)?.TakeDamage(Stats[StatE.AttackDamage]);
        }
    }
    private bool CanAttack() => Target != null && HexVector.Distance(Position, Target.Value) <= Stats[StatE.AttackRange];

    private void PathFindTraverse(){
        int moveSpeed = Stats[StatE.MoveSpeed];

        // DeltaEntity deltaEntity = null;

        ExposedLLNode<HexVector> Path = null;
        for (int i = 0; i < moveSpeed; i++){
            if (Path == null){
                if (Position == Target.Value){
                    RemoveTarget();
                    return;
                }

                Path = PathFinding.PathFind(BoardState, Position, Target.Value, ID);
                if (Path == null) return;
            }

            // clear landing zone
            if (Path.Value == LandingZone){ 
                BoardState.GetEntity<LandTile>(LandingZone.Value).UpdateLaunchPointOccupation(0);
                LandingZone = null;
            }

            // Can attack from here
            if (CanAttack())
                return;

            ExposedLLNode<HexVector> nextSpot = Path.Next;
            if (BoardState.GetEntity<DeltaEntity>(nextSpot.Value) != null){
                // localized pathFind to the next empty stop.
                for(int failSafe = 0; failSafe < 100; failSafe++){
                    // If the next spot is empty, or it's the target, then pathfind to it.
                    if (BoardState.GetEntity<DeltaEntity>(nextSpot.Value) == null || nextSpot.Value == Target.Value){
                        Path = PathFinding.LocalizedPathFind(BoardState, Position, Target.Value, 100);
                        if (Path == null){ // if you can't pathfind to the target, then find a new target.
                            HexVector targetPosition = Target.Value;
                            RemoveTarget();
                            Target = BoardState.NextEmptySpot(targetPosition);
                            if (Target == null) return; // never should happen
                            Path = PathFinding.LocalizedPathFind(BoardState, Position, Target.Value, 100);
                            if (Path == null) return;
                        }
                        nextSpot = Path.Next;
                        break;
                    }
                    nextSpot = nextSpot.Next;
                }
                
                // if the next spot is still blocked, then just stop
                if (nextSpot != null && BoardState.GetEntity<DeltaEntity>(nextSpot.Value) != null) return;
            }

            // Reached End of Path, find a new path
            Path = Path.Next;
            if (Path != null) SetPosition(Path.Value);
        }
    }
    private void RemoveTarget(){
        if (LandingZone != null)
            BoardState.GetEntity<LandTile>(LandingZone.Value).UpdateLaunchPointOccupation(0);
        LandingZone = null;
        Target = null;
    }
    protected abstract HexVector? GetWalkTo();
    protected abstract bool TunnelVision();
    protected abstract int AttackRange();
    protected abstract int AttackDamage();
    protected override void InitializeStats()
    {
        base.InitializeStats();
        Stats.Add(StatE.AttackRange, AttackRange());
        Stats.Add(StatE.AttackDamage, AttackDamage());
    }
    public override void Destroy()
    {
        base.Destroy();
        if (LandingZone != null)
            BoardState.GetEntity<LandTile>(LandingZone.Value).UpdateLaunchPointOccupation(0);
    }
    public override string ToString()
    {
        return base.ToString() + 
            "Target: " + Target + "\n" +
            "LandingZone: " + LandingZone + "\n";
    }
    public static long HashPath(ExposedLLNode<HexVector> Path){
        long hash = 0;
        ExposedLLNode<HexVector> node = Path;
        while (node != null){
            hash ^= SaveUtility.ShiftAndWrap((uint)node.Value.x, node.Value.y);
            node = node.Next;
        }
        return hash;
    }
}