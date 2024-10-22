public class NullTile : LandTile
{
    public override bool IsWalkable() => true;
    public override AnimE StateAnimation() => AnimE.Idle;
}

