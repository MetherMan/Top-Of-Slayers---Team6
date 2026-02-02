public interface IChainCombo
{
    int CurrentChain { get; }
    void Update(float deltaTime);
    void ChainUp();
    void EndChain();
}