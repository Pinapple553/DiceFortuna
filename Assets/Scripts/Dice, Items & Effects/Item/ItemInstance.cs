[System.Serializable]
public class ItemInstance
{
    public ItemData data;
    public int currentTier;
    public int usesRemainingThisMatch;
    public ItemTier Tier => data.tiers[currentTier];
    public ItemInstance(ItemData data, int startingTier = 0)
    {
        this.data = data;
        this.currentTier = startingTier;
        ResetUses();
    }
    public void ResetUses()
    {
        usesRemainingThisMatch = Tier.uses;
    }
    public bool CanUse() => usesRemainingThisMatch > 0;
    public bool TryUse()
    {
        if (!CanUse()) return false;
        usesRemainingThisMatch--;
        return true;
    }
    public bool CanUpgrade() => currentTier < data.tiers.Length - 1;
    public void Upgrade()
    {
        if (CanUpgrade()) currentTier++;
    }
}