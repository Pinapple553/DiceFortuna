using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public int AiMoney = 100;
    public BetData ChooseBet()
    {
        BetType type = (BetType)Random.Range(0, 4);
        int amount = Random.Range(5, 25);

        return new BetData(type, amount);
    }
    public void UpdateMoney(bool win, int amount)
    {
        if (win)
        {
            AiMoney += amount * 2;
        }
        else
        {
            AiMoney -= amount;
        }
    }
}