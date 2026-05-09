using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class DiceManager : MonoBehaviour
{
    private PlayerController controller;
    public List<DiceInstance> activeDiceList = new List<DiceInstance>();

    public static DiceManager Instance;

    public int currentResult = 0;

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

    public bool AddDice(DiceData dice)
    {
        if(GameManager.Instance.roundRunning) return false;
        if (activeDiceList.Count >= 8) return false;
        activeDiceList.Add(new DiceInstance(dice));
        return true;
    }

    public bool RemoveDice(DiceData dice)
    {
        if (GameManager.Instance.roundRunning) return false;
        for (int i = activeDiceList.Count - 1; i >= 0; i--)
        {
            if (activeDiceList[i].data == dice)
            {
                activeDiceList.RemoveAt(i);
                return true;
            }
        }
        return false;
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
    public int GetCurrentResult()
    {
        int result = 0;
        foreach (var instance in activeDiceList)
        {
            result += instance.data.sides[instance.currentSideIndex].value;
        }
        currentResult = result;
        return result;
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
}
