// OnHitAffix.cs
public class OnHitAffix : Affix
{
    public OnHitEffect Effect { get; }

    public OnHitAffix(string id, string name, AffixTier tier, OnHitEffect effect)
        : base(id, name, tier)
    {
        Effect = effect;
    }
}
