public class BasicHadic : Hadic
{
    protected override HexVector? GetWalkTo() => BoardState.GetClosestEntity<UserDeltaEntity>(Position)?.Position;
    protected override bool TunnelVision() => false;
    protected override int AttackRange() => 1;
    protected override int MaxHealth() => 6;
    protected override int MoveSpeed() => 4;
    protected override int AttackDamage() => 2;
}

public class SpeedHadic : Hadic
{
    protected override HexVector? GetWalkTo() => BoardState.GetClosestEntity<Building>(Position)?.Position;
    protected override bool TunnelVision() => true;
    protected override int AttackRange() => 1;
    protected override int MaxHealth() => 3;
    protected override int MoveSpeed() => 8;
    protected override int AttackDamage() => 1;
}

public class BruteHadic : Hadic
{
    protected override HexVector? GetWalkTo() => BoardState.GetClosestEntity<UserDeltaEntity>(Position)?.Position;
    protected override bool TunnelVision() => false;
    protected override int AttackRange() => 1;
    protected override int MaxHealth() => 8;
    protected override int MoveSpeed() => 4;
    protected override int AttackDamage() => 4;
}
