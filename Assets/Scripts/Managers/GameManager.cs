using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.InputSystem.InputSettings;

public class GameManager : MonoBehaviour
{
	public AIPlayer ai;
	public Player player;

	public static GameManager Instance;

    public bool roundRunning = false;
    public bool diceRolling = false;

    [SerializeField] private int roundLength = 3;
	int currentRound = 0;

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

    public void Start()
    {
        UIManager.Instance.CloseRoundInfo(false);
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
        //Submit Token
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
        if(currentRound < roundLength)
        {
			diceRolling = true;
			UIManager.Instance.resetResult();

			DiceManager.Instance.ThrowDice();
			yield return StartCoroutine(EvaluateRoll());

			UIManager.Instance.UpdateUI();
			yield return new WaitForSeconds(0.5f);
			UIManager.Instance.ShowRollResults();

            diceRolling = false;

			currentRound++;
        }
        else
		{
            DetermineWinner();
		}

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
    private void DetermineWinner()
    {
       
    }

}