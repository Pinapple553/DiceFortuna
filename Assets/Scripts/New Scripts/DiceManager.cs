using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class DiceManager : MonoBehaviour
{
    private PlayerController controller;

	public List<DiceData> diceList;
    private void Start()
    {
        ThrowDice();
    }
    private void Awake()
    {
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

    public void RemoveDice(DiceData dice)
    {
        if (diceList.Contains(dice))
        {
            diceList.Remove(dice);
            Debug.Log($"Removed dice: {dice.name}");
        }
        else
        {
            Debug.LogWarning($"Dice not found: {dice.name}");
        }

    }

}
