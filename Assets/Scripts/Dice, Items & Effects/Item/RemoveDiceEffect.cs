using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Removes a player-selected die from the active board for the rest of the match.
// Will not remove the last remaining die.
[CreateAssetMenu(menuName = "Item/Effects/RemoveDice")]
public class RemoveDiceEffect : ItemEffect
{
    public override IEnumerator Apply(PlayerBase target, List<DiceInstance> dice, ItemTier tier, List<int> targetIndices)
    {
        // Safety: never remove the last die
        if (dice.Count <= 1) yield break;
        if (targetIndices.Count == 0) yield break;

        // Remove from highest index down so indices stay valid
        var toRemove = new List<int>(targetIndices);
        toRemove.Sort((a, b) => b.CompareTo(a));

        foreach (int idx in toRemove)
        {
            if (idx < 0 || idx >= dice.Count) continue;
            if (dice.Count <= 1) break; // never remove last
            dice.RemoveAt(idx);
        }

        UIManager.Instance.UpdateUI();
        yield break;
    }

    public override string GetDescription(ItemTier tier) =>
        $"Remove {tier.diceTargets} {(tier.diceTargets == 1 ? "die" : "dice")} from the board for the rest of the match. Cannot remove the last die. ({tier.uses} uses per match)";
}
