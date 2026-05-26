using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Effects/Reroll")]
public class RerollDiceEffect : ItemEffect
{
    public override IEnumerator Apply(PlayerBase target, List<DiceInstance> dice, ItemTier tier, List<int> targetIndices)
    {
        int count = Mathf.Min(targetIndices.Count, tier.diceTargets);

        for (int i = 0; i < count; i++)
        {
            int idx = targetIndices[i];
            if (idx < 0 || idx >= dice.Count) continue;
            DiceAnimation.Instance.Roll(dice[idx]);
        }

        yield return null;

        yield return new WaitUntil(() =>
        {
            for (int i = 0; i < count; i++)
            {
                int idx = targetIndices[i];
                if (idx >= 0 && idx < dice.Count && dice[idx].isRolling) return false;
            }
            return true;
        });
    }

    public override string GetDescription(ItemTier tier) =>
        $"Reroll up to {tier.diceTargets} {(tier.diceTargets == 1 ? "die" : "dice")}. ({tier.uses} usees per match)";
}