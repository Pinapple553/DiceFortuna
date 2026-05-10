using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.InputSystem.InputSettings;

public class GameManager : MonoBehaviour
{
    [SerializeField] private MoneySystem moneySystem;
    [SerializeField] private AIPlayer ai;

    public static GameManager Instance;

    public bool roundRunning = false;
    public bool diceRolling = false;

    [SerializeField] private int roundCost = 20;
    [SerializeField] private int winPayout = 40;

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
    }

    public void StartButtonClick()
    {
        if (!roundRunning)
        {
            StartRound();
        }
        UIManager.Instance.CloseRoundInfo(true);
    }

    public void GameButtonClick()
    {
        if (!DiceManager.Instance.diceSelected)
        {
            if (DiceManager.Instance.SelectDice())
            {
                UIManager.Instance.SetGameButtonText("Roll");
            }
        }
        else
        {
            Roll();
        }
    }

    public void StartRound()
    {
        if (!moneySystem.Spend(roundCost))
        {
            return;
        }
        roundRunning = true;
        UIManager.Instance.SetGameButtonText("Select");
        UIManager.Instance.UpdateUI();
    }
    private void Roll()
    {
        if (diceRolling)
        {
            DiceAnimation.Instance.SkipAll();
            return;
        }
        if (DiceManager.Instance.activeDiceList.Count == 0) return;
      
        StartCoroutine(RoundRoutine());
    }
    private IEnumerator RoundRoutine()
    {
        diceRolling = true;
        UIManager.Instance.resetResult();
        
        DiceManager.Instance.ThrowDice();
        yield return StartCoroutine(EvaluateRoll());

        UIManager.Instance.UpdateUI();
        yield return new WaitForSeconds(0.5f);
        UIManager.Instance.ShowRollResults();

        diceRolling = false;
    }
    private IEnumerator EvaluateRoll()
    {
        while (DiceManager.Instance.IsAnyDiceRolling())
        {
            yield return null;
        }
        yield return UIManager.Instance.CountAllDice();
        yield return DiceManager.Instance.CountAllBonuses();
        yield break;
    }


}