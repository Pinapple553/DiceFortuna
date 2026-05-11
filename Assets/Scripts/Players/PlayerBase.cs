using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PlayerBase : ScriptableObject
{
	public List<DiceData> ownedDiceList;
	public List<int> ownedItemsList;
	public List<DiceInstance> selectedDiceList;
	public int fortunaPoints;
	public int maxDice = 4;


	public int GetDiceAmount(DiceData dice)
	{
		int count = 0;
		foreach (var d in ownedDiceList)
		{
			if (d == dice) count++;
		}
		return count;
	}
	public int GetAvailableAmount(DiceData dice)
	{
		int owned = GetDiceAmount(dice);
		int active = DiceManager.Instance.activeDiceList.Count(i => i.data == dice);
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
}
