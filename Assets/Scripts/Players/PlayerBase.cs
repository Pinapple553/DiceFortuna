using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PlayerBase : ScriptableObject
{
	public List<DiceData> ownedDiceList;
	public List<ItemData> ownedItemsList;    
	public List<DiceInstance> selectedDiceList = new List<DiceInstance>();

	[HideInInspector]
	public List<ItemInstance> itemHand = new List<ItemInstance>();

	public int fortunaPoints;
	public int maxDice = 4;
	public int maxItemsPerRound = 5;

	public int GetDiceAmount(DiceData dice)
	{
		int count = 0;
		foreach (var d in ownedDiceList)
			if (d == dice) count++;
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
	public void BuildItemHand()
	{
		itemHand = new List<ItemInstance>();
		foreach (var item in ownedItemsList)
		{
			itemHand.Add(new ItemInstance(item));
		}
	}

	public int ItemsUsedThisRound()
	{
		return itemHand.Count(i => i.used);
	}

	public bool CanUseItem()
	{
		return ItemsUsedThisRound() < maxItemsPerRound;
	}
	public bool TryUseItem(ItemInstance item)
	{
		if (item.used) return false;
		if (!CanUseItem()) return false;
		item.used = true;
		return true;
	}
}