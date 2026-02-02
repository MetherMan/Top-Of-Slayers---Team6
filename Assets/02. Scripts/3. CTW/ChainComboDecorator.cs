public class ChainComboDecorator : IChainCombo
{
    protected IChainCombo inner;

    protected ChainComboDecorator(IChainCombo inner)
    {
        this.inner = inner;
    }

    public virtual int CurrentChain => inner.CurrentChain;
    public virtual void ChainUp() => inner.ChainUp();
    public virtual void Update(float deltaTime) => inner.Update(deltaTime);
    public virtual void EndChain() => inner.EndChain();
}
