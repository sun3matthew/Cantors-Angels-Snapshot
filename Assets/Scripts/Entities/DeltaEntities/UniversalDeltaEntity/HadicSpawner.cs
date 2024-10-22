using System.Collections.Generic;
using UnityEngine;

public class HadicSpawner : UniversalAutoDeltaEntity
{
    private int SpawnCounter;
    private int SpawnSeed;
    private List<HadicGroup> HadicGroups;
    private class HadicGroup{
        public List<EntityEnum> HadicGroupList;
        public GridVector SpawnPosition;
        public GridVector SpawnDirection;
        public HadicGroup(List<EntityEnum> hadicGroupList, GridVector spawnPosition, GridVector spawnDirection){
            HadicGroupList = hadicGroupList;
            SpawnPosition = spawnPosition;
            SpawnDirection = spawnDirection;
        }
    }
    public Entity Initialize(int spawnSeed){
        Initialize(HadicSpawner);
        HadicGroups = new List<HadicGroup>();
        SpawnSeed = spawnSeed;
        // SpawnCounter = 3;
        SpawnCounter = 1;
        return this;
    }
    protected override Entity Initialize(Entity clone){
        base.Initialize(clone);
        HadicSpawner hadicSpawnerClone = (HadicSpawner)clone;
        SpawnCounter = hadicSpawnerClone.SpawnCounter;
        SpawnSeed = hadicSpawnerClone.SpawnSeed;

        HadicGroups = new List<HadicGroup>();
        foreach (HadicGroup hadicGroup in hadicSpawnerClone.HadicGroups){
            List<EntityEnum> hadicGroupList = new List<EntityEnum>();
            foreach (EntityEnum entityE in hadicGroup.HadicGroupList)
                hadicGroupList.Add(entityE);
            HadicGroups.Add(new HadicGroup(hadicGroupList, hadicGroup.SpawnPosition, hadicGroup.SpawnDirection));
        }

        return this;
    }

    protected override void ResolveUniversalAutoDelta()
    {
        SpawnCounter--;
        if(SpawnCounter <= 0){
            int turnNumber = Board.Instance.TurnNumberOf(BoardState);
            CoreRandom hadicRandom = new(SpawnSeed + turnNumber);
            SpawnCounter = hadicRandom.Next(10,30);
            
            // Use a better function for difficulty scaling
            // basically, different strategies and patterns.
            int difficulty = (turnNumber / 20) + 1;
            int getFckd = hadicRandom.Next(6);
            int count = hadicRandom.Next(difficulty);
            if (count == 0)
                count = 1;
            if (getFckd == 0){
                Debug.Log("Get f*cked");
                count *= 3;
            }

            if (count != 0){
                int boardSize = Board.Instance.BoardSize;
                int spawnPositionOffset = hadicRandom.Next(boardSize);
                int spawnChoice = hadicRandom.Next(4);
                GridVector spawnPosition = spawnChoice switch
                {
                    0 => new GridVector(spawnPositionOffset, 0),
                    1 => new GridVector(spawnPositionOffset, boardSize - 1),
                    2 => new GridVector(0, spawnPositionOffset),
                    _ => new GridVector(boardSize - 1, spawnPositionOffset),
                };

                GridVector spawnDirection = spawnChoice switch
                {
                    1 => new GridVector(1, 0),
                    2 => new GridVector(-1, 0),
                    3 => new GridVector(0, 1),
                    _ => new GridVector(0, -1),
                };

                List<EntityEnum> hadicGroup = GenerateHadicGroup(hadicRandom, count, difficulty);
                HadicGroups.Add(new HadicGroup(hadicGroup, spawnPosition, spawnDirection));
            }
        }

        for (int i = 0; i < HadicGroups.Count; i++){
            HadicGroup hadicGroup = HadicGroups[i];
            List<EntityEnum> hadicGroupList = hadicGroup.HadicGroupList;
            GridVector spawnPosition = hadicGroup.SpawnPosition;
            for(int failSafe = 0; failSafe < 1000; failSafe++){
                if (hadicGroupList.Count == 0)
                    break;
                EntityEnum entityE = hadicGroupList[0];
                hadicGroupList.RemoveAt(0);
                if (entityE == EntityEnum.Null)
                    break;

                GridVector individualSpawnPosition = spawnPosition + hadicGroup.SpawnDirection * failSafe/2 * (failSafe % 2 * 2 - 1);
                if (individualSpawnPosition.x < 0 || individualSpawnPosition.x >= BoardState.Board.BoardSize || individualSpawnPosition.y < 0 || individualSpawnPosition.y >= BoardState.Board.BoardSize)
                    continue;

                if (BoardState.GetEntity<RealDeltaEntity>((HexVector)individualSpawnPosition) == null)
                    BoardState.AddEntity(Get<Hadic>(entityE).Initialize((HexVector)individualSpawnPosition));
            }

            if (hadicGroupList.Count == 0){
                HadicGroups.RemoveAt(i);
                i--;
            }
        }
    }

    public List<EntityEnum> GenerateHadicGroup(CoreRandom hadicRandom, int count, int difficulty){
        List<EntityEnum> hadicGroup = new List<EntityEnum>();
        for (int i = 0; i < count; i++){
            EntityEnum entityE = hadicRandom.Next(6) switch
            {
                0 => EntityEnum.BruteHadic,
                1 => EntityEnum.SpeedHadic,
                2 => EntityEnum.SpeedHadic,
                3 => EntityEnum.Null,
                _ => EntityEnum.BasicHadic,
            };
            hadicGroup.Add(entityE);
        }
        return hadicGroup;
    }
    public override string ToString()
    {
        string entityString = base.ToString();
        entityString += "SpawnCounter: " + SpawnCounter + "\n";
        entityString += "SpawnSeed: " + SpawnSeed + "\n";
        return entityString;
    }
}
