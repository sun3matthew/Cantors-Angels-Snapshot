using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Delta is a collection of MonoDeltas.
/// This is needed for User Delta Entities to check their status.
/// Is is just a list of MonoDeltas.
/// </summary>
public class Delta
{
    public List<MonoDelta> MonoDeltas { get; protected set; }
    public Delta(){
        MonoDeltas = new List<MonoDelta>();
    }
    public override string ToString()
    {
        string deltaString = "";
        foreach (MonoDelta monoDelta in MonoDeltas)
            deltaString += monoDelta?.ToString() + "\n";
        return deltaString;
    }
}

public class HistoryDelta : Delta
{
    public long ParentHash { get; private set; }
    public HistoryDelta(long hash, List<MonoDelta> monoDeltas){
        MonoDeltas = monoDeltas;
        ParentHash = hash;
    }

    public override string ToString() => "ParentHash: " + SaveUtility.ToHexString(ParentHash) + "\n" + base.ToString();
    public void Serialize(List<byte> bytes){
        SaveFile.WriteLong(bytes, ParentHash);
        SaveFile.WriteInt(bytes, MonoDeltas.Count);
        foreach(MonoDelta monoDelta in MonoDeltas)
            monoDelta.Serialize(bytes);
    }

    public static HistoryDelta Deserialize(byte[] bytes, ref int index){
        long hash = SaveFile.ReadLong(bytes, ref index);
        List<MonoDelta> monoDeltas = new();
        int numMonoDeltas = SaveFile.ReadInt(bytes, ref index);
        for(int i = 0; i < numMonoDeltas; i++)
            monoDeltas.Add(MonoDelta.Deserialize(bytes, ref index));
        return new HistoryDelta(hash, monoDeltas);
    }
}
