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
        Debug.Log("Locked dice");
        return null;
    }

    public override string GetDescription() => "Reroll one chosen die.";
}
