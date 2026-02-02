public class ChainComboSystem : IChainCombo
{
    public int CurrentChain { get; private set; }

    private float chainRemainTime;
    private float chainTimer;
    private bool isChain;

    public ChainComboSystem(float chainRemainTime)
    {
        this.chainRemainTime = chainRemainTime;
    }

    public void Update(float deltaTime)
    {
        if (!isChain) return;
        chainTimer -= deltaTime;

        if (chainTimer <= 0)
        {
            EndChain();
        }
    }

    //에너미 관련 스크립트에서 죽었을 때 메서드 적용
    public void ChainUp()
    {
        if (!isChain)
        {
            isChain = true;
            CurrentChain = 0;
        }

        CurrentChain++;
        chainTimer = chainRemainTime;
    }

    public void EndChain()
    {
        isChain = false;
        CurrentChain = 0;
    }
}
