using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text betText;

    private MoneySystem money;

    private int betAmount = 10;
    private BetType betType = BetType.Odd;

    public void Init(MoneySystem moneySystem)
    {
        money = moneySystem;
        UpdateMoney();
    }

    public void SetBetOdd() => betType = BetType.Odd;
    public void SetBetEven() => betType = BetType.Even;
    public void SetBetHigh() => betType = BetType.High;
    public void SetBetLow() => betType = BetType.Low;

    public void IncreaseAmount()
    {
        betAmount += 5;
        betText.text = $"Amount: {betAmount}";
    }

    public void DecreaseAmount()
    {
        betAmount = Mathf.Max(5, betAmount - 5);
        betText.text = $"Amount: {betAmount}";
    }

    public BetData GetBet()
    {
        return new BetData(betType, betAmount);
    }

    public void ShowResult(bool win, int amount)
    {
        if (win)
            resultText.text = $"WIN +{amount}";
        else
            resultText.text = $"LOSE -{amount}";
    }

    public void UpdateMoney()
    {
        moneyText.text = $"Money: {money.PlayerMoney}";
    }
}