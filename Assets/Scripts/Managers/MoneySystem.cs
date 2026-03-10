using UnityEngine;

public class MoneySystem : MonoBehaviour
{
    public int PlayerMoney { get; private set; } = 100;

    public bool Spend(int amount)
    {
        if (amount > PlayerMoney) return false;

        PlayerMoney -= amount;
        return true;
    }

    public void Add(int amount)
    {
        PlayerMoney += amount;
    }

    public void UpdateMoney(bool win, int amount)
    {
        if (win)
        {
            PlayerMoney += amount * 2;
        }
        else
        {
            PlayerMoney -= amount;
        }
    }
}