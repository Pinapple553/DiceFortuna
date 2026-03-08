using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiceManager : MonoBehaviour
{
    [SerializeField] private List<Dice> dice;

    [Header("Dice Throw Parameters")]
    [SerializeField] private float rollForce = 6f;
    [SerializeField] private float rollTorque = 8f;
    [SerializeField] private float directionRandomness = 0.2f;
    [SerializeField] private float stableTimeRequired = 0.5f;

    [SerializeField] private Transform throwOrigin;
    [SerializeField] private Transform throwTarget;

    [Header("Dice Prefabs")]
    [SerializeField] private List<Dice> dicePrefabs = new();

    private List<Dice> activeDice = new();
    private List<DiceSimulation> recordedSimulations = new();

    private PlayerController controller;

    private void Awake()
    {
        controller = new PlayerController();
        controller.Dice.Roll.performed += RollAllDice;
        controller.Dice.Reset.performed += ResetDice;
    }

    private void OnEnable()
    {
        controller.Dice.Enable();
    }

    private void OnDisable()
    {
        controller.Dice.Disable();
    }

    private void RollAllDice(InputAction.CallbackContext ctx)
    {
        StartCoroutine(RollRoutine());
    }
    public IEnumerator Roll()
    {
        StartCoroutine(RollRoutine());

        yield return new WaitUntil(AllDiceStopped);
    }

    IEnumerator RollRoutine()
    {
        ResetDice(default);

        CreateDice();

        recordedSimulations.Clear();

        foreach (var dice in activeDice)
        {
            var sim = dice.Initialize(throwOrigin, throwTarget, rollForce, rollTorque, directionRandomness);
            recordedSimulations.Add(sim);
            dice.ThrowDice(sim);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitUntil(AllDiceStopped);

        foreach (var dice in activeDice)
        {
            dice.CalculateResult();
        }
    }

    private bool AllDiceStopped()
    {
        foreach (var d in activeDice)
        {
            if (!d.IsStopped)
            {
                return false;
            }
        }
        return true;
    }

    private void CreateDice()
    {
        foreach (var prefab in dicePrefabs)
        {
            var dice = Instantiate(prefab, throwOrigin.position, Random.rotation);
            dice.SetStableTime(stableTimeRequired);
            activeDice.Add(dice);
        }
    }

    private void ResetDice(InputAction.CallbackContext ctx)
    {
        foreach (var d in activeDice)
            Destroy(d.gameObject);

        activeDice.Clear();
    }

    public List<int> GetResults()
    {
        List<int> results = new();

        foreach (var d in dice)
        {
            results.Add(d.GetTopSideValue());

        }


        return results;
    }
}

