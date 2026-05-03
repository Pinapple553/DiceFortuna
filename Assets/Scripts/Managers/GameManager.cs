using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputSettings;

public class GameManager : MonoBehaviour
{
    [SerializeField] private MoneySystem moneySystem;
    [SerializeField] private AIPlayer ai;

    private bool roundRunning = false;

    public void StartRound()
    {
        if (roundRunning) return;
        if (DiceManager.Instance.activeDiceList.Count == 0) return;

        BetData playerBet = UIManager.Instance.GetBet();

        if (!moneySystem.Spend(playerBet.amount))
        {
            return;
        }
        RoundRoutine(playerBet);
    }

    private void RoundRoutine(BetData playerBet)
    {
        roundRunning = true;

        UIManager.Instance.resetResult();
        
        List<int> results = DiceManager.Instance.ThrowDice();

        bool win = Evaluate(playerBet.betType, results);
        moneySystem.UpdateMoney(win, playerBet.amount);
        ai.UpdateMoney(!win, playerBet.amount);

        UIManager.Instance.UpdateUI();
        UIManager.Instance.ShowResult(win, playerBet.amount);
       
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
        int maxResult = DiceManager.Instance.GetMaxResult();
        float highResult = maxResult/2;
        switch (bet)
        {
            case BetType.Odd:
                return total % 2 == 1;

            case BetType.Even:
                return total % 2 == 0;

            case BetType.High:
                return total > highResult;

            case BetType.Low:
                return total < highResult;
        }
        return false;
    }
}