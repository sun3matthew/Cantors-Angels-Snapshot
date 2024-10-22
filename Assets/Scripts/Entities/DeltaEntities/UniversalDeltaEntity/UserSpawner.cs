using System;
using System.Collections.Generic;
using UnityEngine;

public class UserSpawner : UniversalDeltaEntity, IManualDeltaEntity
{
    //TODO Spawners set the ID of the newly spawned entities, so you don't have infinite growth.
    public new Entity Initialize(){
        base.Initialize(UserSpawner);
        return this;
    }

    public void CreateEntity(EntityEnum entityE, HexVector position){
        MonoDelta monoDelta = new(this);
        monoDelta.Write((int)entityE);
        monoDelta.Write(position);
        BoardState.InjectMonoDelta(monoDelta, this);
    }

    public void ResolveManualDelta()
    {
        // No need for a delta wrapper, since this is just a helper, no actual state change.
        MonoDelta monoDelta = BoardState.GetInjectedMonoDeltaBind().MonoDelta;
        int readIdx = MonoDelta.ReadStart;
        EntityEnum entityE = (EntityEnum)monoDelta.ReadInt(ref readIdx);
        HexVector position = monoDelta.ReadHexVector(ref readIdx);

        if (!CanCreateEntity(entityE, position)){
            // Debug.LogError("Save File Has Been Tampered With");
            return;
        }

        RealDeltaEntity entity = Get<RealDeltaEntity>(entityE);
        entity.Initialize(position);

        BoardState.AddEntity(entity);

        int[] cost = Cost(entityE, position);
        BoardState.GetEntity<Economy>(Economy).AddResource(ResourceType.Spice, -cost[0]);
        BoardState.GetEntity<Economy>(Economy).AddResource(ResourceType.Faith, -cost[1]);
    }

    public bool CanCreateEntity(EntityEnum entityE) => CanCreateEntity(entityE, new HexVector(0, 0));
    public bool CanCreateEntity(EntityEnum entityE, HexVector position){
        if (!Get<UserDeltaEntity>(entityE).CanCreateEntity(position))
            return false;

        Economy economy = BoardState.GetEntity<Economy>(Economy);
        int gold = economy.GetResource(ResourceType.Spice);
        int faith = economy.GetResource(ResourceType.Faith);
        int[] cost = Cost(entityE, position);
        return gold >= cost[0] && faith >= cost[1];
    }

    private int[] Cost(EntityEnum entityE, HexVector position){
        int[] cost = new int[2];
        if (entityE == EntityEnum.ArtilleryMech){
            cost[(int)ResourceType.Spice] = 400;
            cost[(int)ResourceType.Faith] = 300;
        } else if (entityE == EntityEnum.Village){
            int distanceFromNearestChurch = closestChurchDistance(position);
            cost[(int)ResourceType.Spice] = 16 * distanceFromNearestChurch;
            cost[(int)ResourceType.Faith] = 8 * distanceFromNearestChurch;
        } else if (entityE == EntityEnum.Church){
            cost[(int)ResourceType.Spice] = 500;
            cost[(int)ResourceType.Faith] = 1000;
        } else if (entityE == EntityEnum.BasicMech){
            cost[(int)ResourceType.Spice] = 300;
            cost[(int)ResourceType.Faith] = 300;
        }else if (entityE == EntityEnum.Bunker){
            cost[(int)ResourceType.Spice] = 16 * closestChurchDistance(position);
        }else if (entityE == EntityEnum.ScoutMech){
            cost[(int)ResourceType.Spice] = 250;
            cost[(int)ResourceType.Faith] = 100;
        }
        return cost;
    }
    private int closestChurchDistance(HexVector position){
        Church closestChurch = BoardState.GetClosestEntity<Church>(position);
        return closestChurch == null ? 10000 : HexVector.Distance(closestChurch.Position, position);
    }
}
