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

    private bool roundRunning = false;

    public void StartRound()
    {
        if (roundRunning)
        {
            DiceAnimation.Instance.SkipAll();
            return;
        }

        if (DiceManager.Instance.activeDiceList.Count == 0) return;

        BetData playerBet = UIManager.Instance.GetBet();

        if (!moneySystem.Spend(playerBet.amount))
        {
            return;
        }
        StartCoroutine(RoundRoutine());
    }

    private IEnumerator RoundRoutine()
    {
        roundRunning = true;
        UIManager.Instance.resetResult();
        
        DiceManager.Instance.ThrowDice();
        yield return StartCoroutine(EvaluateRoll());

        UIManager.Instance.UpdateUI();
        yield return new WaitForSeconds(0.5f);
        UIManager.Instance.ShowRollResults();

        roundRunning = false;
    }
    private IEnumerator EvaluateRoll()
    {
        while (DiceManager.Instance.IsAnyDiceRolling())
        {
            yield return null;
        }
        DiceManager.Instance.GetCurrentResult();
        yield break;
    }




    bool Evaluate(BetType bet, List<int> results)
    {
        int total = 0;
        foreach (var r in results)
        {
            total += r;
        }
        int maxResult = DiceManager.Instance.GetMaxResult();
        int minResult = DiceManager.Instance.GetMinResult();
        float highResult = (maxResult + minResult) / 2f;
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