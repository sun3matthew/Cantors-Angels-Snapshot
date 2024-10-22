using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class UserDeltaEntity : RealDeltaEntity, IManualDeltaEntity
{
    protected DeltaMakerUI CurrentDeltaMaker;
    protected List<Func<DeltaMakerUI>> Actions;

    public virtual bool CanSpawn() => true;

    public virtual bool CanCreateEntity(HexVector position) => BoardState.GetEntity<RealDeltaEntity>(position) == null;

    /// <summary>
    /// Called when the unit is selected during the player's turn.
    /// This is where the player can choose what action to take.
    /// </summary>
    /// <returns>Null if the unit has no actions to take, otherwise returns the MonoDelta being created</returns>
    public abstract MonoDelta InitActionTree();

    
    // Find the empty monoDeltaBind, and use it to create a complete monoDeltaBind.
    // * you can have multiple MonoDeltas, all associated together with 
    public void ResolveManualDelta(){
        MonoDeltaBind monoDeltaBind = BoardState.GetInjectedMonoDeltaBind();
        if (monoDeltaBind == null || monoDeltaBind.AssociatedEntity != this){
            Debug.LogError("MonoDeltaBind is null or already associated with another entity " + monoDeltaBind + "\n" + monoDeltaBind?.AssociatedEntity);
            return;
        }

        // update info
        monoDeltaBind.OverrideDelta(GetDelta());
        DeltaWrapper(MethodInfoUtil.GetMethodInfo<MonoDelta>(ResolveMonoDelta), monoDeltaBind.MonoDelta);
    }

    protected Delta GetDelta() => BoardState.FindMonoDeltaBind(ID)?.AssociatedDelta ?? new Delta();
    protected abstract void ResolveMonoDelta(MonoDelta monoDelta);
    public void CleanUpSelected(){
        if (CurrentDeltaMaker != null){
            CurrentDeltaMaker.Destroy();
            CurrentDeltaMaker = null;
        }
        Actions = null;
    }
    // 0 is fully done, 1 is in progress, 2 is done with the current action
    public int StepDeltaMaker(){
        if (Actions == null || Actions.Count == 0)
            return 0;
        
        if (CurrentDeltaMaker == null){
            CurrentDeltaMaker = Actions[0].Invoke();
            CurrentDeltaMaker.CreateDeltaUI();
        }

        if (CurrentDeltaMaker.Resolve()){
            Actions.RemoveAt(0); //! Do it here since first line checks for empty tree
            CurrentDeltaMaker.Destroy();
            CurrentDeltaMaker = null;
            return 2;
        }
        return 1;
    }
}
