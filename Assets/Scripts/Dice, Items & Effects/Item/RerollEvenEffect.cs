using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Rerolls all even-valued dice on the board, up to diceTargets count (scales with tier).
[CreateAssetMenu(menuName = "Item/Effects/RerollEven")]
public class RerollEvenEffect : ItemEffect
{
    public override IEnumerator Apply(PlayerBase target, List<DiceInstance> dice, ItemTier tier, List<int> targetIndices)
    {
        var evenIndices = new List<int>();
        for (int i = 0; i < dice.Count && evenIndices.Count < tier.diceTargets; i++)
        {
            int val = dice[i].data.sides[dice[i].currentSideIndex].value;
            if (val % 2 == 0) evenIndices.Add(i);
        }

        foreach (int i in evenIndices)
            DiceAnimation.Instance.Roll(dice[i]);

        yield break;
    }

    public override string GetDescription(ItemTier tier) =>
        $"Reroll up to {tier.diceTargets} even {(tier.diceTargets == 1 ? "die" : "dice")} on the board. ({tier.uses} uses per match)";
}
