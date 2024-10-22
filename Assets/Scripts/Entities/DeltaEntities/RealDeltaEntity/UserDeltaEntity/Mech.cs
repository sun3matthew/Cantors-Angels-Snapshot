using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mech : UserDeltaEntity, ICrashLander
{
    protected class UnitAction{
        public string Name;
        public Sprite Icon;
        public Func<MonoDelta, DeltaMakerUI> ProcessAction;
        public bool Disabled;
        public UnitAction(string name, Sprite icon, Func<MonoDelta, DeltaMakerUI> processAction){
            Name = name;
            Icon = icon;
            ProcessAction = processAction;
            Disabled = false;
        }
    }
    public abstract int ImpactDamage();
    public override Entity Initialize(HexVector position)
    {
        base.Initialize(position);

        int impactDamage = ImpactDamage();
        List<HexVector> hexRadius = HexVector.HexRadius(Position, impactDamage);
        foreach(HexVector hex in hexRadius)
            BoardState.GetEntity<RealDeltaEntity>(hex)?.TakeDamage(impactDamage + 1 - HexVector.Distance(Position, hex));

        float forceScale = impactDamage / 3f;
        ((ICrashLander)this).CrashLand(this, () => BoardRender.Instance.BounceGrid.AddBounceForce((GridVector)Position, -0.6f * forceScale, 1, impactDamage + 1, 0.08f));
        return this;
    }
    protected override void ResolveMonoDelta(MonoDelta monoDelta){
        int readIdx = MonoDelta.ReadStart;
        int action = monoDelta.ReadInt(ref readIdx);
        if (action == 0){
            SetPosition(monoDelta.ReadHexVector(ref readIdx));
            return;
        }
        ResolveActionDelta(monoDelta, action, readIdx);
    }
    protected abstract void ResolveActionDelta(MonoDelta monoDelta, int action, int readIdx);
    protected virtual List<UnitAction> GetUnitActions(){
        return new List<UnitAction>(){
            new("Move", Resources.Load<Sprite>("UI/Button"), (MonoDelta monoDelta) => new TileSelectDeltaMaker(monoDelta, Color.green, this, (UserDeltaEntity userDeltaMaker) => RadiusGridInitializer(userDeltaMaker, userDeltaMaker.Stats[StatE.MoveSpeed], true))),
        };
    }
    protected static void DisableAction(List<UnitAction> unitActions, string actionName){
        foreach(UnitAction unitAction in unitActions){
            if (unitAction.Name == actionName){
                unitAction.Disabled = true;
                return;
            }
        }
    }

    public override MonoDelta InitActionTree(){
        Delta delta = GetDelta();

        if (HasActionDelta())
            return null;

        Actions = new List<Func<DeltaMakerUI>>();
        MonoDelta newMonoDelta = new(this);
        
        List<UnitAction> unitActions = GetUnitActions();
        if (delta.MonoDeltas.Count != 0) // has no action but has deltas so it has a move action, so you can't move again
            DisableAction(unitActions, "Move");

        List<Sprite> sprites = new();
        foreach(UnitAction unitAction in unitActions)
            sprites.Add(unitAction.Icon);

        List<bool> disabled = new();
        foreach(UnitAction unitAction in unitActions)
            disabled.Add(unitAction.Disabled);

        Actions.Add(() => new ButtonDeltaMaker(newMonoDelta, sprites, disabled));
        Actions.Add(() => ProcessActionDelta(newMonoDelta));
        return newMonoDelta;
    }
    protected bool HasMoved(){
        Delta delta = GetDelta();

        foreach (MonoDelta monoDelta in delta.MonoDeltas){
            int readIdx = MonoDelta.ReadStart;
            if (monoDelta.ReadInt(ref readIdx) == 0)
                return true;
        }
        return false;
    }
    protected bool HasActionDelta(){
        Delta delta = GetDelta();

        foreach (MonoDelta monoDelta in delta.MonoDeltas){
            int readIdx = MonoDelta.ReadStart;
            if (monoDelta.ReadInt(ref readIdx) != 0)
                return true;
        }
        return false;
    }

    //* This initialized the current action tree, the user steps it through the UI. Make sure it resets correctly if the user cancels
    private DeltaMakerUI ProcessActionDelta(MonoDelta monoDelta){
        int readIdx = MonoDelta.ReadStart;
        int actionIdx = monoDelta.ReadInt(ref readIdx);
        return GetUnitActions()[actionIdx].ProcessAction(monoDelta);
    }
    protected static bool[,] RadiusGridInitializer(UserDeltaEntity userDeltaMaker, int radius, bool ignoreUnits) => RadiusGridInitializer(userDeltaMaker, 0, radius, ignoreUnits);
    protected static bool[,] RadiusGridInitializer(UserDeltaEntity userDeltaMaker, int innerRadius, int outerRadius, bool ignoreUnits){
        int boardSize = Board.Instance.BoardSize;
        bool[,] Grid = new bool[boardSize, boardSize];
        List<HexVector> hexMovementRange = HexVector.HexRadius(userDeltaMaker.Position, outerRadius);
        foreach(HexVector hex in hexMovementRange){
            if (hex == userDeltaMaker.Position || (ignoreUnits && BoardState.GetEntity<RealDeltaEntity>(hex) != null))
                continue;
            GridVector grid = (GridVector)hex;
            Grid[grid.x, grid.y] = true;
        }
        if (innerRadius == 0)
            return Grid;

        List<HexVector> hexMovementRangeInner = HexVector.HexRadius(userDeltaMaker.Position, innerRadius);
        foreach(HexVector hex in hexMovementRangeInner){
            GridVector grid = (GridVector)hex;
            Grid[grid.x, grid.y] = false;
        }
        return Grid;
    }
}
