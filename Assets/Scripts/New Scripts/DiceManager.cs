using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class DiceManager : MonoBehaviour
{
    private PlayerController controller;
	public List<DiceData> diceList;

	public static DiceManager Instance;
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
		controller = new PlayerController();
        controller.Dice.Roll.performed += ctx => ThrowDice();
	}
    private void OnEnable()
    {
        controller.Dice.Enable();
    }
    private void OnDisable()
    {
        controller.Dice.Disable();
    }

    public void ThrowDice()
    {
        foreach (var dice in diceList)
        {
            int index = Random.Range(0, dice.sides.Length);
            var result = dice.sides[index];
            result.effect?.Apply();
            Debug.Log($"Dice {dice.name} rolled: {result.name}");
        }
    }

    public bool RemoveDice(DiceData dice)
    {
        if (diceList.Contains(dice))
        {
            for (int i = diceList.Count-1; i >= 0; i--)
            {
                if (diceList[i] == dice)
                {
                    diceList.RemoveAt(i);
                    return true;
                }
			}
            diceList.Remove(dice); //incase it breaks
            return true;
		}
        return false;
    }
    public bool AddDice(DiceData dice)
    {
        if (diceList.Count >= 8) return false;

        diceList.Add(dice);
        return true;
    }
    public List<int> GetResults()
    {
        List<int> results = new List<int>();    
        foreach (var dice in diceList)
        {
            int index = Random.Range(0, dice.sides.Length);
            var result = dice.sides[index];
            results.Add(result.value);
            result.effect?.Apply();
            Debug.Log($"Dice {dice.name} rolled: {result.name}");
        }
        return results;
	 }
}
