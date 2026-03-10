using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiceManager : MonoBehaviour
{
    [SerializeField] public List<Dice> diceList;
    [SerializeField] private UIManager ui;
    [SerializeField] private AnimationRecorder anim;

    [Header("Dice Throw Parameters")]
    [SerializeField] private float rollForce = 6f;
    [SerializeField] private float rollTorque = 8f;
    [SerializeField] private float directionRandomness = 0.2f;
    [SerializeField] private float stableTimeRequired = 0.5f;

    [SerializeField] private Transform throwOrigin;
    [SerializeField] private Transform throwTarget;

    [Header("Rig")]
    [SerializeField] private int forcedValue = -1;


    private List<Dice> activeDice = new();
    private List<DiceFrame> recordedSimulations = new();

    private PlayerController controller;

    private void Awake()
    {
        controller = new PlayerController();
        controller.Dice.Roll.performed += ctx => StartCoroutine(RollRoutine());
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

    public bool AddDice(Dice dice)
    {
        if (diceList.Count < 9)
        {
            diceList.Add(dice);
            return true;
        }
        return false;
    }
    public bool RemoveDice(Dice dice)
    {
        for (int i = diceList.Count-1; i >= 0 ; i--)
        {
            if (diceList[i].diceType == dice.diceType)
            {
                diceList.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    public IEnumerator RollRoutine()
    {
        ResetDice(default);
        CreateDice();

        anim.StartSimulation(activeDice);

        List<DiceSimulation> sims = new();

        foreach (var dice in activeDice)
        {
            var sim = dice.Initialize(throwOrigin, throwTarget, rollForce, rollTorque, directionRandomness);
            sims.Add(sim);

            dice.ThrowDice(sim);
        }

        yield return new WaitUntil(AllDiceStopped);
        anim.StopRecording();

        foreach (var dice in activeDice)
        {
            dice.CalculateResult();
        }
        anim.ResetToInitialState();

        if (forcedValue > 0)
        {
            foreach (var dice in activeDice)
            {
                dice.RotateToFace(forcedValue);
            }
        }

        foreach (var dice in activeDice)
        {
            dice.GetComponent<Rigidbody>().isKinematic = true;
        }

        anim.PlayRecording();
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
        foreach (var dice in diceList)
        {
            var d = Instantiate(dice, throwOrigin.position, Random.rotation);
            d.SetStableTime(stableTimeRequired);
            activeDice.Add(d);
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
        foreach (var d in activeDice)
        {
            results.Add(d.GetTopSideValue());
        }
        return results;
    }
    public int GetMaxResult()
    {
        int max = 0;
        foreach (var d in diceList)
        {
            max+= d.MaxFaceValue();
        }
        return max;
    }
    public int GetMinResult()
    {
        int min = 0;
        foreach (var d in diceList)
        {
            min += d.MinFaceValue();
        }
        return min;
    }
}

