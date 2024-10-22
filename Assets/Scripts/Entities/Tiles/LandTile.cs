using UnityEngine;
using System.Collections.Generic;

public abstract class LandTile : Tile
{
    public ExposedLLNode<HexVector> LandingZone { private set; get; }
    public ulong LaunchPointOccupied { private set; get; }
    public override Entity Initialize(HexVector position)
    {
        base.Initialize(position);
        LandingZone = null;
        LaunchPointOccupied = 0;
        return this;
    }
    protected override Entity Initialize(Entity clone)
    {
        base.Initialize(clone);
        LandTile landTile = (LandTile)clone;
        LandingZone = landTile.LandingZone;
        LaunchPointOccupied = landTile.LaunchPointOccupied;
        return this;
    }
    public void UpdateLaunchPointOccupation(ulong LaunchPointOccupied){
        LandTile NewLandTile = Clone<LandTile>();
        NewLandTile.LaunchPointOccupied = LaunchPointOccupied;
        BoardState.UpdateEntity(this, NewLandTile);
    }
    public override bool IsWalkable() => true;

    public override string ToString(){
        string entityString = base.ToString();
        entityString += "Launch Point: " + LandingZone?.Value + "\n";
        entityString += "Launch Point Occupied: " + LaunchPointOccupied + "\n";
        return entityString;
    }

    public void SetLaunchPoint(ExposedLLNode<HexVector> launchPoint) => LandingZone ??= launchPoint;
}

