using UnityEngine;

public class MoneySystem : MonoBehaviour
{
    public bool Spend(int amount)
    {
        if (amount > Player.Instance.money) return false;

        Player.Instance.money -= amount;
        return true;
    }

    public void Add(int amount)
    {
		Player.Instance.money += amount;
    }

    public void UpdateMoney(bool win, int amount)
    {
        if (win)
        {
			Player.Instance.money += amount * 2;
        }
        else
        {
			Player.Instance.money -= amount;
        }
    }
}