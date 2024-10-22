using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RealDeltaEntity : DeltaEntity
{
    public Dictionary<StatE, int> Stats { get; private set; }
    public int Health { get; private set; }

    protected virtual bool CanPush => true;

    public override Entity Initialize(HexVector position)
    {
        base.Initialize(position);
        InitializeStats();
        Health = MaxHealth();
        return this;
    }

    protected override Entity Initialize(Entity clone){
        base.Initialize(clone);
        Health = ((RealDeltaEntity)clone).Health;
        //! sync stats
        InitializeStats();
        return this;
    }

    protected abstract int MaxHealth();
    protected abstract int MoveSpeed();
    protected virtual void InitializeStats(){
        Stats = new Dictionary<StatE, int>();
        Stats.Add(StatE.MaxHealth, MaxHealth());
        Stats.Add(StatE.MoveSpeed, MoveSpeed());
    }

    public void TakeDamage(int damage) => DeltaWrapper(MethodInfoUtil.GetMethodInfo<int>(_TakeDamage), damage);
    private void _TakeDamage(int damage){
        Health -= damage;
        if (Health <= 0)
            Destroy();
    }

    public bool Push(HexVector direction) => DeltaWrapper<bool>(MethodInfoUtil.GetMethodInfo<HexVector, bool>(_Push), direction);
    protected bool _Push(HexVector direction){
        if (!CanPush)
            return false;
        HexVector target = Position + direction;

        Tile startTile = BoardState.GetEntity<Tile>(Position);
        Tile endTile = BoardState.GetEntity<Tile>(target);

        RealDeltaEntity targetEntity = BoardState.GetEntity<RealDeltaEntity>(target);
        if (targetEntity != null || startTile.GetElevation() < endTile.GetElevation() || !endTile.IsWalkable()){
            targetEntity?.TakeDamage(1);
            _TakeDamage(1);
            return false;
        }
        SetPosition(target);
        return true;
    }

    public void Heal(int heal) => DeltaWrapper(MethodInfoUtil.GetMethodInfo<int>(_Heal), heal);
    protected void _Heal(int heal){
        Health += heal;
        if (Health > Stats[StatE.MaxHealth])
            Health = Stats[StatE.MaxHealth];
    }

    //! Fake ID from board.
    public override void UniversalRendererInit(UniversalRenderer universalRenderer, int idx)
    {
        base.UniversalRendererInit(universalRenderer, idx);
        universalRenderer.SetPosition(idx == Board.Instance.WorkingBoard ? 3 : 2);
        if (idx == Board.Instance.WorkingBoard)
            universalRenderer.AddHealthGrid(Stats[StatE.MaxHealth], this);
    }

    public override int GetHash(ref int HashWrapper) => base.GetHash(ref HashWrapper)
        ^ Shift(Health, 4, ref HashWrapper);


    public override string ToString(){
        string entityString = base.ToString();
        entityString += "Health: " + Health + "\n";
        foreach (KeyValuePair<StatE, int> stat in Stats)
            entityString += stat.Key.ToString() + ": " + stat.Value + "\n";
        return entityString;
    }

}
