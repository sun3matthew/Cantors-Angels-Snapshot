public abstract class DeltaMakerUI
{
    protected MonoDelta MonoDelta { get; private set; }
    public DeltaMakerUI(MonoDelta monoDelta){
        MonoDelta = monoDelta;
    }
    public abstract bool Resolve();
    public abstract void CreateDeltaUI();
    public abstract void Destroy();
}
