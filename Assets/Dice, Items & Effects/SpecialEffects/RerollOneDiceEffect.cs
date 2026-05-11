using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Create asset: Right-click → Create → Item → Effects → Reroll One
// Rerolls one specific dice chosen by the player (pass index via GameManager.pendingItemTargetIndex).
[CreateAssetMenu(menuName = "Item/Effects/Reroll One")]
public class RerollOneDiceEffect : ItemEffect
{
    public override IEnumerator Apply(PlayerBase target, List<DiceInstance> dice)
    {
        int index = GameManager.Instance.pendingItemTargetIndex;
        if (index < 0 || index >= dice.Count) yield break;

        DiceAnimation.Instance.Roll(dice[index]);

        // Wait for the reroll animation to finish
        while (dice[index].isRolling)
            yield return null;
    }

    public override string GetDescription() => "Reroll one chosen die.";
}
