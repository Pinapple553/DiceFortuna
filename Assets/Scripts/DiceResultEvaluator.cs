using System.Collections.Generic;

public static class DiceResultEvaluator 
{
    public static bool Evaluate(BetType bet, List<int> results)
    {
        int total = 0;

        foreach (var r in results)
            total += r;

        switch (bet)
        {
            case BetType.Odd:
                return total % 2 == 1;

            case BetType.Even:
                return total % 2 == 0;

            case BetType.High:
                return total >= 10;

            case BetType.Low:
                return total <= 9;
        }

        return false;
    }
}