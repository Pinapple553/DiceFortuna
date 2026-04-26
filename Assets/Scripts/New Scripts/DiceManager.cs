using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DiceManager : MonoBehaviour
{
	[SerializeField] 
	private List<DiceData> diceList;
    private void Start()
    {
        ThrowDice(diceList);
    }
    public void ThrowDice(List<DiceData> diceList)
    {
        foreach (var dice in diceList)
        {
            int index = Random.Range(0, dice.sides.Length);
            var result = dice.sides[index];
            result.effect?.Apply();
            Debug.Log($"Dice {dice.name} rolled: {result.name}");
        }
    }
}
