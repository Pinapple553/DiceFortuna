using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Rerolls all odd-valued dice on the board, up to diceTargets count (scales with tier).
[CreateAssetMenu(menuName = "Item/Effects/RerollOdd")]
public class RerollOddEffect : ItemEffect
{
    public override IEnumerator Apply(PlayerBase target, List<DiceInstance> dice, ItemTier tier, List<int> targetIndices)
    {
        // Gather indices of odd dice, limited by tier.diceTargets
        var oddIndices = new List<int>();
        for (int i = 0; i < dice.Count && oddIndices.Count < tier.diceTargets; i++)
        {
            int val = dice[i].data.sides[dice[i].currentSideIndex].value;
            if (val % 2 != 0) oddIndices.Add(i);
        }

        foreach (int i in oddIndices)
            DiceAnimation.Instance.Roll(dice[i]);

        yield break;
    }

    public override string GetDescription(ItemTier tier) =>
        $"Reroll up to {tier.diceTargets} odd {(tier.diceTargets == 1 ? "die" : "dice")} on the board. ({tier.uses} uses per match)";
}
