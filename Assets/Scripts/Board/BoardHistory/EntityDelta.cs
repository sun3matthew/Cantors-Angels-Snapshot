using UnityEngine;

public class MonoEntityDelta{ // The position with the entity that it changes from and to
    public HexVector Position { get; private set; }
    public Entity From { get; private set; }
    public Entity To { get; private set; }
    public MonoEntityDelta(HexVector position, Entity from, Entity to)
    {
        Position = position;
        From = from;
        To = to;
    }
}
public class EntityDelta
{
    public int EntityType { get; private set; }
    public MonoEntityDelta[] FromTo { get; private set; } // two monodeltas, for when you move to a different tile.
    public EntityDelta(int entityType, MonoEntityDelta from, MonoEntityDelta to)
    {
        EntityType = entityType;
        FromTo = new MonoEntityDelta[2];
        FromTo[0] = from;
        FromTo[1] = to;
    }
    public EntityDelta(int entityType, MonoEntityDelta fromTo)// when you don't move
    {
        EntityType = entityType;
        FromTo = new MonoEntityDelta[1];
        FromTo[0] = fromTo;
    }

    public override string ToString()
    {
        string entityDeltaString = "";
        entityDeltaString += "EntityType: " + EntityType + "\n";
        entityDeltaString += "FromTo: " + FromTo.Length + "\n";
        foreach(MonoEntityDelta monoEntityDelta in FromTo)
            entityDeltaString += monoEntityDelta.Position + " " + (monoEntityDelta.From != null ? monoEntityDelta.From.ToString().Replace("\n", " | ") : "null") + " -> " + (monoEntityDelta.To != null ? monoEntityDelta.To.ToString().Replace("\n", " | ") : "null") + "\n";
        return entityDeltaString;
    }
}