using System.Collections.Generic;
using UnityEngine;

public class MonoDeltaBind{
    public MonoDeltaBind(MonoDelta monoDelta, DeltaEntity associatedEntity){
        MonoDelta = monoDelta;
        AssociatedEntity = associatedEntity;
        EntityDeltas = new List<EntityDelta>();
        SoftAssociatedEntities = new HashSet<DeltaEntity>();

        AssociatedDelta = new Delta();
        AssociatedDelta.MonoDeltas.Add(monoDelta);
    }
    public void OverrideDelta(Delta delta){
        AssociatedDelta = delta;
        if (!AssociatedDelta.MonoDeltas.Contains(MonoDelta))
            AssociatedDelta.MonoDeltas.Add(MonoDelta);
    }
    public void Delete(int turnNumber){
        foreach (DeltaEntity entityDelta in SoftAssociatedEntities)
            BoardRender.Instance.RemoveInitializedEntity(entityDelta.Position, turnNumber);
        AssociatedDelta.MonoDeltas.Remove(MonoDelta);   
    }
    //TODO Check and see if you delete the monoDeltaBind from delta.
    public Delta AssociatedDelta { get; private set; }
    public DeltaEntity AssociatedEntity { get; private set; }
    public HashSet<DeltaEntity> SoftAssociatedEntities { get; private set; }
    public MonoDelta MonoDelta { get; private set; }
    public List<EntityDelta> EntityDeltas { get; private set; }
    public override string ToString(){
        string deltaString = "";
        deltaString += "AssociatedDelta: " + AssociatedDelta.ToString() + "\n";
        deltaString += "AssociatedEntity: " + AssociatedEntity.ToString() + "\n";
        deltaString += "MonoDelta: " + MonoDelta?.ToString() + "\n";
        deltaString += "EntityDeltas: " + EntityDeltas.Count + "\n";
        for (int i = 0; i < EntityDeltas.Count; i++)
            deltaString += "EntityDelta " + i + ":\n" + EntityDeltas[i].ToString() + "\n";
        return deltaString;
    }
}