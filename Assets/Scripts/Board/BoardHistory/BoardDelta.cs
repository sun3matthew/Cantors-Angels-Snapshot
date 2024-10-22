using System.Collections.Generic;
using UnityEngine;

public class BoardDelta
{
    public List<EntityDelta> EntityDeltas;
    public long Hash { get; private set; }
    // the deltas needed to make the hash.
    public BoardDelta(long hash, List<EntityDelta> entityDeltas)
    {
        Hash = hash;
        EntityDeltas = entityDeltas;
    }
    public override string ToString()
    {
        string boardDeltaString = "";
        boardDeltaString += "Hash: " + SaveUtility.ToHexString(Hash) + "\n";
        foreach(EntityDelta entityDelta in EntityDeltas)
            boardDeltaString += entityDelta?.ToString() + "\n";
        return boardDeltaString;
    }
}
