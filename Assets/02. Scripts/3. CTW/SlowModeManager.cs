using UnityEngine;

public class SlowModeManager : ChainComboDecorator
{
    private int slowChain;
    private float slowScale;
    private float recoverSpeed = 3f;

    public SlowModeManager(IChainCombo inner, int slowChain, float slowScale) : base(inner)
    {
        this.slowChain = slowChain;
        this.slowScale = slowScale;
    }

    public override void ChainUp()
    {
        base.ChainUp();

        if (inner.CurrentChain >= slowChain)
        {
            Time.timeScale = slowScale;
        }
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        //슬로우 상태일 때 서서히 복구하는 로직 복원
        if (Time.timeScale < 1f)
        {
            Time.timeScale = Mathf.MoveTowards(Time.timeScale, 1.01f, Time.deltaTime * recoverSpeed);
        }

        if(Time.timeScale >= 1f)
        {
            EndChain();
        }
    }

    public override void EndChain()
    {
        Time.timeScale = 1f;
        base.EndChain();
    }
}
