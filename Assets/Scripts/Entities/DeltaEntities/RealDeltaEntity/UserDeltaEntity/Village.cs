using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VillageType
{
    Null,
    Spice,
    Faith,
}
public class Village : AutoBuilding
{
    protected override int MaxHealth() => 1;
    public override AnimE StateAnimation() => AnimE.Idle;
    public override bool SyncAnimation() => false;

    public VillageType VillageType { get; private set; } = VillageType.Null;

    public override bool CanCreateEntity(HexVector position)
    {
        if (base.CanCreateEntity(position)){
            return CheckNeighbors(position);
        }
        return false;
    }
    public override Entity Initialize(HexVector position)
    {
        base.Initialize(position);
        if (BoardState.GetEntity<Tile>(position).SpiceLevel() >= 1)
            VillageType = VillageType.Spice;
        else
            VillageType = VillageType.Faith;
        return this;
    }
    private static bool CheckNeighbors(HexVector position){
        if (!CheckNeighborCount(position, 0))
            return false;
        HexVector[] neighbors = HexVector.Neighbors(position);
        foreach (HexVector neighbor in neighbors){
            Village village = BoardState.GetEntity<Village>(neighbor);
            if (village != null && !CheckNeighborCount(neighbor, 1))
                return false;
        }
        return true;
    }
    private static bool CheckNeighborCount(HexVector position, int buffer){
        int emptyCount = -buffer;
        HexVector[] neighbors = HexVector.Neighbors(position);
        foreach (HexVector neighbor in neighbors)
            if (BoardState.GetEntity<Building>(neighbor) == null)
                emptyCount++;

        GridVector gridPosition = (GridVector)position;
        return emptyCount >= Board.Instance.Elevation[gridPosition.x, gridPosition.y];
    }
    protected override Entity Initialize(Entity clone)
    {
        Village village = (Village)base.Initialize(clone);
        VillageType = village.VillageType;
        return village;
    }
    public void SetVillageType(VillageType villageType) => VillageType = villageType;
}
