using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private DiceManager diceManager;
    [SerializeField] private MoneySystem moneySystem;
    [SerializeField] private UIManager ui;
    [SerializeField] private AIPlayer ai;

    private bool roundRunning;

    private void Start()
    {
        ui.Init(moneySystem);
    }

    public void StartRound()
    {
        if (roundRunning) return;

        BetData playerBet = ui.GetBet();

        if (!moneySystem.Spend(playerBet.amount))
            return;

        StartCoroutine(RoundRoutine(playerBet));
    }

    IEnumerator RoundRoutine(BetData playerBet)
    {
        roundRunning = true;

        BetData aiBet = ai.ChooseBet();

        yield return diceManager.Roll();

        List<int> results = diceManager.GetResults();

        bool win = DiceResultEvaluator.Evaluate(playerBet.betType, results);

        if (win)
            moneySystem.Add(playerBet.amount * 2);

        ui.ShowResult(win, playerBet.amount);
        ui.UpdateMoney();

        roundRunning = false;
    }
}