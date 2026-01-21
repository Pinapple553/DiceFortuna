using System;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiceRoll : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    private PlayerController controller;

    private void Awake()
    {
        controller = new PlayerController();
        controller.Dice.Roll.performed += ctx => RollDice();
    }
    private void OnEnable()
    {
        controller.Dice.Enable();
    }
    private void OnDisable()
    {
        controller.Dice.Disable();
    }

    private Action<InputAction.CallbackContext> RollDice()
    {
        if (true)
        {

        }
        rb.AddForce(new Vector3(0,5,0), ForceMode.Impulse);
        return null;
    }
}
