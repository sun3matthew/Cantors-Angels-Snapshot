using System.Collections.Generic;
using System.Diagnostics;

public enum ResourceType
{
    Spice,
    Faith,
}

public class Economy : UniversalAutoDeltaEntity
{
    private int[] resources;
    public new Entity Initialize()
    {
        Initialize(Economy);
        resources = new int[System.Enum.GetValues(typeof(ResourceType)).Length];

        // resources[(int)ResourceType.Spice] = 1000 * 4;
        // resources[(int)ResourceType.Faith] = 2000 * 4;

        // resources[(int)ResourceType.Spice] = 2000;
        // resources[(int)ResourceType.Faith] = 3000;

        resources[(int)ResourceType.Spice] = 1000;
        resources[(int)ResourceType.Faith] = 1500;
        return this;
    }

    protected override Entity Initialize(Entity clone)
    {
        base.Initialize(clone);
        Economy economy = (Economy)clone;
        resources = new int[economy.resources.Length];
        for (int i = 0; i < resources.Length; i++)
            resources[i] = economy.resources[i];
        return this;
    }
    public int GetResource(ResourceType resourceType) => resources[(int)resourceType];
    public void AddResource(ResourceType resourceType, int amount) => DeltaWrapper(MethodInfoUtil.GetMethodInfo<ResourceType, int>(_AddResource), resourceType, amount);
    protected void _AddResource(ResourceType resourceType, int amount) => resources[(int)resourceType] += amount;
    public override string ToString()
    {
        string economyString = base.ToString();
        economyString += "Resources:\n";
        for (int i = 0; i < resources.Length; i++)
            economyString += ((ResourceType)i).ToString() + ": " + resources[i] + "\n";
        return economyString;
    }
    protected override void ResolveUniversalAutoDelta()
    {
        // Count the number of Villages
        List<Village> villages = BoardState.GetEntities<Village>();
        int faith = 0;
        int spice = 0;
        foreach(Village village in villages){
            if (village.VillageType == VillageType.Faith)
                faith++;
            else if (village.VillageType == VillageType.Spice)
                spice += BoardState.GetEntity<Tile>(village.Position).SpiceLevel();
        }
        resources[(int)ResourceType.Faith] += faith;
        resources[(int)ResourceType.Spice] += spice;
    }
    public override int GetHash(ref int HashWrapper)
    {
        int hash = base.GetHash(ref HashWrapper);
        for (int i = 0; i < resources.Length; i++){
            hash ^= Shift(resources[i], 8, ref HashWrapper);
        }
        return hash;
    }
}