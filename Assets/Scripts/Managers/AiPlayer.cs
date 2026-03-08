using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public BetData ChooseBet()
    {
        BetType type = (BetType)Random.Range(0, 4);
        int amount = Random.Range(5, 25);

        return new BetData(type, amount);
    }
}