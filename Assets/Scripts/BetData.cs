public struct BetData
{
    public BetType betType;
    public int amount;

    public BetData(BetType type, int amount)
    {
        betType = type;
        this.amount = amount;
    }
}