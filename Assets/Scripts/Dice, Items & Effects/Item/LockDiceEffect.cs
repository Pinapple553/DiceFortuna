using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Locks player-selected dice so they skip the next roll and keep their current value.
[CreateAssetMenu(menuName = "Item/Effects/LockDice")]
public class LockDiceEffect : ItemEffect
{
    public override IEnumerator Apply(PlayerBase target, List<DiceInstance> dice, ItemTier tier, List<int> targetIndices)
    {
        int count = Mathf.Min(targetIndices.Count, tier.diceTargets);
        for (int i = 0; i < count; i++)
        {
            int idx = targetIndices[i];
            if (idx < 0 || idx >= dice.Count) continue;
            dice[idx].isLocked = true;
        }

        UIManager.Instance.UpdateUI();
        yield break;
    }

    public override string GetDescription(ItemTier tier) =>
        $"Lock {tier.diceTargets} {(tier.diceTargets == 1 ? "die" : "dice")} — they keep their current value next roll. ({tier.uses} uses per match)";
}
