using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    public List<DiceData> ownedDiceList;
    public List<int> ownedItemsList;

    public int money;

	public static Player Instance;
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			Instance = this;
		}
	}

	public int GetDiceAmount(DiceData dice){
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
}
