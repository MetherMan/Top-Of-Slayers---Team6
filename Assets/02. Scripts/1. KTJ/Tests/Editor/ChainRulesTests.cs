using NUnit.Framework;

public class ChainRulesTests
{
    [Test]
    public void RegisterHit_NewTargetsAdvanceChain_SameTargetDoesNot()
    {
        var rules = new ChainRules(0.1f);

        Assert.That(rules.RegisterHit(101).CurrentChain, Is.EqualTo(1));
        Assert.That(rules.RegisterHit(101).CurrentChain, Is.EqualTo(1));
        Assert.That(rules.RegisterHit(202).CurrentChain, Is.EqualTo(2));
        Assert.That(rules.RegisterHit(303).CurrentChain, Is.EqualTo(3));
    }

    [Test]
    public void GetDamageMultiplier_PreviewsNextTargetBeforeHit()
    {
        var rules = new ChainRules(0.1f);
        rules.RegisterHit(101);

        Assert.That(rules.GetDamageMultiplier(101), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(rules.GetDamageMultiplier(202), Is.EqualTo(1.1f).Within(0.0001f));
    }

    [Test]
    public void Reset_ClearsProgressionAndDamageBonus()
    {
        var rules = new ChainRules(0.25f);
        rules.RegisterHit(101);
        rules.RegisterHit(202);

        rules.Reset();

        Assert.That(rules.CurrentChain, Is.Zero);
        Assert.That(rules.GetDamageMultiplier(303), Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void NegativeDamageRate_IsClampedToZero()
    {
        var rules = new ChainRules(-1f);
        rules.RegisterHit(101);

        Assert.That(rules.GetDamageMultiplier(202), Is.EqualTo(1f).Within(0.0001f));
    }
}
