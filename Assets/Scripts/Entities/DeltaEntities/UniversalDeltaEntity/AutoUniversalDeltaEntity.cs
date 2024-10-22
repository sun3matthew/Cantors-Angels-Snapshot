using UnityEngine;
public abstract class UniversalAutoDeltaEntity : UniversalDeltaEntity, IAutoDeltaEntity
{
    public void ResolveAutoDelta()
    {
        BoardState.InjectMonoDelta(null, this);
        DeltaWrapper(MethodInfoUtil.GetMethodInfo(ResolveUniversalAutoDelta));
    }
    protected abstract void ResolveUniversalAutoDelta();
}
