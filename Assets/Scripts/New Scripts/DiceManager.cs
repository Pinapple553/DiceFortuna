using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DiceManager : MonoBehaviour
{
	[SerializeField] 
	private List<Dice> diceList;
	private void Start()
	{
		ThrowDice(diceList);
	}
	public void ThrowDice(List<Dice> diceList)
    {
        foreach (var dice in diceList)
        {
            int result = Random.Range(0, dice.GetSideCount()); // Simulate a dice throw (1-6)
            DiceSide[] diceSides = dice.GetSides();

            Debug.Log($"Dice {dice.name} rolled: {diceSides[result].name}");
		}
	}
}
