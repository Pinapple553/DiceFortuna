using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class DiceManager : MonoBehaviour
{
    private PlayerController controller;
    public List<DiceInstance> activeDiceList = new List<DiceInstance>();

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

    public bool AddDice(DiceData dice)
    {
        if (activeDiceList.Count >= 8) return false;
        activeDiceList.Add(new DiceInstance(dice));
        return true;
    }

    public bool RemoveDice(DiceData dice)
    {
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
    public List<int> ThrowDice()
    {
        List<int> results = new List<int>();
        foreach (var instance in activeDiceList)
        {
            int index = Random.Range(0, instance.data.sides.Length);
            instance.currentSideIndex = index;
            var result = instance.data.sides[index];
            result.effect?.Apply();
            results.Add(result.value);
        }
        return results;
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
}
