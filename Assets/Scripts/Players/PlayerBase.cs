using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PlayerBase : ScriptableObject
{
	public List<DiceData> ownedDiceList;
	public List<ItemInstance> ownedItemsList;   
	public List<DiceInstance> selectedDiceList = new List<DiceInstance>();

	public int totalFortunaPoints = 0;
	public int roundFortunaPoints = 0;
	public int matchFortunaPoints = 0;
	public int maxDice = 4;
	public int roundItemsUsed = 0;
	public int matchItemsUsed = 0;

	public bool CanUseItem() 
	{
		return roundItemsUsed < GameManager.Instance.maxRoundItems && matchItemsUsed < GameManager.Instance.maxMatchItems && ownedItemsList.Count() > 0;
	}
	public int GetDiceAmount(DiceData dice)
	{
		int count = 0;
		foreach (var d in ownedDiceList)
		if (d == dice) count++;
		return count;
	}
    public int GetSelectedDiceAmout(DiceData dice)
    {
        int amount = 0;
        foreach (var d in selectedDiceList)
        {
            if (d.data == dice) amount++;
        }
        return amount;
    }
    public int GetItemAmount(ItemData item)
    {
        int count = 0;
        foreach (var i in ownedItemsList)
            if (i.data == item) count++;
        return count;
    }
	public int GetAvailableItemAmount(ItemData item)
	{
        int owned = GetItemAmount(item);
        int active = DiceManager.Instance.activeDiceList.Count(i => i.data == item);
        return owned - active;
    }

	public bool AddDice(DiceData dice)
	{
		if (DiceManager.Instance.AddDice(dice, this))
		{
			ownedDiceList.Add(dice);
			return true;
		}
		return false;
	}
	public bool RemoveDice(DiceData dice)
	{
		if (DiceManager.Instance.RemoveDice(dice, this))
		{
			for (int i = ownedDiceList.Count - 1; i >= 0; i--)
			{
				if (ownedDiceList[i] == dice)
				{
					ownedDiceList.RemoveAt(i);
					return true;
				}
			}
		}
		return false;
	}

	public void ResetForNewMatch()
	{
        roundFortunaPoints = 0;
        matchFortunaPoints = 0;
        roundItemsUsed = 0;
        matchItemsUsed = 0;
        selectedDiceList.Clear();
    }
}