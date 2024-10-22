using System;
using System.Reflection;

// Entity that causes a change in the game state
// ! YOU SHOULD NEVER MODIFY A DELTA ENTITY DIRECTLY, ALWAYS CLONE.
public abstract class DeltaEntity : Entity
{
    protected void DeltaWrapper(MethodInfo delta) => DeltaWrapper(delta, null);
    protected void DeltaWrapper(MethodInfo delta, object parameter) => DeltaWrapper(delta, new object[]{parameter});
    protected void DeltaWrapper(MethodInfo delta, object parameter, object parameter2) => DeltaWrapper(delta, new object[]{parameter, parameter2});
    protected void DeltaWrapper(MethodInfo delta, object parameter, object parameter2, object parameter3) => DeltaWrapper(delta, new object[]{parameter, parameter2, parameter3});
    protected void DeltaWrapper(MethodInfo delta, object[] parameters){
        DeltaEntity clone = Clone<DeltaEntity>();
        delta.Invoke(clone, parameters);
        BoardState.UpdateEntity(this, clone);
    }

    protected T DeltaWrapper<T>(MethodInfo delta) => DeltaWrapper<T>(delta, null);
    protected T DeltaWrapper<T>(MethodInfo delta, object parameter) => DeltaWrapper<T>(delta, new object[]{parameter});
    protected T DeltaWrapper<T>(MethodInfo delta, object parameter, object parameter2) => DeltaWrapper<T>(delta, new object[]{parameter, parameter2});
    protected T DeltaWrapper<T>(MethodInfo delta, object parameter, object parameter2, object parameter3) => DeltaWrapper<T>(delta, new object[]{parameter, parameter2, parameter3});
    protected T DeltaWrapper<T>(MethodInfo delta, object[] parameters){
        DeltaEntity clone = Clone<DeltaEntity>();
        T result = (T)delta.Invoke(clone, parameters);
        BoardState.UpdateEntity(this, clone);
        return result;
    }
}
