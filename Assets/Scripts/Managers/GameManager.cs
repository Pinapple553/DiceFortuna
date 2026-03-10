using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputSettings;

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
        if (diceManager.diceList.Count == 0) return;

        BetData playerBet = ui.GetBet();

        if (!moneySystem.Spend(playerBet.amount))
        {
            return;
        }

        StartCoroutine(RoundRoutine(playerBet));
    }

    IEnumerator RoundRoutine(BetData playerBet)
    {
        roundRunning = true;
        
        ui.resetResult();
        
        yield return diceManager.RollRoutine();
        List<int> results = diceManager.GetResults();

        bool win = Evaluate(playerBet.betType, results);
        moneySystem.UpdateMoney(win, playerBet.amount);
        ai.UpdateMoney(!win, playerBet.amount);

        ui.UpdateUI();
        ui.ShowResult(win, playerBet.amount);
       
        
        //ai round turn
        BetData aiBet = ai.ChooseBet();


        roundRunning = false;
    }
    bool Evaluate(BetType bet, List<int> results)
    {
        int total = 0;
        foreach (var r in results)
        {
            total += r;
        }
        int maxResult = diceManager.GetMaxResult();
        float hightResult = maxResult/2;
        switch (bet)
        {
            case BetType.Odd:
                return total % 2 == 1;

            case BetType.Even:
                return total % 2 == 0;

            case BetType.High:
                return total > hightResult;

            case BetType.Low:
                return total < hightResult;
        }
        return false;
    }
}