using UnityEngine;
public abstract class UniversalDeltaEntity : DeltaEntity // I know this sucks.
{
    public static readonly HexVector HadicSpawner = (HexVector)new GridVector(-1, -1);
    public static readonly HexVector Economy = (HexVector)new GridVector(-2, -2);
    public static readonly HexVector UserSpawner = (HexVector)new GridVector(-3, -3);
    public override AnimE StateAnimation() => AnimE.Idle;
    public override bool SyncAnimation() => false;
}
