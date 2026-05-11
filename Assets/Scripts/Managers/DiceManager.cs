using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance;
    private PlayerController controller;

    [SerializeField] private List<DiceCombo> combos;
    
    public List<DiceInstance> activeDiceList = new List<DiceInstance>();
    public List<DiceInstance> playerDiceList = new List<DiceInstance>();

    public int currentResult = 0;
    public bool diceSelected = false;

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

    public bool AddDice(DiceData dice, PlayerBase player)
    {
        if (!GameManager.Instance.roundRunning) return false;
        if (GameManager.Instance.diceRolling) return false;
        if (player.selectedDiceList.Count >= player.maxDice) return false;
		player.selectedDiceList.Add(new DiceInstance(dice));
        UIManager.Instance.UpdateUI();
        return true;
    }
    public bool RemoveDice(DiceData dice, PlayerBase player)
    {
        if (!GameManager.Instance.roundRunning) return false;
        if (GameManager.Instance.diceRolling) return false;
        for (int i = player.selectedDiceList.Count - 1; i >= 0; i--)
        {
            if (player.selectedDiceList[i].data == dice)
            {
                player.selectedDiceList.RemoveAt(i);
                UIManager.Instance.UpdateUI();
                return true;
            }
        }
        UIManager.Instance.UpdateUI();
        return false;
    }
    public bool SelectDice()
    {
        if (!GameManager.Instance.roundRunning) return false;
        if (diceSelected) return false;
        //AI select dice
        if (playerDiceList.Count == 0) return false;
        activeDiceList = new List<DiceInstance>(playerDiceList);
        diceSelected = true;
        UIManager.Instance.UpdateUI();
        return true;
    }

    public void ThrowDice()
    {
        foreach (var instance in activeDiceList)
        {
            DiceAnimation.Instance.Roll(instance);
        }
    }
    public bool IsAnyDiceRolling()
    {
        foreach (var instance in activeDiceList)
        {
            if (instance.isRolling) return true;
        }
        return false;
    }
    public int GetMaxResult()
    {
        int max = 0;
        foreach (var instance in activeDiceList)
        {
            int instanceMax = 0;
            foreach (var side in instance.data.sides)
            {
                if (side.value > instanceMax) instanceMax = side.value;
            }
            max += instanceMax;
        }
        return max;
    }
    public int GetMinResult()
    {
        int min = 0;
        foreach (var instance in activeDiceList)
        {
            int instanceMin = int.MaxValue;
            foreach (var side in instance.data.sides)
            {
                if (side.value < instanceMin) instanceMin = side.value;
            }
            min += instanceMin;
        }
        return min;
    }
    public List<int> GetAllValues()
    {
        List<int> values = new();
        foreach (var dice in activeDiceList)
        {
            values.Add(dice.data.sides[dice.currentSideIndex].value);
        }
        return values;
    }
    public IEnumerator CountAllBonuses()
    {
        List<int> values = GetAllValues();
        int totalBonus = 0;

        foreach (var combo in combos)
        {
            List<int> match = combo.GetMatchingIndices(values);
            if (match.Count > 0)
            {
                yield return UIManager.Instance.ShowCombo(match, combo);
                totalBonus += combo.GetBonus();
            }
        }

        currentResult += totalBonus;
    }
}
